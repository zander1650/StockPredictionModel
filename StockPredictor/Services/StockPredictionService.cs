using Microsoft.ML;
using StockPredictor.Models;

namespace StockPredictor.Services;

public class StockPredictionService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<StockData, StockPrediction>? _engine;

    public bool IsModelTrained => _model is not null;
    public string TrainingStatus { get; set; } = "Not trained";
    public double Accuracy { get; private set; }
    public double AUC { get; private set; }
    public double F1Score { get; private set; }

    // All feature column names used in the pipeline
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

    public StockPredictionService()
    {
        _mlContext = new MLContext(seed: 0);
    }

    private IEstimator<ITransformer> BuildPipeline()
    {
        return _mlContext.Transforms.Concatenate("Features", FeatureColumns)
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.BinaryClassification.Trainers.FastTree(
                numberOfLeaves: 8,
                numberOfTrees: 60,
                minimumExampleCountPerLeaf: 3,
                learningRate: 0.05));
    }

    public async Task TrainFromCsvAsync(string csvPath)
    {
        await Task.Run(() =>
        {
            TrainingStatus = "Loading data...";

            var data = _mlContext.Data.LoadFromTextFile<StockData>(
                csvPath,
                separatorChar: ',',
                hasHeader: true);

            TrainAndEvaluate(data);
        });
    }

    public async Task TrainFromRawDataAsync(List<RawStockRow> rows)
    {
        await Task.Run(() =>
        {
            TrainingStatus = "Building dataset from raw data...";

            var dataset = DatasetBuilder.BuildDataset(rows);

            var data = _mlContext.Data.LoadFromEnumerable(dataset);

            TrainAndEvaluate(data);
        });
    }

    private void TrainAndEvaluate(IDataView data)
    {
        TrainingStatus = "Cross-validating...";

        var pipeline = BuildPipeline();

        // Cross-validate for more reliable metrics on small datasets
        var cvResults = _mlContext.BinaryClassification.CrossValidate(
            data, pipeline, numberOfFolds: 5);

        Accuracy = cvResults.Average(r => r.Metrics.Accuracy);
        AUC = cvResults.Average(r => r.Metrics.AreaUnderRocCurve);
        F1Score = cvResults.Average(r => r.Metrics.F1Score);

        TrainingStatus = "Training final model on all data...";

        // Train final model on the full dataset for best prediction
        _model = pipeline.Fit(data);
        _engine = _mlContext.Model.CreatePredictionEngine<StockData, StockPrediction>(_model);

        TrainingStatus = "Model trained successfully";
    }

    public StockPrediction? Predict(StockData input)
    {
        return _engine?.Predict(input);
    }
}
