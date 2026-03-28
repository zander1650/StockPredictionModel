using StockPredictor.Models;

namespace StockPredictor.Services;

public static class DatasetBuilder
{
    // Minimum rows before indicators (MA50, MACD, Bollinger) are meaningful
    private const int WarmupPeriod = 50;

    public static List<StockData> BuildDataset(List<RawStockRow> rows)
    {
        var dataset = new List<StockData>();
        var prices = new List<float>();
        var volumes = new List<float>();

        for (int i = 0; i < rows.Count - 1; i++)
        {
            prices.Add(rows[i].Close);
            volumes.Add(rows[i].Volume);

            var ma5 = IndicatorCalculator.MovingAverage(prices, 5);
            var ma10 = IndicatorCalculator.MovingAverage(prices, 10);
            var ma50 = IndicatorCalculator.MovingAverage(prices, 50);
            var rsi = IndicatorCalculator.RSI(prices, 14);
            var volatility = IndicatorCalculator.Volatility(prices, 10);
            var ret = IndicatorCalculator.DailyReturn(
                rows[i].Close,
                rows[Math.Max(0, i - 1)].Close);

            var macd = IndicatorCalculator.MACD(prices);
            var bollingerWidth = IndicatorCalculator.BollingerBandWidth(prices, 20);
            var priceToMA50 = IndicatorCalculator.PriceToMA(rows[i].Close, ma50);
            var priceToMA10 = IndicatorCalculator.PriceToMA(rows[i].Close, ma10);
            var momentum = IndicatorCalculator.Momentum(prices, 10);
            var return5 = IndicatorCalculator.MultiDayReturn(prices, 5);
            var volumeMA5Ratio = IndicatorCalculator.VolumeRelativeToMA(volumes, 5);

            var label = rows[i + 1].Close > rows[i].Close;

            // Skip warmup rows where MA50/MACD/Bollinger are unreliable
            if (i < WarmupPeriod)
                continue;

            dataset.Add(new StockData
            {
                Close = rows[i].Close,
                Volume = rows[i].Volume,
                MA5 = ma5,
                MA10 = ma10,
                MA50 = ma50,
                RSI = rsi,
                Volatility = volatility,
                Return = ret,
                MACD = macd,
                BollingerWidth = bollingerWidth,
                PriceToMA50 = priceToMA50,
                PriceToMA10 = priceToMA10,
                Momentum = momentum,
                Return5 = return5,
                VolumeMA5Ratio = volumeMA5Ratio,
                Label = label
            });
        }

        return dataset;
    }
}
