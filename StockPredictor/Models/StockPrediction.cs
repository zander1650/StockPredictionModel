using Microsoft.ML.Data;

namespace StockPredictor.Models;

public class StockPrediction
{
    [ColumnName("PredictedLabel")]
    public bool PredictedLabel;

    public float Probability;

    public float Score;
}
