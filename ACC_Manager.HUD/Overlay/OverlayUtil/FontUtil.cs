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
        static private PrivateFontCollection m_pfc = null;

        public static Font FontOrbitron(float size)
        {
            return GetSpecialFont(size, "ACCManager.HUD.Fonts.orbitron-medium.ttf", "Orbitron");
        }

        public static Font FontUnispace(float size)
        {
            return GetSpecialFont(size - 1, "ACCManager.HUD.Fonts.unispace.bold.ttf", "Unispace");
        }

        private static Font GetSpecialFont(float size, string resourceName, string fontName)
        {
            Font fnt = null;

            if (m_pfc != null)
                lock (m_pfc)
                    if (m_pfc.Families.Length > 0)
                    {
                        // Handy how one of the Font constructors takes a
                        // FontFamily object, huh? :-)
                        for (int i = 0; i < m_pfc.Families.Length; i++)
                        {
                            if (m_pfc.Families[i].Name == fontName)
                                fnt = new Font(m_pfc.Families[i], size);
                        }

                        //fnt = new Font(m_pfc.Families[0], size);
                    }

            if (fnt == null)
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
                        uint cFonts;
                        AddFontMemResourceEx(rgbyt, rgbyt.Length, IntPtr.Zero, out cFonts);

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



            return fnt;
        }
    }
}
