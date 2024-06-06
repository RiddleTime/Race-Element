using System.Windows;
using System.Windows.Controls;
using RaceElement.HUD.Overlay.Configuration;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using MaterialDesignThemes.Wpf;

namespace RaceElement.Controls.HUD.Controls.ValueControls
{
    internal class LinkValueControl : IValueControl<LinkOption>, IControl
    {
        private readonly Grid _grid;
        private readonly LinkOption _link;
        public LinkOption Value { get => new(); set => _ = value; }

        public FrameworkElement Control => _grid;

        public LinkValueControl(LinkOption link, string linkText = null)
        {
            _link = link;
            _grid = new Grid()
            {
                Width = ControlConstants.ControlWidth + ControlConstants.LabelWidth + 20,
                Margin = new Thickness(0, 0, 7, 0),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand
            };
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            MenuItem item = new()
            {
                Width = _grid.Width,
                MinWidth = _grid.Width,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Header = new TextBlock()
                {
                    Text = linkText ?? link.Link,
                    Style = MainWindow.Instance.Resources["MaterialDesignSubtitle2TextBlock"] as Style,
                    Margin = new Thickness(0, 0, -75, 0),
                    Padding = new Thickness(0),
                    Width = _grid.Width,
                    MinWidth = _grid.Width,
                },
                Icon = new PackIcon()
                {
                    Kind = PackIconKind.Link,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                },
            };
            item.Click += Button_Click;
            _grid.Children.Add(item);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd",
                Arguments = $"/c start {_link.Link}",
                WindowStyle = ProcessWindowStyle.Hidden,
            });
        }

        public void Save()
        {

        }
    }
}
