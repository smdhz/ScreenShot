using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
        private Modules.Screenshot Driver = Modules.Screenshot.GetInstance();

        private static readonly TimeSpan[] Spans = new TimeSpan[] 
        {
            TimeSpan.Zero,
            TimeSpan.FromSeconds(3),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(10)
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Top = 10;
            Left = (SystemParameters.WorkArea.Width - Width) / 2;
        }

        private void ClipAll(object sender, RoutedEventArgs e)
        {
            Hide();
            Task.Delay(Spans[cmbWait.SelectedIndex]).Wait();

            Clipboard.SetImage(Driver.CaptureAllScreens().ToBitmapSource());
            Application.Current.Shutdown();
        }

        private void ClipRegion(object sender, RoutedEventArgs e)
        {
            Hide();
            Task.Delay(Spans[cmbWait.SelectedIndex]).Wait();

            Clipboard.SetImage(Driver.CaptureRegion().ToBitmapSource());
            Application.Current.Shutdown();
        }

        private void ClipWindows(object sender, RoutedEventArgs e)
        {
            Hide();
            Task.Delay(Spans[cmbWait.SelectedIndex]).Wait();

            var ret = Driver.CaptrueWindows();
            if (ret != null)
            {
                Clipboard.SetImage(ret.ToBitmapSource());
            }
            Application.Current.Shutdown();
        }

        private void Exit(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }

    public class UserWindow 
    {
        public string Name { get; set; } = string.Empty;
        public IntPtr WindowHandle { get; set; }
    }
}
