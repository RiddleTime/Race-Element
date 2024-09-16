using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.ProgressBars;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Driving.InputBars;

[Overlay(Name = "Input Bars", Version = 1.00, OverlayType = OverlayType.Drive,
  Description = "Live input bars of throttle and brake.",
  OverlayCategory = OverlayCategory.Inputs,
Authors = ["Reinier Klarenberg"])]
internal sealed class InputBarsOverlay : CommonAbstractOverlay
{
    private readonly InputBarsConfiguration _config = new();

    private CachedBitmap _cachedBackground;

    private readonly HorizontalProgressBar[] _horizontalBars;
    private HorizontalProgressBar _horizontalGasBar;
    private HorizontalProgressBar _horizontalBrakeBar;

    private readonly VerticalProgressBar[] _verticalBars;
    private VerticalProgressBar _verticalGasBar;
    private VerticalProgressBar _verticalBrakeBar;

    private float _throttle = 0f;
    private float _brake = 0f;

    public InputBarsOverlay(Rectangle rectangle) : base(rectangle, "Input Bars")
    {
        RefreshRateHz = _config.Bars.RefreshRate;

        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            _horizontalBars = new HorizontalProgressBar[2];
            Width = _config.Bars.Length + 1;
            Height = _config.Bars.Thickness * 2 + _config.Bars.Spacing + 1;
        }
        else
        {
            _verticalBars = new VerticalProgressBar[2];
            Width = _config.Bars.Thickness * 2 + _config.Bars.Spacing + 1;
            Height = _config.Bars.Length + 1;
        }
    }

    public override void SetupPreviewData()
    {
        _throttle = 0.823f;
        _brake = 0.117f;
    }

    public sealed override void BeforeStart()
    {
        int width = _config.Bars.Thickness * 2 + _config.Bars.Spacing;
        int height = _config.Bars.Length;

        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            width = _config.Bars.Length;
            height = _config.Bars.Thickness * 2 + _config.Bars.Spacing;
        }

        _cachedBackground = new CachedBitmap((int)(width * Scale), (int)(height * Scale), g =>
        {
            if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
            {
                using LinearGradientBrush gradientBrush = new(new Rectangle(0, 0, (int)(_config.Bars.Length * Scale), (int)(_config.Bars.Thickness * Scale)), Color.FromArgb(120, Color.Black), Color.FromArgb(230, Color.Black), LinearGradientMode.Horizontal);

                g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.Bars.Length * Scale), (int)(_config.Bars.Thickness * Scale)), (int)(5 * Scale));
                g.FillRoundedRectangle(gradientBrush, new Rectangle(0, (int)((_config.Bars.Thickness + _config.Bars.Spacing) * Scale), (int)(_config.Bars.Length * Scale), (int)(_config.Bars.Thickness * Scale)), (int)(5 * Scale));
            }
            else
            {
                using LinearGradientBrush gradientBrush = new(new Rectangle(0, 0, (int)(_config.Bars.Thickness * Scale), (int)(height * Scale)), Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);

                g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.Bars.Thickness * Scale), (int)(height * Scale)), (int)(6 * Scale));
                g.FillRoundedRectangle(gradientBrush, new Rectangle((int)((_config.Bars.Thickness + _config.Bars.Spacing) * Scale), 0, (int)(_config.Bars.Thickness * Scale), (int)(height * Scale)), (int)(5 * Scale));
            }
        });

        Brush outlineBrush = new SolidBrush(Color.FromArgb(196, Color.Black));

        _config.Bars.ThrottleFirst = _config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal;
        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            _horizontalBrakeBar = new HorizontalProgressBar(_config.Bars.Length, _config.Bars.Thickness)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.BrakeOpacity, _config.Colors.BrakeColor)),
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = Scale,
                Rounding = 5,
            };
            _horizontalGasBar = new HorizontalProgressBar(_config.Bars.Length, _config.Bars.Thickness)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.ThrottleOpacity, _config.Colors.ThrottleColor)),
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = Scale,
                Rounding = 5,
            };
            if (_config.Bars.ThrottleFirst)
            {
                _horizontalBars[0] = _horizontalGasBar;
                _horizontalBars[1] = _horizontalBrakeBar;
            }
            else
            {
                _horizontalBars[0] = _horizontalBrakeBar;
                _horizontalBars[1] = _horizontalGasBar;
            }
        }
        else
        {
            _verticalBrakeBar = new VerticalProgressBar(_config.Bars.Thickness, _config.Bars.Length)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.BrakeOpacity, _config.Colors.BrakeColor)),
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = Scale,
                Rounding = 5,
            };
            _verticalGasBar = new VerticalProgressBar(_config.Bars.Thickness, _config.Bars.Length)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.ThrottleOpacity, _config.Colors.ThrottleColor)),
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = Scale,
                Rounding = 5,
            };
            if (_config.Bars.ThrottleFirst)
            {
                _verticalBars[0] = _verticalGasBar;
                _verticalBars[1] = _verticalBrakeBar;
            }
            else
            {
                _verticalBars[0] = _verticalBrakeBar;
                _verticalBars[1] = _verticalGasBar;
            }
        }
    }

    public sealed override void BeforeStop() => _cachedBackground?.Dispose();

    public sealed override void Render(Graphics g)
    {
        if (!IsPreviewing)
        {
            _throttle = SimDataProvider.LocalCar.Inputs.Throttle;
            _brake = SimDataProvider.LocalCar.Inputs.Brake;
        }


        ApplyElectronicsColors();

        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            _cachedBackground?.Draw(g, _config.Bars.Length, _config.Bars.Thickness * 2 + _config.Bars.Spacing);

            _horizontalGasBar.Value = _throttle;
            _horizontalBrakeBar.Value = _brake;

            _horizontalBars[0]?.Draw(g, 0, 0);
            _horizontalBars[1]?.Draw(g, 0, _config.Bars.Thickness + _config.Bars.Spacing);
        }
        else
        {
            _cachedBackground?.Draw(g, _config.Bars.Thickness * 2 + _config.Bars.Spacing, _config.Bars.Length);

            _verticalBrakeBar.Value = _brake;
            _verticalGasBar.Value = _throttle;

            _verticalBars[0]?.Draw(g, 0, 0);
            _verticalBars[1]?.Draw(g, _config.Bars.Thickness + _config.Bars.Spacing, 0);
        }
    }

    /// <summary>
    /// Applies a fill color to the brake and gas bar based on electronics
    /// </summary>
    private void ApplyElectronicsColors()
    {
        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            if (SimDataProvider.LocalCar.Electronics.AbsActivation > 0)
                _horizontalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.AbsOpacity, _config.Colors.AbsColor));
            else
                _horizontalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.BrakeOpacity, _config.Colors.BrakeColor));

            if (SimDataProvider.LocalCar.Electronics.TractionControlActivation > 0)
                _horizontalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.TcOpacity, _config.Colors.TcColor));
            else
                _horizontalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.ThrottleOpacity, _config.Colors.ThrottleColor));
        }
        else
        {
            if (SimDataProvider.LocalCar.Electronics.AbsActivation > 0)
                _verticalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.AbsOpacity, _config.Colors.AbsColor));
            else
                _verticalBrakeBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.BrakeOpacity, _config.Colors.BrakeColor));

            if (SimDataProvider.LocalCar.Electronics.TractionControlActivation > 0)
                _verticalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.TcOpacity, _config.Colors.TcColor));
            else
                _verticalGasBar.FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.ThrottleOpacity, _config.Colors.ThrottleColor));
        }
    }
}
