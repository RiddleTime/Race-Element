using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace RaceElement.Controls.Util.SetupImage
{
    internal static class ImageControlCreator
    {
        public static System.Windows.Controls.Image CreateImage(int width, int height, RaceElement.HUD.Overlay.OverlayUtil.CachedBitmap cachedBitmap)
        {
            Bitmap bitmap = new(width, height, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                cachedBitmap?.Draw(g, 0, 0, width, height);
            }

            MemoryStream memStream = new();
            bitmap.Save(memStream, ImageFormat.Png);

            BitmapImage bmi = new()
            {
                DecodePixelWidth = bitmap.Width,
                DecodePixelHeight = bitmap.Height,
            };

            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.StreamSource = memStream;
            bmi.EndInit();
            bmi.Freeze();

            System.Windows.Controls.Image image = new()
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                Stretch = System.Windows.Media.Stretch.Uniform,
                Source = bmi,
            };

            memStream.Close();
            memStream.Dispose();

            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, false);

            return image;
        }
    }
}
