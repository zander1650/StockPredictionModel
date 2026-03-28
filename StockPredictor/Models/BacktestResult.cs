namespace StockPredictor.Models;

public class BacktestResult
{
    public List<TradeRecord> Trades { get; set; } = [];
    public List<double> EquityCurve { get; set; } = [];
    public double StartingCapital { get; set; }
    public double FinalCapital { get; set; }
    public double TotalReturnPct => StartingCapital > 0 ? (FinalCapital - StartingCapital) / StartingCapital * 100 : 0;
    public double BuyHoldReturnPct { get; set; }
    public int TotalTrades { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double WinRate => TotalTrades > 0 ? (double)Wins / TotalTrades * 100 : 0;
    public double MaxDrawdownPct { get; set; }
    public double SharpeRatio { get; set; }
    public double ProfitFactor { get; set; }
    public double AvgWinPct { get; set; }
    public double AvgLossPct { get; set; }
    public int TrainDays { get; set; }
    public int TestDays { get; set; }
}

public class TradeRecord
{
    public int Day { get; set; }
    public string Direction { get; set; } = "";
    public float EntryPrice { get; set; }
    public float ExitPrice { get; set; }
    public double ReturnPct { get; set; }
    public double Equity { get; set; }
    public float Probability { get; set; }
    public bool Correct { get; set; }
}
