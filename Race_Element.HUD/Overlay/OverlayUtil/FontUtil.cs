using RaceElement.HUD.Overlay.OverlayUtil;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.Overlay.Util
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
        static private PrivateFontCollection m_pfc = null;

        public static Font FontOrbitron(float size)
        {
            return GetSpecialFont(size, "RaceElement.HUD.Fonts.orbitron-medium.ttf", "Orbitron");
        }

        public static Font FontSegoeMono(float size)
        {
            return GetSpecialFont(size, "RaceElement.HUD.Fonts.segoe-ui-mono-w01-bold.ttf", "Segoe UI Mono W01");
        }

        public static Font FontConthrax(float size)
        {
            return GetSpecialFont(size, "RaceElement.HUD.Fonts.ConthraxSb.ttf", "Conthrax Sb");
        }

        public static float MeasureWidth(Font font, string text)
        {
            float width = 0;

            CachedBitmap c = new CachedBitmap(1, 1, g =>
            {
                width = g.MeasureString(text, font).Width;
            });
            c.Dispose();

            return width;
        }

        private static Font GetSpecialFont(float size, string resourceName, string fontName)
        {
            Font font = null;

            if (m_pfc != null)
                lock (m_pfc)
                    if (m_pfc.Families.Length > 0)
                    {
                        // Handy how one of the Font constructors takes a
                        // FontFamily object, huh? :-)
                        for (int i = 0; i < m_pfc.Families.Length; i++)
                        {
                            if (m_pfc.Families[i].Name == fontName)
                                font = new Font(m_pfc.Families[i], size);
                        }
                    }

            if (font == null)
                using (Stream stmFont = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    if (stmFont != null)
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
                        AddFontMemResourceEx(rgbyt, rgbyt.Length, IntPtr.Zero, out _);

                        // Now do the managed font
                        IntPtr pbyt = Marshal.AllocCoTaskMem(rgbyt.Length);
                        if (null != pbyt)
                        {
                            Marshal.Copy(rgbyt, 0, pbyt, rgbyt.Length);
                            if (m_pfc == null)
                                m_pfc = new PrivateFontCollection();
                            m_pfc.AddMemoryFont(pbyt, rgbyt.Length);
                            Marshal.FreeCoTaskMem(pbyt);
                        }
                    }

                    stmFont.Close();
                    return GetSpecialFont(size, resourceName, fontName);
                }

            return font;
        }
    }
}
