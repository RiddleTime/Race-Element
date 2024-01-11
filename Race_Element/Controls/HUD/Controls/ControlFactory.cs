using RaceElement.Controls.HUD.Controls.ValueControls;
using RaceElement.HUD.Overlay.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;
using Label = System.Windows.Controls.Label;

namespace RaceElement.Controls.HUD.Controls;

internal class ControlFactory
{
    public static ControlFactory Instance { get; private set; } = new ControlFactory();

    public ListViewItem GenerateOption(string group, string label, PropertyInfo pi, ConfigField configField)
    {
        Grid grid = new() { Margin = new Thickness(0, 0, 0, 2) /*Height = 26*/ };
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(120, GridUnitType.Pixel) });
        grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(310, GridUnitType.Pixel) });

        ListViewItem item = new()
        {
            Content = grid,
            Margin = new Thickness(0, 0, -1, 0),
            Padding = new Thickness(0),
            BorderThickness = new Thickness(1, 0, 0, 0),
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
        lblControl.HorizontalAlignment = HorizontalAlignment.Left;
        lblControl.VerticalAlignment = VerticalAlignment.Top;

        // add generated control but only if the generated label is not null
        IControl valueControl = GenerateValueControl(pi, configField);
        if (valueControl != null)
        {
            Grid.SetColumn(valueControl.Control, 1);
            grid.Children.Add(valueControl.Control);

            valueControl.Control.HorizontalAlignment = HorizontalAlignment.Center;
            valueControl.Control.VerticalAlignment = VerticalAlignment.Center;
        }

        return item;
    }

    private Label GenerateLabel(string label)
    {
        return new Label()
        {
            Content = string.Concat(label.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' '),
            Margin = new Thickness(0, 5, 0, -5),
            Padding = new Thickness(7, 0, 0, 0),
            FontWeight = FontWeights.Medium,
            FontSize = 13.5,
        };
    }

    private IControl GenerateValueControl(PropertyInfo pi, ConfigField configField)
    {
        IControl contentControl = null;

        switch (pi.PropertyType)
        {
            case Type _ when pi.PropertyType == typeof(bool):
                {
                    IValueControl<bool> boolValueControl = new BooleanValueControl(configField);
                    contentControl = boolValueControl;
                    break;
                }
            case Type _ when pi.PropertyType == typeof(int):
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
                    break;
                }
            case Type _ when pi.PropertyType == typeof(byte):
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
                    break;
                }
            case Type _ when pi.PropertyType == typeof(float):
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
                    break;
                }
            case Type _ when pi.PropertyType == typeof(System.Drawing.Color):
                {
                    IValueControl<System.Drawing.Color> valueControl = new ColorValueControl(configField);
                    contentControl = valueControl;
                    break;
                }

            case Type _ when pi.PropertyType == typeof(string):
                {
                    StringOptionsAttribute stringOptions = null;
                    foreach (Attribute customAttribute in Attribute.GetCustomAttributes(pi))
                        if (customAttribute is StringOptionsAttribute stringOptionsAttribute)
                            stringOptions = stringOptionsAttribute;

                    bool isPassword = false;
                    if (stringOptions != null)
                        isPassword = stringOptions.IsPassword;

                    contentControl = new StringValueControl(configField, isPassword);
                    break;
                }

            case Type _ when pi.PropertyType.BaseType == typeof(Enum):
                {
                    contentControl = new EnumValueControl(configField, pi.PropertyType);
                    break;
                }

        }

        if (contentControl == null)
            return null;


        return contentControl;
    }
}
