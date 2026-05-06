using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.ProgressBars;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RaceElement.HUD.Common.Overlays.Driving.InputBars;

[Overlay(Name = "Input Bars", Version = 1.00, OverlayType = OverlayType.Drive,
  Description = "Live input bars of throttle and brake.",
  OverlayCategory = OverlayCategory.Inputs,
  UnsupportedGames = Game.MicrosoftFlightSimulator2020,
  Authors = ["Reinier Klarenberg"]
 )]
internal sealed class InputBarsOverlay : CommonAbstractOverlay
{
    private readonly InputBarsConfiguration _config = new();

    private CachedBitmap _cachedBackground;

    private readonly HorizontalProgressBar[] _horizontalBars;
    private HorizontalProgressBar _horizontalGasBar;
    private HorizontalProgressBar _horizontalBrakeBar;
    private HorizontalProgressBar _horizontalClutchBar;

    private readonly VerticalProgressBar[] _verticalBars;
    private VerticalProgressBar _verticalGasBar;
    private VerticalProgressBar _verticalBrakeBar;
    private VerticalProgressBar _verticalClutchBar;

    private float _throttle = 0f;
    private float _brake = 0f;
    private float _clutch = 0f;

    public InputBarsOverlay(Rectangle rectangle) : base(rectangle, "Input Bars")
    {
        RefreshRateHz = _config.Bars.RefreshRate;


        int barCount = _config.Bars.ShowClutch switch
        {
            true => 3,
            false => 2,
        };
        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            _horizontalBars = new HorizontalProgressBar[barCount];
            Width = _config.Bars.Length + 1;
            Height = _config.Bars.Thickness * barCount + _config.Bars.Spacing * (barCount - 1) + 1;
        }
        else
        {
            _verticalBars = new VerticalProgressBar[barCount];
            Width = _config.Bars.Thickness * barCount + _config.Bars.Spacing * (barCount - 1) + 1;
            Height = _config.Bars.Length + 1;
        }
    }

    public override void SetupPreviewData()
    {
        _throttle = 0.823f;
        _brake = 0.117f;
        _clutch = 0.171f;
    }

    public sealed override void BeforeStart()
    {
        int barCount = _config.Bars.ShowClutch switch
        {
            true => 3,
            false => 2,
        };
        int width = _config.Bars.Thickness * barCount + _config.Bars.Spacing * (barCount - 1);
        int height = _config.Bars.Length;

        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            width = _config.Bars.Length;
            height = _config.Bars.Thickness * barCount + _config.Bars.Spacing * (barCount - 1);
        }

        _cachedBackground = new CachedBitmap((int)(width * Scale), (int)(height * Scale), g =>
        {
            if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
            {
                using LinearGradientBrush gradientBrush = new(new Rectangle(0, 0, (int)(_config.Bars.Length * Scale), (int)(_config.Bars.Thickness * Scale)), Color.FromArgb(120, Color.Black), Color.FromArgb(230, Color.Black), LinearGradientMode.Horizontal);

                g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.Bars.Length * Scale), (int)(_config.Bars.Thickness * Scale)), (int)(5 * Scale));
                g.FillRoundedRectangle(gradientBrush, new Rectangle(0, (int)((_config.Bars.Thickness + _config.Bars.Spacing) * Scale), (int)(_config.Bars.Length * Scale), (int)(_config.Bars.Thickness * Scale)), (int)(5 * Scale));

                if (_config.Bars.ShowClutch)
                    g.FillRoundedRectangle(gradientBrush, new Rectangle(0, (int)((_config.Bars.Thickness + _config.Bars.Spacing) * 2 * Scale), (int)(_config.Bars.Length * Scale), (int)(_config.Bars.Thickness * Scale)), (int)(5 * Scale));
            }
            else
            {
                using LinearGradientBrush gradientBrush = new(new Rectangle(0, 0, (int)(_config.Bars.Thickness * Scale), (int)(height * Scale)), Color.FromArgb(230, Color.Black), Color.FromArgb(120, Color.Black), LinearGradientMode.Vertical);

                g.FillRoundedRectangle(gradientBrush, new Rectangle(0, 0, (int)(_config.Bars.Thickness * Scale), (int)(height * Scale)), (int)(6 * Scale));
                g.FillRoundedRectangle(gradientBrush, new Rectangle((int)((_config.Bars.Thickness + _config.Bars.Spacing) * Scale), 0, (int)(_config.Bars.Thickness * Scale), (int)(height * Scale)), (int)(5 * Scale));

                if (_config.Bars.ShowClutch)
                    g.FillRoundedRectangle(gradientBrush, new Rectangle((int)((_config.Bars.Thickness + _config.Bars.Spacing) * 2 * Scale), 0, (int)(_config.Bars.Thickness * Scale), (int)(height * Scale)), (int)(5 * Scale));
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
            _horizontalClutchBar = new HorizontalProgressBar(_config.Bars.Length, _config.Bars.Thickness)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.ClutchOpacity, _config.Colors.ClutchColor)),
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = Scale,
                Rounding = 5,
            };

            if (_config.Bars.ThrottleFirst)
            {
                _horizontalBars[0] = _horizontalGasBar;
                _horizontalBars[1] = _horizontalBrakeBar;
                if (_config.Bars.ShowClutch)
                    _horizontalBars[2] = _horizontalClutchBar;
            }
            else
            {
                if (_config.Bars.ShowClutch)
                {
                    _horizontalBars[0] = _horizontalClutchBar;
                    _horizontalBars[1] = _horizontalBrakeBar;
                    _horizontalBars[2] = _horizontalGasBar;
                }
                else
                {
                    _horizontalBars[0] = _horizontalBrakeBar;
                    _horizontalBars[1] = _horizontalGasBar;
                }
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
            _verticalClutchBar = new VerticalProgressBar(_config.Bars.Thickness, _config.Bars.Length)
            {
                Value = 0,
                Min = 0,
                Max = 1,
                FillBrush = new SolidBrush(Color.FromArgb(_config.Colors.ClutchOpacity, _config.Colors.ClutchColor)),
                OutlineBrush = outlineBrush,
                Rounded = true,
                Scale = Scale,
                Rounding = 5,
            };
            if (_config.Bars.ThrottleFirst)
            {
                _verticalBars[0] = _verticalGasBar;
                _verticalBars[1] = _verticalBrakeBar;
                if (_config.Bars.ShowClutch)
                    _verticalBars[2] = _verticalClutchBar;
            }
            else
            {
                if (_config.Bars.ShowClutch)
                {
                    _verticalBars[0] = _verticalClutchBar;
                    _verticalBars[1] = _verticalBrakeBar;
                    _verticalBars[2] = _verticalGasBar;
                }
                else
                {
                    _verticalBars[0] = _verticalBrakeBar;
                    _verticalBars[1] = _verticalGasBar;
                }
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
            _clutch = SimDataProvider.LocalCar.Inputs.Clutch;
        }

        ApplyElectronicsColors();

        if (_config.Bars.Orientation == InputBarsConfiguration.BarOrientation.Horizontal)
        {
            int backgroundHeight = _config.Bars.Thickness * _horizontalBars.Length + _config.Bars.Spacing * (_horizontalBars.Length - 1);
            _cachedBackground?.Draw(g, _config.Bars.Length, backgroundHeight);

            _horizontalGasBar.Value = _throttle;
            _horizontalBrakeBar.Value = _brake;
            if (_config.Bars.ShowClutch)
                _horizontalClutchBar.Value = _clutch;

            _horizontalBars[0]?.Draw(g, 0, 0);
            _horizontalBars[1]?.Draw(g, 0, _config.Bars.Thickness + _config.Bars.Spacing);
            if (_config.Bars.ShowClutch)
                _horizontalBars[2]?.Draw(g, 0, _config.Bars.Thickness * 2 + _config.Bars.Spacing * 2);
        }
        else
        {
            int backgroundWidth = _config.Bars.Thickness * _verticalBars.Length + _config.Bars.Spacing * (_verticalBars.Length - 1);
            _cachedBackground?.Draw(g, backgroundWidth, _config.Bars.Length);

            _verticalBrakeBar.Value = _brake;
            _verticalGasBar.Value = _throttle;
            if (_config.Bars.ShowClutch)
                _verticalClutchBar.Value = _clutch;

            _verticalBars[0]?.Draw(g, 0, 0);
            _verticalBars[1]?.Draw(g, _config.Bars.Thickness + _config.Bars.Spacing, 0);
            if (_config.Bars.ShowClutch)
                _verticalBars[2]?.Draw(g, _config.Bars.Thickness * 2 + _config.Bars.Spacing * 2, 0);
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
