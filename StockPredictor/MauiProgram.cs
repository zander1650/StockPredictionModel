using Microsoft.Extensions.Logging;
using StockPredictor.Services;

namespace StockPredictor
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<StockDataFetcher>();
            builder.Services.AddSingleton<StockPredictionService>();
            builder.Services.AddSingleton<ThemeService>();
            builder.Services.AddSingleton<SettingsService>();
            builder.Services.AddTransient<BacktestService>();

#if DEBUG
    		builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
