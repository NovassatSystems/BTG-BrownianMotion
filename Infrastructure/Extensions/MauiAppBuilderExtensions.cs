using BTGBrownianMotion.Features.Main;
using BTGBrownianMotion.Shared.Services;

namespace BTGBrownianMotion.Infrastructure.Extensions;

public static class MauiAppBuilderExtensions
{
    public static MauiAppBuilder ConfigureServices(this MauiAppBuilder app)
    {
        app.Services.AddSingleton<IWindowService, WindowService>();
        return app;
    }

    public static MauiAppBuilder ConfigurePages(this MauiAppBuilder app)
    {
        
        app.Services.AddTransient<MainPage>();

        return app;
    }

    public static MauiAppBuilder ConfigureViewModels(this MauiAppBuilder app)
    {
        //app.Services.AddTransient<BaseViewModel>();
        app.Services.AddTransient<MainViewModel>();


        return app;
    }

    public static MauiAppBuilder ConfigureCustomFonts(this MauiAppBuilder app)
    {
        app.ConfigureFonts(fonts =>
        {
            fonts.AddFont("Prometo.otf", "Prometo");
            fonts.AddFont("PrometoBlack.otf", "PrometoBlack");
            fonts.AddFont("PrometoBlackItalic.otf", "PrometoBlackItalic");
            fonts.AddFont("PrometoBold.otf", "PrometoBold");
            fonts.AddFont("PrometoBoldItalic.otf", "PrometoBoldItalic");
            fonts.AddFont("PrometoItalic.otf", "PrometoItalic");
            fonts.AddFont("PrometoLight.otf", "PrometoLight");
            fonts.AddFont("PrometoLightItalic.otf", "PrometoLightItalic");
            fonts.AddFont("PrometoMedium.otf", "PrometoMedium");
            fonts.AddFont("PrometoMediumItalic.otf", "PrometoMediumItalic");
            fonts.AddFont("PrometoThin.otf", "PrometoThin");
            fonts.AddFont("PrometoThinItalic.otf", "PrometoThinItalic");
            fonts.AddFont("PrometoXBold.otf", "PrometoXBold");
            fonts.AddFont("PrometoXBoldItalic.otf", "PrometoXBoldItalic");
            
        });

        return app;
    }
}