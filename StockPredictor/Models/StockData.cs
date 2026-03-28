using Microsoft.ML.Data;

namespace StockPredictor.Models;

public class StockData
{
    [LoadColumn(0)]
    public float Close;

    [LoadColumn(1)]
    public float Volume;

    [LoadColumn(2)]
    public float MA5;

    [LoadColumn(3)]
    public float MA10;

    [LoadColumn(4)]
    public float MA50;

    [LoadColumn(5)]
    public float RSI;

    [LoadColumn(6)]
    public float Volatility;

    [LoadColumn(7)]
    public float Return;

    // ── New features for improved accuracy ──

    [LoadColumn(8)]
    public float MACD;

    [LoadColumn(9)]
    public float BollingerWidth;

    [LoadColumn(10)]
    public float PriceToMA50;

    [LoadColumn(11)]
    public float PriceToMA10;

    [LoadColumn(12)]
    public float Momentum;

    [LoadColumn(13)]
    public float Return5;

    [LoadColumn(14)]
    public float VolumeMA5Ratio;

    [LoadColumn(15)]
    public bool Label;
}
