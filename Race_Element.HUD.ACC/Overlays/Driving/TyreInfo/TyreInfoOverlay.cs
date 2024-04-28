using RaceElement.Data.ACC.Tyres;
using RaceElement.HUD.ACC.Overlays.Driving.TyreInfo;
using RaceElement.HUD.ACC.Overlays.OverlayPressureTrace;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.OverlayTyreInfo;

[Overlay(Name = "Tyre Info", Version = 1.00, OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Car,
    Description = "Shows tyre temperatures and more. Put it on top of vanilla in-game tyre hud.",
Authors = ["Reinier Klarenberg"])]
internal sealed class TyreInfoOverlay : AbstractOverlay
{
    private readonly TyreInfoConfiguration _config = new();

    private const int InitialWidth = 135;
    private const int InitialHeight = 222;
    private const double MaxPadLife = 29;

    private readonly Font _fontFamilyLarge;
    private readonly Font _fontFamily;
    private readonly Font _fontFamilySmall;
    private readonly int _yMono;
    private readonly int _yMonoSmall;

    private TyresTracker.TyresInfo _lastTyresInfo;


    public TyreInfoOverlay(Rectangle rectangle) : base(rectangle, "Tyre Info")
    {
        _fontFamilyLarge = FontUtil.FontSegoeMono(13);
        _fontFamily = FontUtil.FontSegoeMono(11);
        _yMono = _fontFamily.Height / 6;
        _fontFamilySmall = FontUtil.FontSegoeMono(10);
        _yMonoSmall = _fontFamilySmall.Height / 5;

        this.Width = InitialWidth;
        this.Height = InitialHeight;
        this.RefreshRateHz = _config.Information.RefreshRate;
    }



    public sealed override void BeforeStart()
    {
        TyresTracker.Instance.OnTyresInfoChanged += OnTyresInfoChanged;
    }

    public sealed override void BeforeStop()
    {
        TyresTracker.Instance.OnTyresInfoChanged -= OnTyresInfoChanged;
    }

    private void OnTyresInfoChanged(object sender, TyresTracker.TyresInfo e)
    {
        _lastTyresInfo = e;
    }

    public sealed override void SetupPreviewData()
    {
        _lastTyresInfo = TyresTracker.GetPreviewTyresInfo();
    }

    public sealed override void Render(Graphics g)
    {
        if (this.IsRepositioning)
        {
            using Pen repositionLinePen = new(Brushes.Red, 2 * this.Scale);
            g.DrawLine(repositionLinePen, new Point(InitialWidth / 2, 0), new Point(InitialWidth / 2, InitialHeight));
            g.DrawLine(repositionLinePen, new Point(0, InitialHeight / 2), new Point(InitialWidth, InitialHeight / 2));
        }

        if (_config.Information.Pressures)
            DrawTyrePressures(g);

        if (this._config.Information.PadLife)
        {
            DrawPadWearText(g, 66, 36, Position.Front);
            DrawPadWearText(g, 66, 170, Position.Rear);
        }

        if (this._config.Information.BrakeTemps)
        {
            DrawBrakeTemps(g, 66, 59, Position.Front);
            DrawBrakeTemps(g, 66, 147, Position.Rear);
        }

        if (this._config.Information.LossOfPressure)
        {
            DrawTyrePressureLoss(g, 22, 0, Wheel.FrontLeft);
            DrawTyrePressureLoss(g, 110, 0, Wheel.FrontRight);
            DrawTyrePressureLoss(g, 22, 206, Wheel.RearLeft);
            DrawTyrePressureLoss(g, 110, 206, Wheel.RearRight);
        }

        DrawTyreTemp(g, 27, 80, Wheel.FrontLeft);
        DrawTyreTemp(g, 106, 80, Wheel.FrontRight);
        DrawTyreTemp(g, 27, 122, Wheel.RearLeft);
        DrawTyreTemp(g, 106, 122, Wheel.RearRight);
    }

    private void DrawTyrePressures(Graphics g)
    {
        TyrePressureRange range = TyrePressures.GetCurrentRange(pageGraphics.TyreCompound, pageStatic.CarModel);

        int baseY = 16;
        if (range != null)
        {
            DrawTyrePressure(g, 0, baseY, Wheel.FrontLeft, range);
            DrawTyrePressure(g, 76, baseY, Wheel.FrontRight, range);
            DrawTyrePressure(g, 0, baseY + 168, Wheel.RearLeft, range);
            DrawTyrePressure(g, 76, baseY + 168, Wheel.RearRight, range);
        }
    }

