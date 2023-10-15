using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Monkeysoft.Screenshot.Driver;
using Monkeysoft.Screenshot.Modules;

namespace Monkeysoft.Screenshot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        private static extern bool ShouldSystemUseDarkMode();

        private Modules.Screenshot Driver = Modules.Screenshot.GetInstance();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string key = ShouldSystemUseDarkMode() ? "Dark" : "Light";
            Resources.MergedDictionaries.Add(
                new ResourceDictionary
                {
                    Source = new Uri($"/Themes/{key}.xaml", UriKind.RelativeOrAbsolute)
                });
        }

        private void ClipAll(object sender, RoutedEventArgs e)
        {
            Hide();
            Clipboard.SetImage(Driver.CaptureAllScreens().ToBitmapSource());
            Application.Current.Shutdown();
        }

        private void ClipRegion(object sender, RoutedEventArgs e)
        {
            Hide();
            Clipboard.SetImage(Driver.CaptureRegion().ToBitmapSource());
            Application.Current.Shutdown();
        }

        private void ClipWindows(object sender, RoutedEventArgs e)
        {
            Hide();
            var ret = Driver.CaptrueWindows();
            if (ret != null)
            {
                Clipboard.SetImage(ret.ToBitmapSource());
            }
            Application.Current.Shutdown();
        }

        private void Exit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }

    public class UserWindow 
    {
        public string Name { get; set; }
        public IntPtr WindowHandle { get; set; }
    }
}
