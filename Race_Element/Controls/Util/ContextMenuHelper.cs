using MaterialDesignThemes.Wpf;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RaceElement.Controls.Util
{
    internal static class ContextMenuHelper
    {
        public static MenuItem DefaultMenuItem(string header, PackIconKind icon)
        {
            var menuItem = new MenuItem()
            {
                Header = new TextBlock()
                {
                    Text = header,
                    Style = MainWindow.Instance.Resources["MaterialDesignSubtitle2TextBlock"] as Style,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                },
                Cursor = Cursors.Hand,
                Icon = new PackIcon()
                {
                    Kind = icon,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                },
                Style = MainWindow.Instance.Resources["MaterialDesignMenuItem"] as Style,
                Height = 32,
            };

            return menuItem;
        }

        public static ContextMenu DefaultContextMenu()
        {
            int horizontalOffset = -18;
            int verticalOffset = -18;

            var menu = new ContextMenu()
            {
                Margin = new Thickness(horizontalOffset, verticalOffset, horizontalOffset, verticalOffset),
                Padding = new Thickness(0, 0, 0, 0),
                HorizontalOffset = horizontalOffset,
                VerticalOffset = verticalOffset,
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(235, 17, 17, 17)),
            };

            return menu;
        }
    }
}
