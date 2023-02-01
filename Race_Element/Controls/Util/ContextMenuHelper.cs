using MaterialDesignThemes.Wpf;
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
                Header = header,
                Cursor = Cursors.Hand,
                Icon = new PackIcon()
                {
                    Kind = icon,
                    Height = 36,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Style = MainWindow.Instance.Resources["MaterialDesignMenuItem"] as Style,
                Height = 36,
            };

            return menuItem;
        }

        public static ContextMenu DefaultContextMenu()
        {
            int horizontalOffset = -20;
            int verticalOffset = -16;

            return new ContextMenu()
            {
                Margin = new Thickness(horizontalOffset, verticalOffset, horizontalOffset, verticalOffset),
                Padding = new Thickness(0, 0, 0, 0),
                HorizontalOffset = horizontalOffset,
                VerticalOffset = verticalOffset,
                UsesItemContainerTemplate = true,
                Background = new SolidColorBrush(Color.FromArgb(220, 0, 0, 0)),
            };
        }
    }
}
