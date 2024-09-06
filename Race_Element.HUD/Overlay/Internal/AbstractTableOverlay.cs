using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.Overlay.Internal
{
    public abstract class AbstractTableOverlay : CommonAbstractOverlay
    {
        private int NextX { get; set; }
        private int NextY { get; set; }
        
        private readonly int _columnGap = 5;
        // pixels between rows
        private readonly int _rowGap = 3;

        static Font DefaultFont = FontUtil.FontSegoeMono(13);
        int fontMaxCharWidth = -1;

        int TextOffset = 2;

        public static readonly Color DefaultTextColor = Color.White;
        public static readonly SolidBrush DefaultTextColorBrush = new SolidBrush(Color.FromArgb(150, DefaultTextColor));
        public static readonly SolidBrush OddBackground = new(Color.FromArgb(100, Color.Black));
        public static readonly SolidBrush EvenBackground = new(Color.FromArgb(180, Color.Black));
        public static readonly SolidBrush DriversCarBackground = new(Color.FromArgb(180, Color.DarkSeaGreen));
        public static readonly SolidBrush HeaderBackground = new(Color.FromArgb(100, Color.DarkSeaGreen));

        protected AbstractTableOverlay(Rectangle rectangle, string Name) : base(rectangle, Name) {            
        }

        /// <summary>
        /// Get the names and lenths for the overall header (e.g. race info and labels for all cells)
        /// </summary>
        /// <returns></returns>
        abstract public List<HeaderLabel> GetOverallHeader();

        /// <summary>
        /// Get names and length for intermediate headers for a section (e.g. class headers)
        /// </summary>
        /// <returns></returns>
        abstract public List<HeaderLabel> GetSectionHeaders();

        /// <summary>
        /// Get the row values for a 
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        abstract public List<List<CellValue>> GetCellRows(int section);

        /// <summary>
        /// Get metadata for the rows in the sections
        /// </summary>        
        abstract public List<ColumnMetaData> GetColumnMetaData();

        public override void Render(Graphics g)
        {
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            NextX = 0;
            NextY = 0;

            if (fontMaxCharWidth < 0)
            {
                for (int i = 32; i < 127; i++)
                {
                    char character = (char)i;
                    SizeF size = g.MeasureString(character.ToString(), DefaultFont);
                    fontMaxCharWidth = Math.Max(fontMaxCharWidth, (int)size.Width);
                }
            }
                                   
            DrawOveralHeaders(g);
            DrawSections(g);
        }

        public sealed override void SetupPreviewData()
        {
            SimDataProvider.Instance.SetupPreviewData();
        }

        private void DrawSections(Graphics g)
        {
            List<HeaderLabel> sectionHeaders = GetSectionHeaders();
            if (sectionHeaders.Count == 0)
            {
                DrawRowSection(g, 0);
            }
            else
            {
                int section = 0;
                foreach (HeaderLabel sectionHeader in sectionHeaders)
                {
                    DrawSectionHeader(g, sectionHeader);
                    DrawRowSection(g, section++);
                }                                
            }
        }

        private void DrawSectionHeader(Graphics g, HeaderLabel sectionHeader)
        {
            int maxTextwidth = sectionHeader.ColumnLength * fontMaxCharWidth;
            
            g.FillRectangle(sectionHeader.BackgroundColor, NextX, NextY, maxTextwidth, DefaultFont.Height);
            
            TextRenderer.DrawText(g, TruncateString(sectionHeader.Name, sectionHeader.ColumnLength), DefaultFont, new Point(NextX, NextY + TextOffset), DefaultTextColor);

            NextX = 0;
            NextY += DefaultFont.Height + _rowGap;
        }

        private void DrawRowSection(Graphics g, int section)
        {            
            List<ColumnMetaData> columnMetaDatas = GetColumnMetaData();
            List<List<CellValue>> rows = GetCellRows(section);


            for (int rowNum = 0; rowNum < rows.Count; rowNum++)            
            {
                List<CellValue> rowValues = rows[rowNum];
                
                Brush backgroundColor = OddBackground;                
                if (rowNum % 2 == 0)
                {
                    backgroundColor = EvenBackground;
                }

                if (rowValues[0].Value.Equals(SessionData.Instance.PlayerCarIndex.ToString()))
                {
                    backgroundColor = DriversCarBackground;
                }                

                for (int colNum = 0; colNum < rowValues.Count; colNum++)
                {
                    ColumnMetaData columnMetaData = columnMetaDatas[colNum];
                    SolidBrush TextColor = DefaultTextColorBrush;
                    Brush cellBackground = backgroundColor;
                    if (rowValues[colNum].TextColor != null)
                    {
                        TextColor = new SolidBrush(Color.FromArgb(150, (Color)rowValues[colNum].TextColor));
                    }
                    if (rowValues[colNum].BackgroundColor !=  null)
                    {
                        cellBackground = new SolidBrush(Color.FromArgb(150, (Color)rowValues[colNum].BackgroundColor));
                    }

                    int maxTextwidth = columnMetaData.ColumnLength * fontMaxCharWidth;
                    g.FillRectangle(cellBackground, NextX, NextY, maxTextwidth, DefaultFont.Height);

                    TextRenderer.DrawText(g, TruncateString(rowValues[colNum].Value, columnMetaData.ColumnLength), DefaultFont, new Point(NextX, NextY + TextOffset), TextColor.Color);
                    NextX += maxTextwidth + _columnGap;
                }
                NextY += DefaultFont.Height + _rowGap;
                NextX = 0;
            }                       
        }

        

        private void DrawOveralHeaders(Graphics g)
        {
            foreach (HeaderLabel headerLabel in GetOverallHeader())
            {
                int maxTextwidth = headerLabel.ColumnLength * fontMaxCharWidth;
                g.FillRectangle(headerLabel.BackgroundColor, NextX, NextY, maxTextwidth, DefaultFont.Height);

                TextRenderer.DrawText(g, TruncateString(headerLabel.Name, headerLabel.ColumnLength), DefaultFont, new Point(NextX, NextY + TextOffset), DefaultTextColorBrush.Color);
                
                NextX += maxTextwidth + _columnGap; 
            }
            NextY += DefaultFont.Height + _rowGap;
            NextX = 0;
        }


        private string TruncateString(String text, int maxStringLength)
        {
            if (text == null) return "";
            return text.Length <= maxStringLength ? text : text.Substring(0, maxStringLength);
        }        
    }

    public class HeaderLabel
    {
        public string Name;
        public int ColumnLength;
        public Brush BackgroundColor;
        public HeaderLabel(string name, int labelLength, Brush backgroundColor)
        {
            Name = name;
            ColumnLength = labelLength;
            BackgroundColor = backgroundColor;
        }
    }

    public class ColumnMetaData
    {
        public string Name;
        public int ColumnLength;
        public ColumnMetaData(string name, int columnLength) {
            Name = name;
            ColumnLength = columnLength;
        }
    }

    public class CellValue
    {
        public String Value;
        public Color? TextColor = null;
        public Color? BackgroundColor = null;
        
        public CellValue(string v, Color? textColor, Color? backgroundColor)
        {
            this.Value = v;
            this.TextColor = textColor;
            this.BackgroundColor = backgroundColor;
        }
    }    
}
