using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Documents.DocumentStructures;
using System.Windows.Media.Imaging;
using Size = System.Drawing.Size;

namespace Monkeysoft.Screenshot
{
    public class Screenshot
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        public static Bitmap CaptureAllScreens()
        {
            Rect rect = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);

            var bitmap = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.CopyFromScreen(
                    (int)rect.X, (int)rect.Y, 0, 0,
                    new Size((int)rect.Size.Width, (int)rect.Size.Height),
                    CopyPixelOperation.SourceCopy);

                return bitmap;
            }
        }

        public static Bitmap CaptureRegion()
        {
            var bitmap = CaptureAllScreens();
            BitmapSource bgImg = bitmap.ToBitmapSource();

            var window = new RegionSelectionWindow
            {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                //Topmost = true,
                //ShowInTaskbar = false,
                BorderThickness = new Thickness(0),
                Background =
                {
                    Source = bgImg,
                    Opacity = 0.7d
                },
                Left = SystemParameters.VirtualScreenLeft,
                Top = SystemParameters.VirtualScreenTop,
                Width = SystemParameters.VirtualScreenWidth,
                Height = SystemParameters.VirtualScreenHeight
            };

            window.ShowDialog();
            return window.SelectedRegion.HasValue ?
                bitmap.Clip(window.SelectedRegion.Value) :
                bitmap;
        }

        public static Bitmap CaptureMonitor()
        {
            var bitmap = CaptureAllScreens();
            List<Bitmap> list = new List<Bitmap>();
            float offset = 0;

            foreach (var m in MonitorManager.GetMonitors().OrderBy(i => i.MonitorArea.X))
            {
                list.Add(bitmap.Clip(new Rect(
                    offset,
                    SystemParameters.VirtualScreenHeight - m.ScreenSize.Y,
                    m.ScreenSize.X,
                    m.ScreenSize.Y)));
                offset += m.ScreenSize.X;
            }

            var window = new MonitorSelection();
            window.lstGallery.ItemsSource = list.Select(i => i.ToBitmapSource());

            window.ShowDialog();
            return list[window.lstGallery.SelectedIndex];
        }
    }
}