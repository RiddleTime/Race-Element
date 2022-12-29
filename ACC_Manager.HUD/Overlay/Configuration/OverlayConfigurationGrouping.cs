using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.Overlay.Configuration
{
    public class ConfigGroupingAttribute : Attribute
    {
        public string Title;
        public string Description;

        public ConfigGroupingAttribute(string title, string description)
        {
            Title = title;
            Description = description;
        }
    }
}
