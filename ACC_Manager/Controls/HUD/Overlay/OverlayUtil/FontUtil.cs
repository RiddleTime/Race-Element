using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayUtil
{
    internal class FontUtil
    {
        public static FontFamily GetRobotoMedium()
        {
            PrivateFontCollection private_fonts = new PrivateFontCollection();

            string resource = "ACCSetupApp.Fonts.Roboto-Regular.ttf";

            // receive resource stream
            Stream fontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource);

            // create an unsafe memory block for the font data
            System.IntPtr data = Marshal.AllocCoTaskMem((int)fontStream.Length);

            // create a buffer to read in to
            byte[] fontdata = new byte[fontStream.Length];

            // read the font data from the resource
            fontStream.Read(fontdata, 0, (int)fontStream.Length);

            // copy the bytes to the unsafe memory block
            Marshal.Copy(fontdata, 0, data, (int)fontStream.Length);

            // pass the font to the font collection
            private_fonts.AddMemoryFont(data, (int)fontStream.Length);

            // close the resource stream
            fontStream.Close();

            // free up the unsafe memory
            Marshal.FreeCoTaskMem(data);

            return private_fonts.Families[0];
        }
    }
}
