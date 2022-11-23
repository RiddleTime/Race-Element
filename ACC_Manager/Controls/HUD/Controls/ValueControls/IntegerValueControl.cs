using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using ACCManager.HUD.Overlay.Configuration;

namespace ACCManager.Controls.HUD.Controls.ValueControls
{
    internal class IntegerValueControl : IValueControl<int>, IControl
    {
        private readonly Slider _slider;

        public Control Control => _slider;
        public int Value { get; set; }

        public IntegerValueControl(IntRangeAttribute intRange)
        {
            _slider = new Slider()
            {
                Minimum = intRange.Min,
                Maximum = intRange.Max,
                TickFrequency = intRange.Increment,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Width = 170
            };
        }
    }
}
