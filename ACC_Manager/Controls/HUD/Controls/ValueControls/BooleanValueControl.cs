using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ACCManager.Controls.HUD.Controls.ValueControls
{
    internal class BooleanValueControl : IValueControl<bool>, IControl
    {
        private readonly ToggleButton _toggleButton;

        public Control Control => _toggleButton;
        public bool Value { get; set; }

        public BooleanValueControl()
        {
            _toggleButton = new ToggleButton();
        }
    }
}
