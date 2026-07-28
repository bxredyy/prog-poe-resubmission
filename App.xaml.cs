// App.xaml.cs
// Entry point for the WPF app. Builds the ServiceContainer when the app starts.
// Reference: Microsoft Docs - WPF Application Class
//   https://learn.microsoft.com/en-us/dotnet/api/system.windows.application

using System.Windows;
using CyberSecurityBot.Services;

namespace CyberSecurityBot
{
    public partial class App : Application
    {
        // Static reference so MainWindow and the views can reach the services.
        public static ServiceContainer Services;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Services = new ServiceContainer();
        }
    }
}
