using Microsoft.VisualBasic;
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

namespace Monkeysoft.Screenshot.Driver
{
    public class Screenshot
    {
        public static Bitmap CaptureWindow(IntPtr handle)
        {
            var bitmap = CaptureAllScreens();
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

        private static Bitmap CaptureRectangleNative(IntPtr handle, Rectangle rect)
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

        public static bool IsDWMEnabled() => Environment.OSVersion.Version.Major >= 6 && NativeMethods.DwmIsCompositionEnabled();
        public static bool IsWindows10OrGreater() => Environment.OSVersion.Version.Major >= 10;

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
    }
}