using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace RaceElement.HUD.Overlay.Configuration;

public abstract class OverlayConfiguration
{
    [ConfigGrouping("HUD", "General settings")]
    public GenericConfig GenericConfiguration { get; set; } = new GenericConfig();
    public sealed class GenericConfig
    {
        /// <summary>Sets the Visibility of the <see cref="Scale"/>, when true it will show the Scale option in the GUI, make sure you test Scaling before enabling this option.</summary>
        public bool AllowRescale = false;

        [ToolTip("Defines the scale of the overlay.")]
        [FloatRange(0.650f, 3.000f, 0.002f, 3)]
        public float Scale { get; set; } = 1.00f;

        [ToolTip("Sets the transparency of the HUD. This will become noticeable once you active the HUD.")]
        [FloatRange(0.25f, 1f, 0.01f, 2)]
        public float Opacity { get; set; } = 1f;

        [ToolTip("Allows other software to to detect this overlay as a Window, can be used for streaming apps.")]
        public bool Window { get; set; } = false;

        [ToolTip("When streaming with Window enabled turn this off when you don't want to see the actual overlay on top of your game.")]
        public bool AlwaysOnTop { get; set; } = true;
    }

    public OverlayConfiguration()
    {
    }

    public class ConfigField
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public static List<ConfigField> GetConfigFields(OverlayConfiguration overlayConfiguration)
    {
        List<ConfigField> configFields = [];
        var runtimeProperties = overlayConfiguration.GetType().GetRuntimeProperties();
        foreach (PropertyInfo nested in runtimeProperties)
        {
            ConfigGroupingAttribute groupingAttribute;
            if ((groupingAttribute = nested.GetCustomAttribute<ConfigGroupingAttribute>()) != null)
            {
                var nestedValue = nested.GetValue(overlayConfiguration);
                foreach (PropertyInfo subNested in nested.PropertyType.GetRuntimeProperties())
                {
                    configFields.Add(new ConfigField() { Name = $"{nested.Name}.{subNested.Name}", Value = subNested.GetValue(nestedValue) });
                }
            }
            else
            {
                configFields.Add(new ConfigField() { Name = nested.Name, Value = nested.GetValue(overlayConfiguration) });
            }
        }

        return configFields;
    }

    internal void SetConfigFields(List<ConfigField> configFields)
    {
        if (configFields == null)
            return;

        Type type = this.GetType();
        var runtimeProperties = type.GetRuntimeProperties();

        foreach (var field in configFields)
        {
            bool isGrouped = field.Name.Contains('.');
            if (isGrouped)
            {
                string[] groupSplit = field.Name.Split('.');
                string groupName = groupSplit[0];
                string propName = groupSplit[1];
                //Debug.WriteLine($"Group: {groupName}, PropName: {propName}");
                foreach (var prop in runtimeProperties)
                {
                    if (prop.Name == groupName)
                    {
                        var nestedValue = prop.GetValue(this);
                        foreach (PropertyInfo subNested in nestedValue.GetType().GetRuntimeProperties())
                        {
                            if (subNested.Name == propName)
                            {
                                if (subNested.PropertyType == typeof(Single))
                                    subNested.SetValue(nestedValue, Single.Parse(field.Value.ToString()));
                                else if (subNested.PropertyType == typeof(int))
                                    subNested.SetValue(nestedValue, int.Parse(field.Value.ToString()));
                                else if (subNested.PropertyType == typeof(bool))
                                    subNested.SetValue(nestedValue, field.Value);
                                else if (subNested.PropertyType == typeof(string))
                                    subNested.SetValue(nestedValue, field.Value);
                                else if (subNested.PropertyType == typeof(byte))
                                    subNested.SetValue(nestedValue, byte.Parse(field.Value.ToString()));
                                else if (subNested.PropertyType == typeof(Color))
                                    subNested.SetValue(nestedValue, ColorFromToString(field.Value.ToString()));
                                else if (subNested.PropertyType.BaseType == typeof(Enum))
                                {
                                    try
                                    {
                                        var enumList = Enum.GetValues(subNested.PropertyType).Cast<Enum>().ToList();
                                        var enumItem = enumList.FirstOrDefault(x => x.ToString().Equals(field.Value.ToString()));
                                        subNested.SetValue(nestedValue, enumItem);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.WriteLine(e.ToString());
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine($"{prop.PropertyType} - {nestedValue}");
                                }


                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var prop in runtimeProperties)
                {
                    if (prop.Name == field.Name)
                    {
                        if (prop.PropertyType == typeof(Single))
                            prop.SetValue(this, Single.Parse(field.Value.ToString()));
                        else if (prop.PropertyType == typeof(int))
                            prop.SetValue(this, int.Parse(field.Value.ToString()));
                        else if (prop.PropertyType == typeof(bool))
                            prop.SetValue(this, field.Value);
                        else if (prop.PropertyType == typeof(string))
                            prop.SetValue(this, field.Value);
                        else if (prop.PropertyType == typeof(byte))
                            prop.SetValue(this, byte.Parse(field.Value.ToString()));
                        else if (prop.PropertyType == typeof(Color))
                            prop.SetValue(this, ColorFromToString(field.Value.ToString()));
                        else if (prop.PropertyType.BaseType == typeof(Enum))
                        {
                            var enumList = Enum.GetValues(prop.PropertyType).Cast<Enum>().ToList();
                            var enumItem = enumList.FirstOrDefault(x => x.ToString().Equals(field.Value.ToString()));
                            prop.SetValue(this, enumItem);
                        }
                    }
                }
            }
        }
    }

    public List<PropertyInfo> GetProperties()
    {
        List<PropertyInfo> properties = this.GetType().GetRuntimeProperties().ToList();
        return properties;
    }

    private System.Drawing.Color ColorFromToString(string value)
    {
        if (value.Contains("#"))
        {
            value = value.Replace("Color [", "");
            value = value.Replace("]", "");
            return (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(value);
        }


        if (value.Contains("A") && value.Contains("R") && value.Contains("G") && value.Contains("B"))
        {
            try
            {
                int a = int.Parse(value.Split('A')[1].Split(',')[0].Replace("=", ""));
                int r = int.Parse(value.Split('R')[1].Split(',')[0].Replace("=", ""));
                int g = int.Parse(value.Split('G')[1].Split(',')[0].Replace("=", ""));
                int b = int.Parse(value.Split('B')[1].Split(']')[0].Replace("=", ""));
                return System.Drawing.Color.FromArgb(a, r, g, b);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
                return Color.Red;
            }
        }
        else
        {
            try
            {
                string[] split = value.Split(',');
                int r = int.Parse(split[0]);
                int g = int.Parse(split[1]);
                int b = int.Parse(split[2]);
                return System.Drawing.Color.FromArgb(255, r, g, b);
            }
            catch (Exception ea)
            {
                Debug.WriteLine(ea);
                return Color.Red;
            }
        }

    }
}
