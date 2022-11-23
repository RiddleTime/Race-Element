using Octokit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Label = System.Windows.Controls.Label;

namespace ACCManager.Controls.HUD.Controls
{
    internal class ControlFactory
    {
        public static ControlFactory Instance { get; private set; } = new ControlFactory();

        public ListViewItem GenerateOption(string group, string label, Type type)
        {
            Grid grid = new Grid() { Height = 30 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(70, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(150, GridUnitType.Star) });
            ListViewItem item = new ListViewItem()
            {
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                BorderBrush = System.Windows.Media.Brushes.OrangeRed,
                Content = grid
            };

            Label lblControl = new Label()
            {
                Content = label,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(lblControl);

            ContentControl contentControl = null;
            if (type == typeof(bool))
            {
                contentControl = new ToggleButton();
            }
            else
            {
                contentControl = new Label()
                {
                    Content = type.Name,
                };
            }
            contentControl.HorizontalAlignment = HorizontalAlignment.Center;
            contentControl.VerticalAlignment = VerticalAlignment.Center;

            grid.Children.Add(contentControl);

            Grid.SetColumn(lblControl, 0);
            if (contentControl != null)
                Grid.SetColumn(contentControl, 1);

            return item;
        }
    }
}
