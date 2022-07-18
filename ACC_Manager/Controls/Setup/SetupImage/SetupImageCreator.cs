using ACCManager.Data;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;
using static ACCManager.Data.SetupJson;
using static ACCManager.HUD.Overlay.OverlayUtil.InfoTable;

namespace ACCManager.Controls.Setup.SetupImage
{
    internal class SetupImageCreator
    {
        public static System.Windows.Controls.Image CreateImage(int width, int height, string file)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                RenderSetup(g, file, width, height);
            }

            MemoryStream memStream = new MemoryStream();
            bitmap.Save(memStream, ImageFormat.Png);

            BitmapImage bmi = new BitmapImage
            {
                DecodePixelWidth = bitmap.Width,
                DecodePixelHeight = bitmap.Height,
            };

            bmi.BeginInit();
            bmi.CacheOption = BitmapCacheOption.OnLoad;
            bmi.StreamSource = memStream;
            bmi.EndInit();
            bmi.Freeze();

            System.Windows.Controls.Image image = new System.Windows.Controls.Image
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                Stretch = System.Windows.Media.Stretch.Uniform,
                Source = bmi,
            };

            memStream.Close();
            memStream.Dispose();

            return image;
        }

        private static void RenderSetup(Graphics g, string file, int width, int height)
        {

        }
    }
}
