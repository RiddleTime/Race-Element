using ACCManager.Controls.HUD.Controls.ValueControls;
using ACCManager.HUD.Overlay.Configuration;
using Octokit;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;
using Label = System.Windows.Controls.Label;

namespace ACCManager.Controls.HUD.Controls
{
    internal class ControlFactory
    {
        public static ControlFactory Instance { get; private set; } = new ControlFactory();

        public ListViewItem GenerateOption(string group, string label, PropertyInfo pi, ConfigField configField)
        {
            Grid grid = new Grid() { Height = 26 };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(240, GridUnitType.Pixel) });

            ListViewItem item = new ListViewItem()
            {
                Content = grid,
                Margin = new Thickness(0, 0, -1, 0),
                Padding = new Thickness(0),
                BorderThickness = new Thickness(1, 0, 0, 0),
                //BorderBrush = System.Windows.Media.Brushes.OrangeRed,
                Background = new SolidColorBrush(Color.FromArgb(120, 0, 0, 0)),
                IsTabStop = false,
                Focusable = false,
                VerticalContentAlignment = VerticalAlignment.Center,
            };

            // add tooltip if exists.
            foreach (Attribute cad in Attribute.GetCustomAttributes(pi))
                if (cad is ToolTipAttribute toolTip)
                {
                    item.ToolTip = toolTip.ToolTip;
                    break;
                }

            // add label
            Label lblControl = GenerateLabel(label);
            grid.Children.Add(lblControl);
            Grid.SetColumn(lblControl, 0);

            // add generated control but only if the generated label is not null
            IControl valueControl = GenerateValueControl(pi, configField);
            valueControl.Control.HorizontalAlignment = HorizontalAlignment.Right;
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
                Content = string.Concat(label.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '),
                Margin = new Thickness(0),
                Padding = new Thickness(7, 0, 0, 0),
                FontWeight = FontWeights.Normal,
                FontSize = 15,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
        }

        private IControl GenerateValueControl(PropertyInfo pi, ConfigField configField)
        {
            IControl contentControl = null;

            if (pi.PropertyType == typeof(bool))
            {
                IValueControl<bool> boolValueControl = new BooleanValueControl(configField);
                contentControl = boolValueControl;
            }
            if (pi.PropertyType == typeof(int))
            {
                IntRangeAttribute intRange = null;
                foreach (Attribute customAttribute in Attribute.GetCustomAttributes(pi))
                    if (customAttribute is IntRangeAttribute intRangeAttribute)
                        intRange = intRangeAttribute;

                if (intRange != null)
                {
                    IValueControl<int> intValueControl = new IntegerValueControl(intRange, configField);
                    contentControl = intValueControl;
                }
            }
            if (pi.PropertyType == typeof(byte))
            {
                ByteRangeAttribute byteRange = null;
                foreach (Attribute customattribute in Attribute.GetCustomAttributes(pi))
                    if (customattribute is ByteRangeAttribute byteRangeAttribute)
                        byteRange = byteRangeAttribute;

                if (byteRange != null)
                {
                    IValueControl<byte> intValueControl = new ByteValueControl(byteRange, configField);
                    contentControl = intValueControl;
                }
            }
            if (pi.PropertyType == typeof(float))
            {
                FloatRangeAttribute floatRange = null;
                foreach (Attribute customAttribute in Attribute.GetCustomAttributes(pi))
                    if (customAttribute is FloatRangeAttribute floatRangeAttribute)
                        floatRange = floatRangeAttribute;

                if (floatRange != null)
                {
                    IValueControl<float> floatValueControl = new FloatValueControl(floatRange, configField);
                    contentControl = floatValueControl;
                }
            }


            if (contentControl == null)
                return null;

            contentControl.Control.HorizontalAlignment = HorizontalAlignment.Center;
            contentControl.Control.VerticalAlignment = VerticalAlignment.Center;

            return contentControl;
        }
    }
}
