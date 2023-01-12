using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace RaceElement.HUD.Overlay.Configuration
{
    public abstract class OverlayConfiguration
    {
        public bool AllowRescale = false;

        [ConfigGrouping("HUD", "General settings")]
        public GenericConfig GenericConfiguration { get; set; } = new GenericConfig();
        public class GenericConfig
        {
            [ToolTip("Defines the scale of the overlay.")]
            [FloatRange(0.50f, 2.00f, 0.01f, 2)]
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
            List<ConfigField> configFields = new List<ConfigField>();
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
                bool isGrouped = field.Name.Contains(".");
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
                                    else
                                    if (subNested.PropertyType == typeof(int))
                                        subNested.SetValue(nestedValue, int.Parse(field.Value.ToString()));
                                    else
                                    if (subNested.PropertyType == typeof(bool))
                                        subNested.SetValue(nestedValue, field.Value);
                                    else
                                    if (subNested.PropertyType == typeof(byte))
                                        subNested.SetValue(nestedValue, byte.Parse(field.Value.ToString()));
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
                            else
                            if (prop.PropertyType == typeof(int))
                                prop.SetValue(this, int.Parse(field.Value.ToString()));
                            else
                            if (prop.PropertyType == typeof(bool))
                                prop.SetValue(this, field.Value);
                            else
                            if (prop.PropertyType == typeof(byte))
                                prop.SetValue(this, byte.Parse(field.Value.ToString()));
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

    }
}
