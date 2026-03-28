using Microsoft.ML;
using StockPredictor.Models;

namespace StockPredictor.Services;

public class BacktestService
{
    private readonly MLContext _mlContext;

    private static readonly string[] FeatureColumns =
    [
        nameof(StockData.Close),
        nameof(StockData.Volume),
        nameof(StockData.MA5),
        nameof(StockData.MA10),
        nameof(StockData.MA50),
        nameof(StockData.RSI),
        nameof(StockData.Volatility),
        nameof(StockData.Return),
        nameof(StockData.MACD),
        nameof(StockData.BollingerWidth),
        nameof(StockData.PriceToMA50),
        nameof(StockData.PriceToMA10),
        nameof(StockData.Momentum),
        nameof(StockData.Return5),
        nameof(StockData.VolumeMA5Ratio)
    ];

    public BacktestService()
    {
        _mlContext = new MLContext(seed: 0);
    }

    /// <summary>
    /// Walk-forward backtest: train on first <paramref name="trainPct"/>% of data,
    /// then simulate one trade per day on the remaining test set.
    /// </summary>
    public async Task<BacktestResult> RunAsync(
        List<RawStockRow> rawRows,
        double startingCapital = 10_000,
        double trainPct = 0.7)
    {
        return await Task.Run(() =>
        {
            var fullDataset = DatasetBuilder.BuildDataset(rawRows);

            if (fullDataset.Count < 20)
                throw new InvalidOperationException(
                    $"Only {fullDataset.Count} usable data points after warmup. Need at least 20.");

            int trainCount = (int)(fullDataset.Count * trainPct);
            trainCount = Math.Max(trainCount, 10);

            var trainSet = fullDataset.GetRange(0, trainCount);
            var testSet = fullDataset.GetRange(trainCount, fullDataset.Count - trainCount);

            if (testSet.Count < 2)
                throw new InvalidOperationException("Not enough test data to backtest.");

            // Train model on training set
            var trainData = _mlContext.Data.LoadFromEnumerable(trainSet);

            var pipeline = _mlContext.Transforms.Concatenate("Features", FeatureColumns)
                .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
                .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                    numberOfLeaves: 8,
                    numberOfTrees: 60,
                    minimumExampleCountPerLeaf: 3,
                    learningRate: 0.05));

            var model = pipeline.Fit(trainData);
            var engine = _mlContext.Model.CreatePredictionEngine<StockData, StockPrediction>(model);

            // Simulate trades on test set
            double equity = startingCapital;
            double peak = equity;
            double maxDrawdown = 0;
            var trades = new List<TradeRecord>();
            var equityCurve = new List<double> { equity };
            var dailyReturns = new List<double>();

            double grossWins = 0;
            double grossLosses = 0;
            int wins = 0;
            int losses = 0;
            double totalWinPct = 0;
            double totalLossPct = 0;

            for (int i = 0; i < testSet.Count - 1; i++)
            {
                var today = testSet[i];
                var tomorrow = testSet[i + 1];

                var prediction = engine.Predict(today);

                float entryPrice = today.Close;
                float exitPrice = tomorrow.Close;

                // Simple strategy: go long if UP, go short if DOWN
                double tradePct;
                string direction;

                if (prediction.PredictedLabel)
                {
                    // Predicted UP → buy today, sell tomorrow
                    direction = "LONG";
                    tradePct = (exitPrice - entryPrice) / entryPrice;
                }
                else
                {
                    // Predicted DOWN → short today, cover tomorrow
                    direction = "SHORT";
                    tradePct = (entryPrice - exitPrice) / entryPrice;
                }

                bool correct = tradePct > 0;
                equity *= (1 + tradePct);
                dailyReturns.Add(tradePct);

                if (tradePct > 0)
                {
                    wins++;
                    grossWins += tradePct * equity;
                    totalWinPct += tradePct;
                }
                else
                {
                    losses++;
                    grossLosses += Math.Abs(tradePct) * equity;
                    totalLossPct += Math.Abs(tradePct);
                }

                if (equity > peak) peak = equity;
                double drawdown = (peak - equity) / peak * 100;
                if (drawdown > maxDrawdown) maxDrawdown = drawdown;

                equityCurve.Add(equity);

                trades.Add(new TradeRecord
                {
                    Day = trainCount + i + 1,
                    Direction = direction,
                    EntryPrice = entryPrice,
                    ExitPrice = exitPrice,
                    ReturnPct = tradePct * 100,
                    Equity = equity,
                    Probability = prediction.Probability,
                    Correct = correct
                });
            }

            // Buy & hold comparison
            double buyHoldReturn = testSet.Count >= 2
                ? (testSet[^1].Close - testSet[0].Close) / testSet[0].Close * 100
                : 0;

            // Sharpe ratio (annualized, assuming 252 trading days)
            double sharpe = 0;
            if (dailyReturns.Count > 1)
            {
                double avgReturn = dailyReturns.Average();
                double stdDev = Math.Sqrt(dailyReturns.Select(r => Math.Pow(r - avgReturn, 2)).Average());
                if (stdDev > 0)
                    sharpe = avgReturn / stdDev * Math.Sqrt(252);
            }

            double profitFactor = grossLosses > 0 ? grossWins / grossLosses : grossWins > 0 ? double.PositiveInfinity : 0;

            return new BacktestResult
            {
                Trades = trades,
                EquityCurve = equityCurve,
                StartingCapital = startingCapital,
                FinalCapital = equity,
                BuyHoldReturnPct = buyHoldReturn,
                TotalTrades = trades.Count,
                Wins = wins,
                Losses = losses,
                MaxDrawdownPct = maxDrawdown,
                SharpeRatio = sharpe,
                ProfitFactor = profitFactor,
                AvgWinPct = wins > 0 ? totalWinPct / wins * 100 : 0,
                AvgLossPct = losses > 0 ? totalLossPct / losses * 100 : 0,
                TrainDays = trainCount,
                TestDays = testSet.Count
            };
        });
    }
}
