using BTGBrownianMotion.Infrastructure.Extensions;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace BTGBrownianMotion
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .ConfigureServices()
                .ConfigurePages()
                .ConfigureViewModels()
                .ConfigureCustomFonts();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
