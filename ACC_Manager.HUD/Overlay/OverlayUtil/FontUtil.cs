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

namespace ACCManager.HUD.Overlay.Util
{
    public class FontUtil
    {
        // Adding a private font (Win2000 and later)
        [DllImport("gdi32.dll", ExactSpelling = true)]
        private static extern IntPtr AddFontMemResourceEx(byte[] pbFont, int cbFont, IntPtr pdv, out uint pcFonts);

        // Cleanup of a private font (Win2000 and later)
        [DllImport("gdi32.dll", ExactSpelling = true)]
        internal static extern bool RemoveFontMemResourceEx(IntPtr fh);

        // Some private holders of font information we are loading
        static private IntPtr m_fh = IntPtr.Zero;
        static private PrivateFontCollection m_pfc = null;

        public static Font GetBoldFont(float size)
        {
            return GetSpecialFont(size, "ACCManager.HUD.Fonts.orbitron-medium.ttf");
        }

        private static Font GetSpecialFont(float size, string resourceName)
        {

            Font fnt = null;
            if (m_pfc == null)
            {

                // First load the font as a memory stream
                Stream stmFont = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

                if (null != stmFont)
                {

                    // 
                    // GDI+ wants a pointer to memory, GDI wants the memory.
                    // We will make them both happy.
                    //

                    // First read the font into a buffer
                    byte[] rgbyt = new Byte[stmFont.Length];
                    stmFont.Read(rgbyt, 0, rgbyt.Length);

                    // Then do the unmanaged font (Windows 2000 and later)
                    // The reason this works is that GDI+ will create a font object for
                    // controls like the RichTextBox and this call will make sure that GDI
                    // recognizes the font name, later.
                    uint cFonts;
                    AddFontMemResourceEx(rgbyt, rgbyt.Length, IntPtr.Zero, out cFonts);

                    // Now do the managed font
                    IntPtr pbyt = Marshal.AllocCoTaskMem(rgbyt.Length);
                    if (null != pbyt)
                    {
                        Marshal.Copy(rgbyt, 0, pbyt, rgbyt.Length);
                        m_pfc = new PrivateFontCollection();
                        m_pfc.AddMemoryFont(pbyt, rgbyt.Length);
                        Marshal.FreeCoTaskMem(pbyt);
                    }
                }

                stmFont.Close();
            }

            if (m_pfc.Families.Length > 0)
            {
                // Handy how one of the Font constructors takes a
                // FontFamily object, huh? :-)
                fnt = new Font(m_pfc.Families[0], size);
            }

            return fnt;
        }
    }
}
