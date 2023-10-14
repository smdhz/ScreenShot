using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

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
            Clipboard.SetImage(Driver.Screenshot.CaptureRegion().ToBitmapSource());
            Application.Current.Shutdown();
        }

        private void Exit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void ClipWindow(object sender, RoutedEventArgs e)
        {
            //Hide();
            Clipboard.SetImage(Driver.Screenshot.CaptureWindow(new WindowInteropHelper(this).Handle).ToBitmapSource());
            Application.Current.Shutdown();
        }
    }
}
