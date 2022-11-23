using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ACCManager.Controls.HUD.Controls.ValueControls
{
    internal interface IControl
    {
        Control Control { get; }
    }

    internal interface IValueControl<T>
    {
        T Value { get; set; }
    }
}
