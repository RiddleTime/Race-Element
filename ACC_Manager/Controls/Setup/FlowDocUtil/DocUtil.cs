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
