using ACCManager.HUD.Overlay.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ACCManager.Controls.HUD.Controls.ValueControls
{
    internal class ByteValueControl : IValueControl<byte>, IControl
    {
        private readonly Slider _slider;

        public Control Control => _slider;
        public byte Value { get; set; }

        public ByteValueControl(ByteRangeAttribute intRange)
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
