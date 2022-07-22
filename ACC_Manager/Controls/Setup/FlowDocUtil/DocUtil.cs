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
            return table;
        }

        /// <summary>
        /// Header is centered
        /// </summary>
        /// <param name="headerWidth"></param>
        /// <param name="cells"></param>
        /// <returns></returns>
        public static Table GetMultiTable(int headerWidth, int cells)
        {
            if (cells % 2 == 1)
                throw new ArgumentException("Cells requires to be an even number");

            Table table = new Table();

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
            TableCell cellB = new TableCell(b) { TextAlignment = TextAlignment.Center };
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
                TableCell cell1 = new TableCell(GetDefaultParagraph($"{labels[i]}{values1[i].ToString($"F{denominator}")}", new Thickness(0, 0, 5, 0))) { TextAlignment = TextAlignment.Right };
                TableCell cell2 = new TableCell(GetDefaultParagraph($"{labels[i]}{values2[i].ToString($"F{denominator}")}", new Thickness(5, 0, 0, 0))) { TextAlignment = TextAlignment.Left };

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
                    row.Cells.Add(new TableCell());

            for (int i = 0; i < cells1.Count; i++)
                row.Cells.Add(cells1[i]);

            TableCell header = new TableCell(GetDefaultParagraph(title))
            {
                TextAlignment = TextAlignment.Center,
                BorderThickness = new Thickness(2, 0, 2, 0),
                BorderBrush = Brushes.DarkGray
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
                    row.Cells.Add(new TableCell());

            return row;
        }

        public static Paragraph GetDefaultHeader()
        {
            Paragraph content = new Paragraph();
            content.FontSize = 17;
            content.FontWeight = FontWeights.Medium;
            content.Foreground = Brushes.White;
            content.TextAlignment = TextAlignment.Center;
            content.Margin = new Thickness(0, 0, 0, 2);
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
            Paragraph content = new Paragraph();
            content.FontSize = 12;
            content.FontWeight = FontWeights.Medium;
            content.Foreground = Brushes.White;
            content.Margin = new Thickness(0, 0, 0, 0);
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
            List list = new List();
            list.FontSize = 13;
            list.Foreground = Brushes.White;

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
