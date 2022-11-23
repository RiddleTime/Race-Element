using System.Windows.Controls;
using System.Windows;
using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using System.Windows.Media;
using System.Windows.Input;
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;

namespace ACCManager.Controls.HUD.Controls.ValueControls
{
    internal class FloatValueControl : IValueControl<float>, IControl
    {
        private readonly Grid _grid;
        private readonly Label _label;
        private readonly Slider _slider;

        public FrameworkElement Control => _grid;
        public float Value { get; set; }
        private readonly FloatRangeAttribute _floatRange;

        public FloatValueControl(FloatRangeAttribute floatRange, ConfigField configField)
        {
            _floatRange = floatRange;

            _grid = new Grid()
            {
                Width = 220,
                Margin = new Thickness(0, 0, 7, 0),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand
            };
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8, GridUnitType.Star) });

            _label = new Label()
            {
                HorizontalContentAlignment = HorizontalAlignment.Right
            };
            _grid.Children.Add(_label);
            Grid.SetColumn(_label, 0);

            _slider = new Slider()
            {
                Minimum = _floatRange.Min,
                Maximum = _floatRange.Max,
                TickFrequency = _floatRange.Increment,
                IsSnapToTickEnabled = true,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 150
            };
            _slider.ValueChanged += (s, e) => UpdateLabel();

            float value = float.Parse(configField.Value.ToString());
            value.Clip(floatRange.Min, floatRange.Max);
            _slider.Value = value;

            _grid.Children.Add(_slider);
            Grid.SetColumn(_slider, 1);

            Control.MouseWheel += (sender, args) =>
            {
                int delta = args.Delta;
                _slider.Value += delta.Clip(-1, 1) * _floatRange.Increment;
                args.Handled = true;
            };
        }

        private void UpdateLabel()
        {
            _label.Content = $"{_slider.Value.ToString($"F{_floatRange.Decimals}")}";
        }

        public void Save()
        {
            throw new System.NotImplementedException();
        }
    }
}
