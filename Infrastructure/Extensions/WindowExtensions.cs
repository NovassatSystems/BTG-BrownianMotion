using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using System.Runtime.InteropServices;

namespace BTGBrownianMotion.Infrastructure.Extensions;

public static class WindowExtensions
{
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_MAXIMIZE = 3;
    public static Window SetWindowMaximized(this Window window)
    {
        if (window is null) return window;
        window.Created += (s, e) =>
        {
            var nativeWindow = window.Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

            ShowWindow(hwnd, SW_MAXIMIZE);
        };
        return window;
    }
    
    public static Window SetWindowCentralized(this Window window)
    {
        if (window is null) return window;
        window.Created += (s, e) =>
        {
            var nativeWindow = window.Handler.PlatformView as Microsoft.UI.Xaml.Window;
            var appWindow = nativeWindow?.GetAppWindow();

            if (appWindow is not null)
            {
                var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
                var screenCenter = displayArea.WorkArea;

                var width = 500;
                var height = 450;

                appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
                appWindow.Move(new Windows.Graphics.PointInt32(
                    screenCenter.X + (screenCenter.Width - width) / 2,
                    screenCenter.Y + (screenCenter.Height - height) / 2));
            }
        };
        return window;
    }
}