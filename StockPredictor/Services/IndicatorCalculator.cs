namespace StockPredictor.Services;

public static class IndicatorCalculator
{
    public static float MovingAverage(List<float> prices, int period)
    {
        if (prices.Count < period)
            return prices.Average();

        return prices.Skip(prices.Count - period).Take(period).Average();
    }

    public static float Volatility(List<float> prices, int period)
    {
        if (prices.Count < period)
            return 0;

        var window = prices.Skip(prices.Count - period).Take(period).ToList();
        var avg = window.Average();
        var variance = window.Select(p => Math.Pow(p - avg, 2)).Average();

        return (float)Math.Sqrt(variance);
    }

    public static float RSI(List<float> prices, int period)
    {
        if (prices.Count < period + 1)
            return 50;

        float gain = 0;
        float loss = 0;

        for (int i = prices.Count - period; i < prices.Count; i++)
        {
            var diff = prices[i] - prices[i - 1];

            if (diff > 0)
                gain += diff;
            else
                loss -= diff;
        }

        if (loss == 0)
            return 100;

        var rs = gain / loss;

        return 100 - (100 / (1 + rs));
    }

    public static float DailyReturn(float today, float yesterday)
    {
        if (yesterday == 0) return 0;
        return (today - yesterday) / yesterday;
    }

    /// <summary>MACD = EMA12 - EMA26 (approximated with simple MAs)</summary>
    public static float MACD(List<float> prices)
    {
        var ma12 = MovingAverage(prices, 12);
        var ma26 = MovingAverage(prices, 26);
        return ma12 - ma26;
    }

    /// <summary>Bollinger Band width = (upper - lower) / middle, using 20-day MA ± 2 std devs</summary>
    public static float BollingerBandWidth(List<float> prices, int period = 20)
    {
        if (prices.Count < period)
            return 0;

        var window = prices.Skip(prices.Count - period).Take(period).ToList();
        var avg = window.Average();
        var stdDev = (float)Math.Sqrt(window.Select(p => Math.Pow(p - avg, 2)).Average());

        if (avg == 0) return 0;
        return (4f * stdDev) / avg;
    }

    /// <summary>Price relative to a moving average (ratio). &gt;1 = above, &lt;1 = below</summary>
    public static float PriceToMA(float price, float ma)
    {
        if (ma == 0) return 1;
        return price / ma;
    }

    /// <summary>Momentum = price now vs price N days ago (percentage change)</summary>
    public static float Momentum(List<float> prices, int period)
    {
        if (prices.Count <= period)
            return 0;

        var past = prices[prices.Count - 1 - period];
        if (past == 0) return 0;
        return (prices[^1] - past) / past;
    }

    /// <summary>Multi-day return over N days</summary>
    public static float MultiDayReturn(List<float> prices, int days)
    {
        if (prices.Count <= days)
            return 0;

        var past = prices[prices.Count - 1 - days];
        if (past == 0) return 0;
        return (prices[^1] - past) / past;
    }

    /// <summary>Current volume relative to its own 5-day average</summary>
    public static float VolumeRelativeToMA(List<float> volumes, int period = 5)
    {
        if (volumes.Count < period)
            return 1;

        var avg = volumes.Skip(volumes.Count - period).Take(period).Average();
        if (avg == 0) return 1;
        return volumes[^1] / avg;
    }
}
