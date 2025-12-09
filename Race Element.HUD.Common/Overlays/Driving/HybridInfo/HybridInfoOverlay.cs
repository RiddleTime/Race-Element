using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;

namespace RaceElement.HUD.Common.Overlays.Driving.HybridInfo;

[Overlay(
    Name = "Hybrid Info",
    Description = "A overlay that shows the current hybrid energy status",
    Authors = ["Connor Molz"],
    Game = Game.iRacing
)]

public sealed class HybridInfoOverlay: CommonAbstractOverlay
{
    private readonly HybridInfoConfiguration _config = new();
    
    private readonly Font _font;

    private CachedBitmap _cachedBackground;
    
    // Stages of energy amount in the battery highest threshold = 1 lowest threshold = 3
    private CachedBitmap _cachedEnergyStage1;
    private CachedBitmap _cachedEnergyStage2;
    private CachedBitmap _cachedEnergyStage3;
    
    private float _energyStringWidth = -1;


    public HybridInfoOverlay(Rectangle rectangle) : base(rectangle, "Hybrid Info")
    {
        _font = FontUtil.FontSegoeMono(_config.Bar.FontSize);
    }
    
    public sealed override void BeforeStart()
    {
            int cornerRadius = (int)(_config.Bar.Roundness * Scale);

            _cachedBackground = new CachedBitmap((int)(_config.Bar.Width * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
            {
                Color bgColor = Color.FromArgb(185, 0, 0, 0);
                HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, bgColor, Color.FromArgb(bgColor.A - 50, bgColor));
                g.FillRoundedRectangle(hatchBrush, new Rectangle(0, 0, (int)(_config.Bar.Width * Scale), (int)(_config.Bar.Height * Scale)), cornerRadius);
                g.DrawRoundedRectangle(new Pen(Color.Black, 1 * Scale), new Rectangle(0, 0, (int)(_config.Bar.Width * Scale), (int)(_config.Bar.Height * Scale)), cornerRadius);
            });

            _cachedEnergyStage1 = new CachedBitmap((int)(_config.Bar.Width / 2 * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
            {
                Rectangle rect = new(0, 0, (int)(_config.Bar.Width / 2 * Scale), (int)(_config.Bar.Height * Scale));
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, cornerRadius, 0, 0, cornerRadius);
                using SolidBrush brush = new(Color.FromArgb(_config.Colors.ThresholdHighOpacity,
                    _config.Colors.ThresholdHighColor));
                g.FillPath(brush, path);
            });

            _cachedEnergyStage2 = new CachedBitmap((int)(_config.Bar.Width / 2 * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
            {
                Rectangle rect = new(0, 0, (int)(_config.Bar.Width / 2 * Scale), (int)(_config.Bar.Height * Scale));
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, 0, cornerRadius, cornerRadius, 0);
                using SolidBrush brush = new(Color.FromArgb(_config.Colors.ThresholdMediumOpacity,
                    _config.Colors.ThresholdMediumColor));
                g.FillPath(brush, path);
            });
            
            _cachedEnergyStage3 = new CachedBitmap((int)(_config.Bar.Width / 2 * Scale + 1), (int)(_config.Bar.Height * Scale + 1), g =>
            {
                Rectangle rect = new(0, 0, (int)(_config.Bar.Width / 2 * Scale), (int)(_config.Bar.Height * Scale));
                using GraphicsPath path = GraphicsExtensions.CreateRoundedRectangle(rect, 0, cornerRadius, cornerRadius, 0);
                using SolidBrush brush = new(Color.FromArgb(_config.Colors.ThresholdLowOpacity,
                    _config.Colors.ThresholdLowColor));
                g.FillPath(brush, path);
            });
    }
    
    public sealed override void BeforeStop()
    {
        Width = _config.Bar.Width + 1;
        Height = _config.Bar.Height + 1;
        
        Height += _font.Height * 1;

        RefreshRateHz = 30;
        _config.GenericConfiguration.AllowRescale = true;
        
        _cachedBackground?.Dispose();
        _cachedEnergyStage1?.Dispose();
        _cachedEnergyStage2?.Dispose();
        _cachedEnergyStage3?.Dispose();

        _font?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        _cachedBackground?.Draw(g, 0, 0, _config.Bar.Width, _config.Bar.Height);
        float energyLevel = SimDataProvider.LocalCar.Electronics.PushToPassLevel;
        
        DrawEnergyBar(g, energyLevel);
        DrawEnergyText(g, energyLevel);
    }
    
    
    private void DrawEnergyBar(Graphics g, float energyLevel)
    {
        int width = _config.Bar.Width;
        float fillPercent = energyLevel / 100f;
        float drawWidth = _config.Bar.Width * fillPercent;

        if (energyLevel >= _config.Colors.ThresholdHighLevel)
        {
            
            drawWidth.ClipMin(1);

            g.SetClip(new Rectangle(0, 0, (int)drawWidth, _config.Bar.Height));
            _cachedEnergyStage1?.Draw(g, 0, 0, width, _config.Bar.Height);
            g.ResetClip();
        }
        else if (energyLevel > _config.Colors.ThresholdMediumLevel)
        {
            
            drawWidth.ClipMin(1);

            g.SetClip(new Rectangle(0, 0, (int)drawWidth, _config.Bar.Height));
            _cachedEnergyStage2?.Draw(g, 0, 0, width, _config.Bar.Height);
            g.ResetClip();
        }
        else {
            
            drawWidth.ClipMin(1);

            g.SetClip(new Rectangle(0, 0, (int)drawWidth, _config.Bar.Height));
            _cachedEnergyStage3?.Draw(g, 0, 0, width, _config.Bar.Height);
            g.ResetClip();
        }
        
    }

    private void DrawEnergyText(Graphics g, float energyLevel)
    {
        string currentEnergyLevel = $"{energyLevel.ToString($"F{_config.Bar.Decimals}")}";

        currentEnergyLevel.FillStart(_config.Bar.Decimals + 3, ' '); // (+3) = ('-' or '+') plus "0."
        
        if (_energyStringWidth < 0)
            _energyStringWidth = g.MeasureString(currentEnergyLevel, _font).Width;

        if (_energyStringWidth < 0)
            _energyStringWidth = g.MeasureString(currentEnergyLevel, _font).Width;

        int x = _config.Bar.Width / 2;
        int y = _config.Bar.Height + 2;
        

        DrawTextWithOutline(g, Color.White, currentEnergyLevel, x, y);
    }

    private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y)
    {
        Rectangle backgroundDimension = new((int)(x - _energyStringWidth / 2), y, (int)_energyStringWidth, (int)(_font.Height * 0.9));

        g.SmoothingMode = SmoothingMode.AntiAlias;
        using SolidBrush backgroundBrush = new(Color.FromArgb(185, 0, 0, 0));
        g.FillRoundedRectangle(backgroundBrush, backgroundDimension, (int)(2 * Scale));

        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.TextContrast = 1;

        g.DrawStringWithShadow(text, _font, textColor, new PointF(x - _energyStringWidth / 2, y), 1.3f);
    }
}