    private void DrawTyrePressureLoss(Graphics g, int x, int y, Wheel wheel)
    {
        if (_lastTyresInfo == null)
            return;

        float pressureLoss = _lastTyresInfo.PressureLoss[(int)wheel];

        if (pressureLoss == 0)
            return;

        SmoothingMode previous = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        TextRenderingHint previousHint = g.TextRenderingHint;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.TextContrast = 1;

        string text = $"-{pressureLoss:F2}";
        int textWidth = (int)g.MeasureString(text, _fontFamilySmall).Width;
        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, textWidth, _fontFamilySmall.Height), 2);
        g.DrawStringWithShadow(text, _fontFamilySmall, Brushes.DarkOrange, new PointF(x - textWidth / 2, y + _yMonoSmall / 2));

        g.SmoothingMode = previous;
        g.TextRenderingHint = previousHint;
    }

    private void DrawTyreTemp(Graphics g, int x, int y, Wheel wheel)
    {
        float temp = pagePhysics.TyreCoreTemperature[(int)wheel];

        Brush tyreBrush = Brushes.LimeGreen;

        if (temp > 95)
            tyreBrush = Brushes.IndianRed;
        if (temp < 75)
            tyreBrush = Brushes.Cyan;

        if (pageGraphics.TyreCompound == "wet_compound")
        {
            tyreBrush = Brushes.LimeGreen;
            if (temp > 65)
                tyreBrush = Brushes.IndianRed;
            if (temp < 25)
                tyreBrush = Brushes.Cyan;
        }


        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.CompositingQuality = CompositingQuality.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.TextContrast = 1;

        string text = $"{temp:F1}°";
        int textWidth = (int)g.MeasureString(text, _fontFamily).Width;

        Rectangle backgroundDimension = new(x - textWidth / 2, y, textWidth, _fontFamily.Height);

        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(185, 0, 0, 0)), backgroundDimension, 2);

        g.DrawStringWithShadow(text, _fontFamily, tyreBrush, new PointF(x - textWidth / 2, y + _yMono - 1));
    }

    private void DrawBrakeTemps(Graphics g, int x, int y, Position position)
    {
        float averageBrakeTemps = 0;
        switch (position)
        {
            case Position.Front:
                {
                    float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.FrontLeft];
                    float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.FrontRight];
                    averageBrakeTemps = (brakeTempLeft + brakeTempRight) / 2;
                    break;
                }
            case Position.Rear:
                {
                    float brakeTempLeft = pagePhysics.BrakeTemperature[(int)Wheel.RearLeft];
                    float brakeTempRight = pagePhysics.BrakeTemperature[(int)Wheel.RearRight];
                    averageBrakeTemps = (brakeTempLeft + brakeTempRight) / 2;
                    break;
                }
        }

        string text = $"{averageBrakeTemps:F0}°";
        int textWidth = (int)g.MeasureString(text, _fontFamilySmall).Width;

        SmoothingMode previous = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        TextRenderingHint previousHint = g.TextRenderingHint;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.TextContrast = 1;

        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamilySmall.Height), 2);
        g.DrawStringWithShadow(text, _fontFamilySmall, Brushes.White, new PointF(x - textWidth / 2, y + _yMonoSmall));

        g.SmoothingMode = previous;
        g.TextRenderingHint = previousHint;
    }

    private void DrawPadWearText(Graphics g, int x, int y, Position position)
    {
        double percentage = 0;
        switch (position)
        {
            case Position.Front:
                {
                    float padLifeLeft = pagePhysics.PadLife[(int)Wheel.FrontLeft];
                    float padLifeRight = pagePhysics.PadLife[(int)Wheel.FrontRight];

                    float averagePadLife = (padLifeLeft + padLifeRight) / 2;

                    percentage = averagePadLife / MaxPadLife;
                    break;
                }
            case Position.Rear:
                {
                    float padLifeLeft = pagePhysics.PadLife[(int)Wheel.RearRight];
                    float padLifeRight = pagePhysics.PadLife[(int)Wheel.RearRight];

                    float averagePadLife = (padLifeLeft + padLifeRight) / 2;

                    percentage = averagePadLife / MaxPadLife;
                    break;
                }
        }

        string text = $"{percentage * 100:F0}%";
        int textWidth = (int)g.MeasureString(text, _fontFamilySmall).Width;

        SmoothingMode previous = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        TextRenderingHint previousHint = g.TextRenderingHint;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.TextContrast = 1;

        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(120, 0, 0, 0)), new Rectangle(x - textWidth / 2, y, (int)textWidth, _fontFamilySmall.Height), 2);

        g.DrawStringWithShadow(text, _fontFamilySmall, Brushes.White, new PointF(x - textWidth / 2, y + _yMonoSmall / 2));

        g.SmoothingMode = previous;
        g.TextRenderingHint = previousHint;
    }

    private void DrawTyrePressure(Graphics g, int x, int y, Wheel wheel, TyrePressureRange range)
    {
        int alpha = 255;

        Color brushColor = Color.FromArgb(alpha, 0, 255, 0);

        if (pagePhysics.WheelPressure[(int)wheel] >= range.OptimalMaximum)
            brushColor = Color.FromArgb(alpha, 255, 0, 0);

        if (pagePhysics.WheelPressure[(int)wheel] <= range.OptimalMinimum)
            brushColor = Color.FromArgb(alpha, 0, 0, 255);

        SmoothingMode previous = g.SmoothingMode;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        int width = 58;
        g.FillRoundedRectangle(new SolidBrush(brushColor), new Rectangle(x, y, width, 20), 3);


        CompositingQuality previousComposingQuality = g.CompositingQuality;
        TextRenderingHint previousTextRenderingHint = g.TextRenderingHint;
        int previousTextContrast = g.TextContrast;

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.TextContrast = 1;
        DrawTextWithOutline(g, Color.White, pagePhysics.WheelPressure[(int)wheel].ToString($"F{_config.Information.Decimals}"), x + width / 2, y + 1);

        g.SmoothingMode = previous;
        g.CompositingQuality = previousComposingQuality;
        g.TextContrast = previousTextContrast;
        g.TextRenderingHint = previousTextRenderingHint;
    }

    private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y)
    {
        int textWidth = (int)g.MeasureString(text, _fontFamilyLarge).Width;
        Rectangle backgroundDimension = new(x - textWidth / 2, y, (int)textWidth, _fontFamilyLarge.Height);
        g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(210, 0, 0, 0)), backgroundDimension, 2);
        g.DrawRoundedRectangle(new Pen(Color.FromArgb(135, 0, 0, 0), 0.6f * this.Scale), backgroundDimension, 2);
        g.DrawStringWithShadow(text, _fontFamilyLarge, textColor, new PointF(x - textWidth / 2, y + _fontFamilyLarge.GetHeight(g) / 11f), 1.3f * this.Scale);
    }
}
