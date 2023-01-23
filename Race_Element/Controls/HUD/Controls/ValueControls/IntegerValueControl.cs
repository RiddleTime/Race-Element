using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RaceElement.Util.SystemExtensions;
using RaceElement.HUD.Overlay.Configuration;
using static RaceElement.HUD.Overlay.Configuration.OverlayConfiguration;

namespace RaceElement.Controls.HUD.Controls.ValueControls
{
    internal class IntegerValueControl : IValueControl<int>, IControl
    {
        private readonly Grid _grid;
        private readonly Label _label;
        private readonly Slider _slider;

        public FrameworkElement Control => _grid;
        public int Value { get; set; }
        private readonly ConfigField _field;

        public IntegerValueControl(IntRangeAttribute intRange, ConfigField configField)
        {
            _field = configField;
            _grid = new Grid()
            {
                Width = 250,
                Margin = new Thickness(0, 0, 7, 0),
                Background = new SolidColorBrush(Color.FromArgb(140, 2, 2, 2)),
                Cursor = Cursors.Hand
            };
            _grid.PreviewMouseLeftButtonUp += (s, e) => Save();
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });
            _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(8, GridUnitType.Star) });

            _label = new Label()
            {
                Content = _field.Value,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                FontWeight = FontWeights.Bold,
                FontSize = 13,
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
                VerticalAlignment = VerticalAlignment.Center,
                Width = 180,
            };
            _slider.ValueChanged += (s, e) =>
            {
                _field.Value = _slider.Value.ToString();
                _label.Content = _field.Value;
            };
            int value = int.Parse(_field.Value.ToString());
            value.Clip(intRange.Min, intRange.Max);
            _slider.Value = value;
            _grid.Children.Add(_slider);
            Grid.SetColumn(_slider, 1);

            _label.Content = _field.Value;
            Control.MouseWheel += (sender, args) =>
            {
                int delta = args.Delta;
                _slider.Value += delta.Clip(-1, 1) * intRange.Increment;
                args.Handled = true;
                Save();
            };
        }

        public void Save()
        {
            ConfigurationControls.SaveOverlayConfigField(_field);
        }
    }
}
