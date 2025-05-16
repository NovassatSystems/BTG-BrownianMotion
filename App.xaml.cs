using BTGBrownianMotion.Infrastructure.Extensions;

namespace BTGBrownianMotion
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Console.WriteLine($"[Unhandled] {e.ExceptionObject}");
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                Console.WriteLine($"[UnobservedTaskException] {e.Exception.Message}");
                e.SetObserved();
            };

            
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window =  new Window(new AppShell());
            window.SetWindowMaximized();
            return window;
        }


    }
}