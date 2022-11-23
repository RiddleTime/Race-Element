using ACCManager.Controls.HUD.Controls.ValueControls;
using ACCManager.HUD.Overlay.Configuration;
using Octokit;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
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

        public ListViewItem GenerateOption(string group, string label, PropertyInfo pi)
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

            Label lblControl = GenerateLabel(label);
            grid.Children.Add(lblControl);
            Grid.SetColumn(lblControl, 0);

            IControl valueControl = GenerateValueControl(pi);

            if (valueControl != null)
            {
                Grid.SetColumn(valueControl.Control, 1);
                grid.Children.Add(valueControl.Control);
            }

            return item;
        }

        private Label GenerateLabel(string label)
        {
            return new Label()
            {
                Content = label,
                Margin = new Thickness(0),
                Padding = new Thickness(0),
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private IControl GenerateValueControl(PropertyInfo pi)
        {
            IControl contentControl = null;

            if (pi.PropertyType == typeof(bool))
            {
                IValueControl<bool> boolValueControl = new BooleanValueControl();
                contentControl = (IControl)boolValueControl;
            }

            if (pi.PropertyType == typeof(int))
            {
                IntRangeAttribute intRange = null;
                foreach (Attribute cad in Attribute.GetCustomAttributes(pi))
                    if (cad is IntRangeAttribute)
                        intRange = (IntRangeAttribute)cad;

                IValueControl<int> intValueControl = new IntegerValueControl(intRange);
                contentControl = (IControl)intValueControl;
            }

            if (pi.PropertyType == typeof(byte))
            {
                ByteRangeAttribute byteRange = null;
                foreach (Attribute cad in Attribute.GetCustomAttributes(pi))
                    if (cad is ByteRangeAttribute)
                        byteRange = (ByteRangeAttribute)cad;

                IValueControl<byte> intValueControl = new ByteValueControl(byteRange);
                contentControl = (IControl)intValueControl;
            }

            //else
            //{
            //    //contentControl = new Label()
            //    //{
            //    //    Content = type.Name,
            //    //};
            //}

            if (contentControl == null)
                return null;

            contentControl.Control.HorizontalAlignment = HorizontalAlignment.Center;
            contentControl.Control.VerticalAlignment = VerticalAlignment.Center;

            return contentControl;
        }
    }
}
