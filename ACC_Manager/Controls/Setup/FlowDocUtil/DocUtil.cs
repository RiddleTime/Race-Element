using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace ACCManager.Controls.Setup.FlowDocUtil
{
    public static class DocUtil
    {
        public static Table GetTable(int headerWidth, int valueWidth)
        {
            Table table = new Table();
            TableColumn columnTitle = new TableColumn();
            columnTitle.Width = new GridLength(headerWidth, GridUnitType.Star);
            table.Columns.Add(columnTitle);

            TableColumn columnValues = new TableColumn();
            columnValues.Width = new GridLength(valueWidth, GridUnitType.Star);
            table.Columns.Add(columnValues);

            table.Margin = new Thickness(0);
            table.CellSpacing = 0;
            table.LineStackingStrategy = LineStackingStrategy.MaxHeight;
            return table;
        }

        public static Table GetLeftAllignedTable(int headerWidth, int cells)
        {
            Table table = new Table();

            int cellWidth = (100 - headerWidth) / cells;

            TableColumn columnHeader = new TableColumn();
            columnHeader.Width = new GridLength(headerWidth, GridUnitType.Star);
            table.Columns.Add(columnHeader);

            for (int i = 0; i < cells; i++)
            {
                TableColumn column = new TableColumn();
                column.Width = new GridLength(cellWidth, GridUnitType.Star);
                table.Columns.Add(column);
            }

            table.Margin = new Thickness(0);
            table.CellSpacing = 0;
            return table;
        }

        /// <summary>
        /// Header is centered
        /// </summary>
        /// <param name="headerWidth"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static Table GetCenteredTable(int headerWidth, int cells)
        {
            if (cells % 2 == 1)
                throw new ArgumentException("Cells requires to be an even number");

            Table table = new Table() { Padding = new Thickness(0) };

            int cellWidth = (100 - headerWidth) / cells;

            for (int i = 0; i < cells / 2; i++)
            {
                TableColumn column = new TableColumn();
                column.Width = new GridLength(cellWidth, GridUnitType.Star);
                table.Columns.Add(column);
            }

            TableColumn columnHeader = new TableColumn();
            columnHeader.Width = new GridLength(headerWidth, GridUnitType.Star);
            table.Columns.Add(columnHeader);

            for (int i = 0; i < cells / 2; i++)
            {
                TableColumn column = new TableColumn();
                column.Width = new GridLength(cellWidth, GridUnitType.Star);
                table.Columns.Add(column);
            }

            table.Margin = new Thickness(0);
            table.LineStackingStrategy = LineStackingStrategy.MaxHeight;
            table.CellSpacing = 0;
            return table;
        }

        public static Table GetTable(int valueWidth1, int headerWidth, int valueWidth2)
        {
            Table table = new Table();
            TableColumn columnValues1 = new TableColumn();
            columnValues1.Width = new GridLength(valueWidth1, GridUnitType.Star);
            table.Columns.Add(columnValues1);

            TableColumn columnTitle = new TableColumn();
            columnTitle.Width = new GridLength(headerWidth, GridUnitType.Star);
            table.Columns.Add(columnTitle);

            TableColumn columnValues2 = new TableColumn();
            columnValues2.Width = new GridLength(valueWidth2, GridUnitType.Star);
            table.Columns.Add(columnValues2);

            table.Margin = new Thickness(0);
            return table;
        }

        public static TableRow GetTableRow(string title, string value)
        {
            TableRow row = new TableRow();
            row.Cells.Add(new TableCell(GetDefaultParagraph(title)));
            row.Cells.Add(new TableCell(GetDefaultParagraph(value)));
            return row;
        }

        public static TableRow GetTableRow(string value1, string title, string value2)
        {
            TableRow row = new TableRow();
            TableCell cellA = new TableCell(GetDefaultParagraph(value1)) { TextAlignment = TextAlignment.Right };
            TableCell cellB = new TableCell(GetDefaultParagraph(title)) { TextAlignment = TextAlignment.Center };
            TableCell cellC = new TableCell(GetDefaultParagraph(value2)) { TextAlignment = TextAlignment.Left };
            row.Cells.Add(cellA);
            row.Cells.Add(cellB);
            row.Cells.Add(cellC);
            return row;
        }

        public static TableRow GetTableRow(Paragraph a, Paragraph b, Paragraph c)
        {
            return GetTableRow(a, b, c, 3);
        }

        public static TableRow GetTableRow(Paragraph a, Paragraph b, Paragraph c, int cellCount)
        {
            int cellsToAdd = 0;
            if (cellCount > 3)
                cellsToAdd = (cellCount - 1) / 2;

            TableRow row = new TableRow();
            TableCell cellA = new TableCell(a) { TextAlignment = TextAlignment.Right };
            if (cellsToAdd > 0)
                cellA.ColumnSpan = cellsToAdd;
            TableCell cellB = new TableCell(b) { TextAlignment = TextAlignment.Center, Padding = new Thickness(0, 3, 0, 3) };
            TableCell cellC = new TableCell(c) { TextAlignment = TextAlignment.Left };
            if (cellsToAdd > 0)
                cellC.ColumnSpan = cellsToAdd;

            row.Cells.Add(cellA);
            row.Cells.Add(cellB);
            row.Cells.Add(cellC);

            return row;
        }


        public static TableRow GetTableRowCompare(string value1, string title, string value2, int cellCount)
        {
            int cellsToAdd = 0;
            if (cellCount > 3)
                cellsToAdd = (cellCount - 1) / 2;


            TableRow row = new TableRow();
            TableCell cellA = new TableCell(GetDefaultParagraph(value1, new Thickness(0, 0, 5, 0))) { TextAlignment = TextAlignment.Right };
            if (cellsToAdd > 0)
                cellA.ColumnSpan = cellsToAdd;
            TableCell cellB = new TableCell(GetDefaultParagraph(title))
            {
                TextAlignment = TextAlignment.Center,
                BorderThickness = new Thickness(2, 0, 2, 0),
                BorderBrush = Brushes.DarkGray
            };
            if (!value1.Equals(value2))
            {
                cellB.Blocks.First().Foreground = Brushes.DarkOrange;
                cellB.BorderBrush = Brushes.DarkOrange;
            }

            TableCell cellC = new TableCell(GetDefaultParagraph(value2, new Thickness(5, 0, 0, 0))) { TextAlignment = TextAlignment.Left };
            if (cellsToAdd > 0)
                cellC.ColumnSpan = cellsToAdd;



            row.Cells.Add(cellA);
            row.Cells.Add(cellB);
            row.Cells.Add(cellC);

            return row;
        }

        public static TableRow GetTableRowLeft(string title, double value, int cellCount, int denominator = 0)
        {
            return GetTableRowLeft(title, new string[1], new double[] { value }, cellCount, denominator);
        }

        public static TableRow GetTableRowLeft(string title, string[] labels, double[] values, int cellCount, int denominator = 0)
        {
            if (values.Length == 0)
                throw new ArgumentException("Provide at least 1 value.");
            if (labels.Length == 0)
                labels = new string[values.Length];

            List<TableCell> tableCells = new List<TableCell>();

            for (int i = 0; i != values.Length; i++)
            {
                TableCell cell = new TableCell(GetDefaultParagraph($"{labels[i]}{values[i].ToString($"F{denominator}")}", new Thickness(3, 0, 3, 0)))
                {
                    TextAlignment = TextAlignment.Left,
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(0),
                };
                tableCells.Add(cell);
            }

            TableRow row = new TableRow();

            int spacingCells = 0;
            if (cellCount > values.Count() + 1)
                spacingCells = (cellCount - (values.Count() + 1));

            TableCell header = new TableCell(GetDefaultParagraph(title, new Thickness(3, 0, 3, 0)))
            {
                TextAlignment = TextAlignment.Left,
                BorderThickness = new Thickness(2, 0, 2, 0),
                Padding = new Thickness(3, 0, 0, 0),
                BorderBrush = Brushes.DarkGray
            };


            row.Cells.Add(header);

            for (int i = 0; i < tableCells.Count; i++)
            {
                if (i == 0)
                    tableCells[i].Padding = new Thickness(3, 0, 0, 0);
                row.Cells.Add(tableCells[i]);
            }

            return row;
        }

        public static TableRow GetTableRowLeftTitle(string title, int cellCount)
        {
            TableRow row = new TableRow();

            // add header
            Paragraph pHeader = GetDefaultHeader(18, title);
            pHeader.TextAlignment = TextAlignment.Left;
            pHeader.Margin = new Thickness(3, 5, 0, 3);
            pHeader.FontStyle = FontStyles.Italic;
            TableCell header = new TableCell(pHeader)
            {
                TextAlignment = TextAlignment.Left,
                ColumnSpan = cellCount,
                Padding = new Thickness(0),
            };
            row.Cells.Add(header);

            return row;
        }


        public static TableRow GetTableRowLeft(string title, string value, int cellCount, bool fullValueColumnSpan = false)
        {
            if (!fullValueColumnSpan)
            {
                return GetTableRowLeft(title, new string[] { value }, cellCount);
            }
            else
            {
                TableRow row = new TableRow();

                // set amount of spacing cells on the right
                int spacingCells = 0;
                if (cellCount > 2)
                    spacingCells = (cellCount - 1);

                // add header
                TableCell header = new TableCell(GetDefaultParagraph(title))
                {
                    TextAlignment = TextAlignment.Left,
                    BorderThickness = new Thickness(2, 0, 2, 0),
                    Padding = new Thickness(0),
                    BorderBrush = Brushes.DarkGray
                };
                row.Cells.Add(header);

                TableCell valueCell = new TableCell(GetDefaultParagraph(value, new Thickness(3, 0, 3, 0)))
                {
                    TextAlignment = TextAlignment.Left,
                    Padding = new Thickness(3, 0, 0, 0),
                };
                if (spacingCells > 0)
                    valueCell.ColumnSpan = spacingCells;
                row.Cells.Add(valueCell);

                return row;

            }
        }

        public static TableRow GetTableRowLeft(string title, string[] values, int cellCount)
        {
            if (values.Length == 0)
                throw new ArgumentException("Provide at least 1 value.");

            TableRow row = new TableRow();

            // set amount of spacing cells on the right
            int spacingCells = 0;
            if (cellCount > values.Count() + 1)
                spacingCells = (cellCount - (values.Count() + 1));

            // add header
            TableCell header = new TableCell(GetDefaultParagraph(title))
            {
                TextAlignment = TextAlignment.Left,
                BorderThickness = new Thickness(2, 0, 2, 0),
                Padding = new Thickness(0),
                BorderBrush = Brushes.DarkGray
            };
            row.Cells.Add(header);


            // add values 
            for (int i = 0; i != values.Length; i++)
                row.Cells.Add(new TableCell(GetDefaultParagraph(values[i], new Thickness(3, 0, 3, 0)))
                {
                    TextAlignment = TextAlignment.Left,
                    Padding = new Thickness(0),
                    BorderThickness = new Thickness(0)
                });

            if (spacingCells > 0)
                for (int i = 0; i < spacingCells; i++)
                    row.Cells.Add(new TableCell() { Padding = new Thickness(0), BorderThickness = new Thickness(0) });

            return row;
        }


        public static TableRow GetTableRowCompare(string value1, string title, string value2)
        {
            return GetTableRowCompare(value1, title, value2, 3);
        }

        public static TableRow GetTableRowCompare(double value1, string title, double value2, int cellCount, int denominator = 0)
        {
            return GetTableRowCompare(new double[] { value1 }, title, new double[] { value2 }, cellCount, denominator);
        }

        public static TableRow GetTableRowCompare(double[] values1, string title, double[] values2, int cellCount, int denominator = 0)
        {
            return GetTableRowCompareWithLabels(new string[values1.Length], values1, title, values2, cellCount, denominator);
        }

        public static TableRow GetTableRowCompareWithLabels(string[] labels, double[] values1, string title, double[] values2, int cellCount, int denominator = 0)
        {
            if (values1.Length != values2.Length)
                throw new ArgumentException("Both of the values arrays have to be the same length.");

            List<TableCell> cells1 = new List<TableCell>();
            List<TableCell> cells2 = new List<TableCell>();

            bool different = false;

            for (int i = 0; i < values1.Length; i++)
            {
                TableCell cell1 = new TableCell(GetDefaultParagraph($"{labels[i]}{values1[i].ToString($"F{denominator}")}", new Thickness(0, 0, 5, 0)))
                {
                    TextAlignment = TextAlignment.Right,
                    Padding = new Thickness(0),
                    BorderThickness = new Thickness(0)
                };
                TableCell cell2 = new TableCell(GetDefaultParagraph($"{labels[i]}{values2[i].ToString($"F{denominator}")}", new Thickness(5, 0, 0, 0)))
                {
                    TextAlignment = TextAlignment.Left,
                    Padding = new Thickness(0),
                    BorderThickness = new Thickness(0)
                };

                if (values1[i] > values2[i])
                {
                    cell1.Blocks.First().Foreground = Brushes.LimeGreen;
                    cell2.Blocks.First().Foreground = Brushes.OrangeRed;
                    different = true;
                }

                if (values1[i] < values2[i])
                {
                    cell2.Blocks.First().Foreground = Brushes.LimeGreen;
                    cell1.Blocks.First().Foreground = Brushes.OrangeRed;
                    different = true;
                }

                cells1.Add(cell1);
                cells2.Add(cell2);
            }

            TableRow row = new TableRow();

            int spacingCells = 0;
            if (cellCount > values2.Count() * 2 + 1)
                spacingCells = (cellCount - (values2.Count() * 2 + 1)) / 2;

            // add spacing cells on left side
            if (spacingCells > 0)
                for (int i = 0; i < spacingCells; i++)
                    row.Cells.Add(new TableCell(GetDefaultParagraph())
                    {
                        Padding = new Thickness(0),
                        BorderThickness = new Thickness(0),
                    });

            for (int i = 0; i < cells1.Count; i++)
                row.Cells.Add(cells1[i]);

            TableCell header = new TableCell(GetDefaultParagraph(title))
            {
                TextAlignment = TextAlignment.Center,
                BorderThickness = new Thickness(2, 0, 2, 0),
                BorderBrush = Brushes.DarkGray,
                Padding = new Thickness(0)
            };
            if (different)
            {
                header.BorderBrush = Brushes.DarkOrange;
                header.Blocks.First().Foreground = Brushes.DarkOrange;
            }

            row.Cells.Add(header);

            for (int i = 0; i < cells2.Count; i++)
                row.Cells.Add(cells2[i]);

            // add spacing cells on right side
            if (spacingCells > 0)
                for (int i = 0; i < spacingCells; i++)
                    row.Cells.Add(new TableCell(GetDefaultParagraph())
                    {
                        Padding = new Thickness(0),
                        BorderThickness = new Thickness(0),
                        LineHeight = 1,
                        LineStackingStrategy = LineStackingStrategy.MaxHeight
                    });

            return row;
        }

        public static Paragraph GetDefaultHeader()
        {
            Paragraph content = new Paragraph
            {
                FontSize = 17,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 0)
            };
            return content;
        }

        public static Paragraph GetDefaultHeader(double fontSize)
        {
            Paragraph content = GetDefaultHeader();
            content.FontSize = fontSize;
            return content;
        }

        public static Paragraph GetDefaultHeader(string inlineText)
        {
            Paragraph content = GetDefaultHeader();
            content.Inlines.Add(inlineText);
            return content;
        }

        public static Paragraph GetDefaultHeader(double fontSize, string inlineText)
        {
            Paragraph content = GetDefaultHeader();
            content.FontSize = fontSize;
            content.Inlines.Add(inlineText);
            return content;
        }

        public static Paragraph GetDefaultParagraph()
        {
            Paragraph content = new Paragraph
            {
                FontSize = 12,
                FontWeight = FontWeights.Medium,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 0)
            };
            return content;
        }

        public static Paragraph GetDefaultParagraph(double fontSize)
        {
            Paragraph content = GetDefaultParagraph();
            content.FontSize = fontSize;
            return content;
        }

        public static Paragraph GetDefaultParagraph(string inlineText, Thickness margin)
        {
            Paragraph content = GetDefaultParagraph(inlineText);
            content.Margin = margin;
            return content;
        }

        public static Paragraph GetDefaultParagraph(string inlineText)
        {
            Paragraph content = GetDefaultParagraph();
            content.Margin = new Thickness(5, 0, 0, 0);
            content.Inlines.Add(inlineText);
            return content;
        }

        public static Paragraph GetDefaultParagraph(double fontSize, string inlineText)
        {
            Paragraph content = GetDefaultParagraph(fontSize);
            content.Inlines.Add(inlineText);
            return content;
        }

        public static List GetDefaultList()
        {
            List list = new List
            {
                FontSize = 13,
                Foreground = Brushes.White
            };

            return list;
        }

        public static string GetTrackName(string fileName)
        {
            string[] dashSplit = fileName.Split('\\');
            string trackName = dashSplit[dashSplit.Length - 2];
            trackName = Regex.Replace(trackName, "^[a-z]", m => m.Value.ToUpper());
            trackName = trackName.Replace("_", " ");
            return trackName;
        }

        public static string GetParseName(string fileName)
        {
            string[] dashSplit = fileName.Split('\\');
            string parseName = dashSplit[dashSplit.Length - 3];
            return parseName;
        }

    }
}
