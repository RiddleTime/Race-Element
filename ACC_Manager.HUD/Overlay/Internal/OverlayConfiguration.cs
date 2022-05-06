using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.Overlay.Internal
{
    public class OverlayConfiguration
    {
        public OverlayConfiguration()
        {
            LogAllChildMembers();
        }


        public class ConfigField
        {
            public string Name { get; set; }
            public object Value { get; set; }
        }

        public List<ConfigField> GetConfigFields()
        {
            List<ConfigField> configFields = new List<ConfigField>();
            foreach (var nested in this.GetType().GetRuntimeProperties())
            {
                configFields.Add(new ConfigField() { Name = nested.Name, Value = nested.GetValue(this) });
            }

            return configFields;
        }

        public void SetConfigFields(List<ConfigField> configFields)
        {
            foreach (var field in configFields)
            {
                Type type = this.GetType();

                foreach (var prop in type.GetRuntimeProperties())
                {
                    if (prop.Name == field.Name)
                    {
                        prop.SetValue(this, field.Value);
                    }
                }

            }
        }

        private void LogAllChildMembers()
        {
            //Debug.WriteLine("Checking overlay config");
            foreach (var nested in this.GetType().GetRuntimeProperties())
            {
                //Debug.WriteLine($"{nested.Name} - {nested.GetValue(this)}");
                //nested.SetValue(this, true);
            }
        }
    }
}
