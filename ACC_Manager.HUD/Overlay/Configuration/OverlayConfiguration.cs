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

        public List<ConfigField> GetConfigFields()
        {
            List<ConfigField> configFields = new List<ConfigField>();
            var runtimeProperties = this.GetType().GetRuntimeProperties();
            foreach (PropertyInfo nested in runtimeProperties)
            {
                ConfigGroupingAttribute groupingAttribute;
                if ((groupingAttribute = nested.GetCustomAttribute<ConfigGroupingAttribute>()) != null)
                {
                    var nestedValue = nested.GetValue(this);
                    foreach (PropertyInfo subNested in nested.PropertyType.GetRuntimeProperties())
                    {
                        configFields.Add(new ConfigField() { Name = $"{nested.Name}.{subNested.Name}", Value = subNested.GetValue(nestedValue) });
                    }
                }
                else
                {
                    configFields.Add(new ConfigField() { Name = nested.Name, Value = nested.GetValue(this) });
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
                        if (prop.PropertyType == typeof(Single))
                            prop.SetValue(this, Single.Parse(field.Value.ToString()));

                        if (prop.PropertyType == typeof(int))
                            prop.SetValue(this, int.Parse(field.Value.ToString()));

                        if (prop.PropertyType == typeof(bool))
                            prop.SetValue(this, field.Value);

                        if (prop.PropertyType == typeof(byte))
                            prop.SetValue(this, byte.Parse(field.Value.ToString()));
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
