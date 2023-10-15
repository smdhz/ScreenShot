using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Monkeysoft.Screenshot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        private static extern bool ShouldSystemUseDarkMode();

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            string key = ShouldSystemUseDarkMode() ? "Dark" : "Light";
            Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri($"/Themes/{key}.xaml", UriKind.RelativeOrAbsolute)
                });
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(
                e.Exception.Message,
                e.GetType().Name,
                MessageBoxButton.OK,
                MessageBoxImage.Exclamation);
        }
    }
}
