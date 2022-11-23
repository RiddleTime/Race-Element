using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using static ACCManager.HUD.Overlay.Configuration.OverlayConfiguration;

namespace ACCManager.Controls.HUD.Controls.ValueControls
{
    internal class IntegerValueControl : IValueControl<int>, IControl
    {
        private readonly Grid _grid;
        private readonly Label _label;
        private readonly Slider _slider;

        public FrameworkElement Control => _grid;
        public int Value { get; set; }

        public IntegerValueControl(IntRangeAttribute intRange, ConfigField configField)
        {
            _grid = new Grid() { Width = 200, Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)), Cursor = Cursors.Hand };
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8, GridUnitType.Star) });

            _label = new Label()
            {
                Content = Value,
                HorizontalContentAlignment = HorizontalAlignment.Right
            };
            _grid.Children.Add(_label);
            Grid.SetColumn(_label, 0);

            _slider = new Slider()
            {
                Minimum = intRange.Min,
                Maximum = intRange.Max,
                TickFrequency = intRange.Increment,
                IsSnapToTickEnabled = true,
                HorizontalAlignment = HorizontalAlignment.Right,
                Width = 150,
            };
            _slider.ValueChanged += (s, e) =>
            {
                _label.Content = _slider.Value;
            };

            int value = int.Parse(configField.Value.ToString());
            value.Clip(intRange.Min, intRange.Max);
            _slider.Value = value;

            _grid.Children.Add(_slider);
            Grid.SetColumn(_slider, 1);

            Control.MouseWheel += (sender, args) =>
            {
                int delta = args.Delta;
                _slider.Value += delta.Clip(-1, 1) * intRange.Increment;
                args.Handled = true;
            };
        }

        public void Save()
        {
            throw new System.NotImplementedException();
        }
    }
}
