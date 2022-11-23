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
            ListViewItem item = new ListViewItem()
            {
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0),
                BorderBrush = System.Windows.Media.Brushes.OrangeRed
            };
            StackPanel panel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            item.Content = panel;
            panel.Children.Add(new ListViewItem()
            {
                Content = new Label()
                {
                    Content = label,
                    Margin = new Thickness(0),
                    Padding = new Thickness(0),
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                }
            });

            if (type == typeof(bool))
            {
                panel.Children.Add(new ToggleButton()
                {
                    HorizontalContentAlignment = HorizontalAlignment.Right
                });
            }
            else
            {
                panel.Children.Add(new Label()
                {
                    Content = type.Name
                });
            }

            return item;
        }
    }
}
