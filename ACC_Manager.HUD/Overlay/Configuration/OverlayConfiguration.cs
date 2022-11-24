using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ACCManager.HUD.Overlay.Configuration
{
    public abstract class OverlayConfiguration
    {
        public bool AllowRescale = false;
        public float Scale { get; set; } = 1.0f;

        [ConfigGrouping("HUD", "General settings")]
        public GenericConfig GenericConfiguration { get; set; } = new GenericConfig();
        public class GenericConfig
        {
            [ToolTip("Defines the scale of the overlay.")]
            [FloatRange(0.5f, 2.0f, 0.01f, 2)]
            public float Scale { get; set; } = 1.0f;

            [ToolTip("Allows other software to to detect this overlay as a Window, can be used for streaming apps.")]
            public bool Window { get; set; } = false;

            [ToolTip("When streaming with Window enabled turn this off when you don't want to see the actual overlay on top of your game.")]
            public bool AlwaysOnTop { get; set; } = true;
        }


        [ToolTip("Allows other software to to detect this overlay as a Window, can be used for streaming apps.")]
        public bool Window { get; set; } = false;

        [ToolTip("When streaming with Window enabled turn this off when you don't want to see the actual overlay on top of your game.")]
        public bool AlwaysOnTop { get; set; } = true;

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

            foreach (var field in configFields)
            {
                Type type = this.GetType();

                foreach (var prop in type.GetRuntimeProperties())
                {
                    if (prop.Name == field.Name)
                    {
                        ConfigGroupingAttribute groupingAttribute;
                        if ((groupingAttribute = type.GetCustomAttribute<ConfigGroupingAttribute>()) != null)
                        {
                            var nestedValue = prop.GetValue(this);
                            foreach (PropertyInfo subNested in nestedValue.GetType().GetRuntimeProperties())
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

                                subNested.SetValue(nestedValue, Single.Parse(field.Value.ToString()));
                                //configFields.Add(new ConfigField() { Name = $"{prop.Name}.{subNested.Name}", Value = subNested.GetValue(nestedValue) });
                            }
                        }
                        else
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
