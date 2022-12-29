using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RaceElement.Controls.HUD.Controls.ValueControls
{
    internal interface IControl
    {
        FrameworkElement Control { get; }
    }

    internal interface IValueControl<T> : IControl
    {
        T Value { get; set; }

        void Save();
    }
}
