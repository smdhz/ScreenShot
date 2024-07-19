using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Monkeysoft.Screenshot.Driver;
using Size = System.Drawing.Size;

namespace Monkeysoft.Screenshot.Modules
{
    public class Screenshot
    {
        private static Screenshot? instance;
        public static Screenshot GetInstance() => instance ??= new Screenshot();

        private Bitmap CaptureWindow(IntPtr handle)
        {
            Rectangle rect = Rectangle.Empty;

            if (IsDWMEnabled() && NativeMethods.GetExtendedFrameBounds(handle, out Rectangle tempRect))
            {
                rect = tempRect;
            }

            if (rect.IsEmpty)
            {
                rect = NativeMethods.GetWindowRect(handle);
            }

            if (!IsWindows10OrGreater() && NativeMethods.IsZoomed(handle))
            {
                rect = NativeMethods.MaximizedWindowFix(handle, rect);
            }

            rect = Rectangle.Intersect(new Rectangle(
                (int)SystemParameters.VirtualScreenLeft,
                (int)SystemParameters.VirtualScreenTop,
                (int)SystemParameters.VirtualScreenWidth,
                (int)SystemParameters.VirtualScreenHeight), rect);

            return CaptureRectangleNative(NativeMethods.GetDesktopWindow(), rect);
        }

        private Bitmap CaptureRectangleNative(IntPtr handle, Rectangle rect)
        {
            IntPtr hdcSrc = NativeMethods.GetWindowDC(handle);
            IntPtr hdcDest = NativeMethods.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = NativeMethods.CreateCompatibleBitmap(hdcSrc, rect.Width, rect.Height);
            IntPtr hOld = NativeMethods.SelectObject(hdcDest, hBitmap);
            NativeMethods.BitBlt(hdcDest, 0, 0, rect.Width, rect.Height, hdcSrc, rect.X, rect.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);

            NativeMethods.SelectObject(hdcDest, hOld);
            NativeMethods.DeleteDC(hdcDest);
            NativeMethods.ReleaseDC(handle, hdcSrc);
            Bitmap bmp = Image.FromHbitmap(hBitmap);
            NativeMethods.DeleteObject(hBitmap);

            return bmp;
        }

        private bool IsDWMEnabled() => Environment.OSVersion.Version.Major >= 6 && NativeMethods.DwmIsCompositionEnabled();
        private bool IsWindows10OrGreater() => Environment.OSVersion.Version.Major >= 10;

        public Bitmap CaptureAllScreens()
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

        public Bitmap CaptureRegion()
        {
            var bitmap = CaptureAllScreens();
            BitmapSource bgImg = bitmap.ToBitmapSource();

            var window = new RegionSelectionWindow
            {
                bgImage =
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

        public Bitmap? CaptrueWindows()
        {
            List<UserWindow> windows = new List<UserWindow>();
            NativeMethods.EnumWindows((hWnd, lParam) =>
            {
                if (hWnd.IsWindowValidForCapture()) 
                {
                    string? title = NativeMethods.GetWindowText(hWnd);
                    if (!string.IsNullOrEmpty(title))
                        windows.Add(new UserWindow { Name = title, WindowHandle = hWnd });
                }
                return windows.Count <= 10;
            }, IntPtr.Zero);

            WindowPicker window = new WindowPicker();
            window.List.ItemsSource = windows;
            if (window.ShowDialog() ?? false)
            {
                Task.Delay(TimeSpan.FromSeconds(0.5)).Wait();
                return CaptureWindow(windows[window.List.SelectedIndex].WindowHandle);
            }

            return null;
        }
    }
}