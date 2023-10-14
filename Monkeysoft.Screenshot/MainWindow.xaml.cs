using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Monkeysoft.Screenshot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        private static extern bool ShouldSystemUseDarkMode();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string key = ShouldSystemUseDarkMode() ? "Dark" : "Light";

            Top = 20;
            Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri($"/Themes/{key}.xaml", UriKind.RelativeOrAbsolute)
                });
        }

        private void ClipRegion(object sender, RoutedEventArgs e)
        {
            Hide();
            Clipboard.SetImage(Screenshot.CaptureRegion().ToBitmapSource());
            Application.Current.Shutdown();
        }

        private void Exit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void ClipMonitor(object sender, RoutedEventArgs e)
        {
            Hide();
            Clipboard.SetImage(Screenshot.CaptureMonitor().ToBitmapSource());
            Application.Current.Shutdown();
        }
    }
}
