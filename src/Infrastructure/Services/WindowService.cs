#if WINDOWS
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using System.Runtime.InteropServices;
#endif

using BTGBrownianMotion.Infrastructure.Extensions;

namespace BTGBrownianMotion.Shared.Services;
public interface IWindowService
{                     
    void ClosePage(Page page = null);
    void OpenWindowCentered<TPage, TViewModel>()
        where TPage : Page
        where TViewModel : class;

    void OpenWindowCentered<TPage, TViewModel, TParam>(TParam param)
        where TPage : Page
        where TViewModel : class;
}
public class WindowService : IWindowService
{
    private readonly IServiceProvider _serviceProvider;
#if WINDOWS
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    const uint SWP_NOMOVE = 0x0002;
    const uint SWP_NOSIZE = 0x0001;
    const uint SWP_NOACTIVATE = 0x0010;
    const uint SWP_SHOWWINDOW = 0x0040;
#endif
    public WindowService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void ClosePage(Page page = null)
    {
        var window = page is null ? 
            Microsoft.Maui.Controls.Application.Current.Windows.LastOrDefault() :
            Microsoft.Maui.Controls.Application.Current.Windows.FirstOrDefault(w => w.Page == page);
        if (window is not null)
            Microsoft.Maui.Controls.Application.Current.CloseWindow(window);
    }

    public void OpenWindowCentered<TPage, TViewModel>()
        where TPage : Page
        where TViewModel : class
    {
        var vm = _serviceProvider.GetRequiredService<TViewModel>();
        CreateWindowWithPage<TPage, TViewModel>(vm);
    }

    public void OpenWindowCentered<TPage, TViewModel, TParam>(TParam param)
        where TPage : Page
        where TViewModel : class
    {
        var vm = _serviceProvider.GetRequiredService<TViewModel>();

        var loadMethod = typeof(TViewModel).GetMethod("Load", [typeof(TParam)]);
        loadMethod?.Invoke(vm, [param]);

        CreateWindowWithPage<TPage, TViewModel>(vm);
    }

    private void CreateWindowWithPage<TPage, TViewModel>(TViewModel vm)
        where TPage : Page
        where TViewModel : class
    {
        var pageConstructor = typeof(TPage).GetConstructors()
            .FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 && parameters[0].ParameterType == typeof(TViewModel);
            });

        if (pageConstructor is null)
            throw new InvalidOperationException($"A página {typeof(TPage).Name} deve ter um construtor que aceite {typeof(TViewModel).Name}.");

        var page = (Page)pageConstructor.Invoke([vm]);

        var newWindow = new Microsoft.Maui.Controls.Window(page);
        newWindow.SetWindowCentralized();

#if WINDOWS
        newWindow.Created += (s, e) =>
        {
            var nativeWindow = newWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        };
#endif

        Microsoft.Maui.Controls.Application.Current.OpenWindow(newWindow);
    }
}
