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
            TableRow row = new TableRow();
            TableCell cellA = new TableCell(a) { TextAlignment = TextAlignment.Right };
            TableCell cellB = new TableCell(b) { TextAlignment = TextAlignment.Center };
            TableCell cellC = new TableCell(c) { TextAlignment = TextAlignment.Left };
            row.Cells.Add(cellA);
            row.Cells.Add(cellB);
            row.Cells.Add(cellC);
            return row;
        }

        public static TableRow GetTableRowCompare(string value1, string title, string value2)
        {
            TableRow row = new TableRow();
            TableCell cellA = new TableCell(GetDefaultParagraph(value1, new Thickness(0, 0, 5, 0))) { TextAlignment = TextAlignment.Right };
            TableCell cellB = new TableCell(GetDefaultParagraph(title)) { TextAlignment = TextAlignment.Center };
            TableCell cellC = new TableCell(GetDefaultParagraph(value2, new Thickness(5, 0, 0, 0))) { TextAlignment = TextAlignment.Left };

            if (!value1.Equals(value2))
            {
                cellB.Background = new SolidColorBrush(Colors.DarkOrange);
            }

            row.Cells.Add(cellA);
            row.Cells.Add(cellB);
            row.Cells.Add(cellC);
            return row;
        }

        public static TableRow GetTableRowCompare(double value1, string title, double value2, int denominator = 0)
        {
            return GetTableRowCompare(new double[] { value1 }, title, new double[] { value2 }, denominator);
        }

        public static TableRow GetTableRowCompare(double[] values1, string title, double[] values2, int denominator = 0)
        {
            if (values1.Length != values2.Length)
                throw new ArgumentException("Both of the values arrays have to be the same length.");

            List<TableCell> cells1 = new List<TableCell>();
            List<TableCell> cells2 = new List<TableCell>();

            bool different = false;

            for (int i = 0; i < values1.Length; i++)
            {
                TableCell cell1 = new TableCell(GetDefaultParagraph($"{values1[i].ToString($"F{denominator}")}", new Thickness(0, 0, 5, 0))) { TextAlignment = TextAlignment.Right };
                TableCell cell2 = new TableCell(GetDefaultParagraph($"{values2[i].ToString($"F{denominator}")}", new Thickness(5, 0, 0, 0))) { TextAlignment = TextAlignment.Left };

                if (values1[i] > values2[i])
                {
                    cell1.Blocks.First().Foreground = new SolidColorBrush(Colors.LimeGreen);
                    cell1.Blocks.First().FontWeight = FontWeights.Bold;
                    different = true;
                }

                if (values1[i] < values2[i])
                {
                    cell2.Blocks.First().Foreground = new SolidColorBrush(Colors.LimeGreen);
                    cell2.Blocks.First().FontWeight = FontWeights.Bold;
                    different = true;
                }

                cells1.Add(cell1);
                cells2.Add(cell2);
            }

            TableRow row = new TableRow();

            for (int i = cells1.Count - 1; i >= 0; i--)
                row.Cells.Add(cells1[i]);

            TableCell header = new TableCell(GetDefaultParagraph(title)) { TextAlignment = TextAlignment.Center };
            if (different)
                header.Background = new SolidColorBrush(Colors.DarkOrange);
            row.Cells.Add(header);

            for (int i = 0; i < cells2.Count; i++)
                row.Cells.Add(cells2[i]);

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
