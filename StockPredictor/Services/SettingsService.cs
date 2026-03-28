namespace StockPredictor.Services;

public class SettingsService
{
    public string DefaultTicker
    {
        get => Preferences.Default.Get(nameof(DefaultTicker), "AAPL");
        set => Preferences.Default.Set(nameof(DefaultTicker), value);
    }

    public double DefaultCapital
    {
        get => Preferences.Default.Get(nameof(DefaultCapital), 10_000.0);
        set => Preferences.Default.Set(nameof(DefaultCapital), value);
    }

    public int DefaultTrainPct
    {
        get => Preferences.Default.Get(nameof(DefaultTrainPct), 70);
        set => Preferences.Default.Set(nameof(DefaultTrainPct), value);
    }

    public double ConfidenceThreshold
    {
        get => Preferences.Default.Get(nameof(ConfidenceThreshold), 0.5);
        set => Preferences.Default.Set(nameof(ConfidenceThreshold), value);
    }

    public bool AllowShorts
    {
        get => Preferences.Default.Get(nameof(AllowShorts), true);
        set => Preferences.Default.Set(nameof(AllowShorts), value);
    }

    public void ResetAll()
    {
        Preferences.Default.Remove(nameof(DefaultTicker));
        Preferences.Default.Remove(nameof(DefaultCapital));
        Preferences.Default.Remove(nameof(DefaultTrainPct));
        Preferences.Default.Remove(nameof(ConfidenceThreshold));
        Preferences.Default.Remove(nameof(AllowShorts));
    }
}
