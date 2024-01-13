using LiteDB;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using SplatoonLoadout.Services;

namespace SplatoonLoadout;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddHttpClient();
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        builder.Services.AddTransient<UpdateChecker>();
        builder.Services.AddTransient<CacheService>();
        builder.Services.AddTransient<WeaponService>();
        builder.Services.AddSingleton<ILiteDatabase, LiteDatabase>(e => new LiteDatabase("Cache/Database.db"));

        if(!Directory.Exists("Cache")) {
            Directory.CreateDirectory("Cache");
        }


#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}


