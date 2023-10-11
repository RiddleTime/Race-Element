using System.Drawing;

namespace RaceElement.HUD.Overlay.OverlayUtil
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Blurs a picture by a certain amount
        /// </summary>
        /// <param name="image">image to blur</param>
        /// <param name="blurSize">impact of the blur</param>
        /// <returns>a picture blured by a certain amount</returns>
        public static Bitmap Blur(this Bitmap image, int blurSize = 5)
        {
            return image.Blur(image.ToRectangle(), blurSize);
        }

        /// <summary>
        /// returns a <see cref="Rectangle"/> with dimensions of a given <see cref="Image"/>
        /// </summary>
        /// <param name="image">image with given dimensions</param>
        /// <returns>rectangle with same dimensions as a given image</returns>
        private static Rectangle ToRectangle(this Image image)
        {
            return new Rectangle(0, 0, image.Width, image.Height);
        }

        /// <summary>
        /// Blurs a picture by a certain amount in an area determined by a rectangle
        /// </summary>
        /// <param name="image">image to blur</param>
        /// <param name="rectangle">area in the picture</param>
        /// <param name="blurSize">impact of the blur</param>
        /// <returns>a picture blured by a certain amount in an area determined by a rectangle</returns>
        //based on http://notes.ericwillis.com/2009/10/blur-an-image-with-csharp/ (2016/08)
        public static Bitmap Blur(this Bitmap image, Rectangle rectangle, int blurSize = 5)
        {
            var blurred = new Bitmap(image.Width, image.Height);

            using (var graphics = Graphics.FromImage(blurred))
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);

            for (var xx = rectangle.X; xx < rectangle.X + rectangle.Width; xx++)
            {
                for (var yy = rectangle.Y; yy < rectangle.Y + rectangle.Height; yy++)
                {
                    int avgR = 0, avgG = 0, avgB = 0;
                    var blurPixelCount = 0;

                    for (var x = xx; x < xx + blurSize && x < image.Width; x++)
                    {
                        for (var y = yy; y < yy + blurSize && y < image.Height; y++)
                        {
                            var pixel = blurred.GetPixel(x, y);

                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;

                            blurPixelCount++;
                        }
                    }

                    avgR = avgR / blurPixelCount;
                    avgG = avgG / blurPixelCount;
                    avgB = avgB / blurPixelCount;

                    for (var x = xx; x < xx + blurSize && x < image.Width && x < rectangle.Width; x++)
                        for (var y = yy; y < yy + blurSize && y < image.Height && y < rectangle.Height; y++)
                            blurred.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

            return blurred;
        }
    }
}
