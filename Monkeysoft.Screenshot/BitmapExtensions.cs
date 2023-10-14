using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Monkeysoft.Screenshot
{
    public static class BitmapExtensions
    {
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(stream.ToArray());
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        public static Bitmap Clip(this Bitmap bitmap, Rect rect)
        {
            var outImg = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(outImg))
            {
                graphics.DrawImage(bitmap, -(int)rect.X, -(int)rect.Y);
                return outImg;
            }
        }
    }
}