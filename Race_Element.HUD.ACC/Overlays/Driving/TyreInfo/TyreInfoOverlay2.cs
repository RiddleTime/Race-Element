using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Driving.TyreInfo;

[Overlay(Name = "Tyre Info Overlay",
Description = "Overlays the vanilla tyre widget.",
Authors = ["Reinier Klarenberg"])]
internal sealed class TyreInfoOverlay2 : AbstractOverlay
{
    private readonly TyreInfoConfiguration _config = new();


    private GraphicsGrid _graphicsGrid;
    private readonly DrawableTextCell[] _cellsCoreTemperature = new DrawableTextCell[4];
    private DrawableTextCell[] _pressureCells = new DrawableTextCell[4];
    private DrawableTextCell[] _pressureLossCells = new DrawableTextCell[4];
    private DrawableTextCell[] _padLifeCells = new DrawableTextCell[4];
    private DrawableTextCell[] _brakeTemperatureCells = new DrawableTextCell[4];

    public TyreInfoOverlay2(Rectangle rectangle) : base(rectangle, "Tyre Info Overlay")
    {
    }

    public override void SetupPreviewData()
    {
        pagePhysics.TyreCoreTemperature = [25, 80, 80, 80];
    }

    public override void BeforeStart()
    {
        int gridRows = 1;  // Core Tyre Temps are always visible
        if (_config.Information.PadLife) gridRows++;
        if (_config.Information.BrakeTemps) gridRows++;
        if (_config.Information.LossOfPressure) gridRows++;
        if (_config.Information.Pressures) gridRows++;
        _graphicsGrid = new(gridRows, 4);


        PointF TyresLeftTopOrigin = new(0, 0);

        Font fontCoreTemps = FontUtil.FontSegoeMono(11f * Scale);
        float scaledTotalTyresWidth = VanillaWidgetDimensions.Tyres.TotalWidth * Scale;
        float scaledTotalTyresHeight = VanillaWidgetDimensions.Tyres.TotalHeight * Scale;
        float scaledTyreWidth = VanillaWidgetDimensions.Tyres.TyreWidth * Scale;
        float scaledTyreHeight = VanillaWidgetDimensions.Tyres.TyreHeight * Scale;
        RectangleF[] rectsTyres = [
            VanillaWidgetDimensions.Tyres.GetTyre(TyresLeftTopOrigin, Scale),
            VanillaWidgetDimensions.Tyres.GetTyre(new(scaledTotalTyresWidth - scaledTyreWidth, TyresLeftTopOrigin.Y), Scale),
            VanillaWidgetDimensions.Tyres.GetTyre(new(TyresLeftTopOrigin.X, scaledTotalTyresHeight - scaledTyreHeight), Scale),
            VanillaWidgetDimensions.Tyres.GetTyre(new(scaledTotalTyresWidth - scaledTyreWidth, scaledTotalTyresHeight - scaledTyreHeight), Scale),
        ];

        int rowIndex = 0;

        // core tyre temps
        for (int i = 0; i < 4; i++)
        {
            _cellsCoreTemperature[i] = new(rectsTyres[i], fontCoreTemps);
            _graphicsGrid.Grid[rowIndex][i] = _cellsCoreTemperature[i];
            _cellsCoreTemperature[i].StringFormat.LineAlignment = (i >= 2) ? StringAlignment.Near : StringAlignment.Far;
            int index = i;
            _cellsCoreTemperature[i].CachedBackground = new CachedBitmap((int)rectsTyres[index].Width + 1, (int)rectsTyres[index].Height + 1, g =>
            {
                float fontHeight = g.MeasureString("99.0", fontCoreTemps).Height;
                float y = (index >= 2) ? 0 : (float)Math.Ceiling(rectsTyres[index].Height - fontHeight);
                y -= (index >= 2) ? 1 : -1;
                int halfWidth = (int)rectsTyres[index].Width / 2;
                PointF from = (index < 2) ? new PointF(halfWidth, y - 1) : new PointF(halfWidth, y + fontHeight);
                PointF to = (index < 2) ? new PointF(halfWidth, y + fontHeight) : new PointF(halfWidth, y);


                Color colorBase;
                float temp = pagePhysics.TyreCoreTemperature[index];
                if (temp > 95)
                    colorBase = Color.IndianRed;
                else if (temp < 75)
                    colorBase = Color.Cyan;
                else
                    colorBase = Color.LimeGreen;
                if (pageGraphics.TyreCompound == "wet_compound")
                {
                    colorBase = Color.LimeGreen;
                    if (temp > 65)
                        colorBase = Color.IndianRed;
                    if (temp < 25)
                        colorBase = Color.Cyan;
                }
                Color colorTo = Color.FromArgb(225, Color.Black);
                Color colorFrom = Color.FromArgb(130, Color.Black);
                using LinearGradientBrush pthGrBrush = new(from, to, colorFrom, colorTo);

                using Pen pen = new(pthGrBrush, 1 * Scale);
                g.DrawRoundedRectangle(pen, new Rectangle(0, (int)y, (int)rectsTyres[index].Width, (int)fontHeight), (int)(6 * Scale));

                g.FillRoundedRectangle(pthGrBrush, new Rectangle(0, (int)y, (int)rectsTyres[index].Width, (int)fontHeight), (int)(6 * Scale));

            });

        }
        UpdateTyreTemps();

        Width = (int)scaledTotalTyresWidth;
        Height = (int)scaledTotalTyresHeight;
    }

    public override void Render(Graphics g)
    {
        UpdateTyreTemps();

        g.CompositingQuality = CompositingQuality.HighQuality;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        _graphicsGrid?.Draw(g);
    }

    private void UpdateTyreTemps()
    {
        for (int i = 0; i < 4; i++)
        {
            float temp = pagePhysics.TyreCoreTemperature[i];

            Color colorBase;
            if (temp > 95)
                colorBase = Color.IndianRed;
            else if (temp < 75)
                colorBase = Color.Cyan;
            else
                colorBase = Color.LimeGreen;
            if (pageGraphics.TyreCompound == "wet_compound")
            {
                colorBase = Color.LimeGreen;
                if (temp > 65)
                    colorBase = Color.IndianRed;
                if (temp < 25)
                    colorBase = Color.Cyan;
            }

            _cellsCoreTemperature[i].TextBrush = new SolidBrush(colorBase);
            if (_cellsCoreTemperature[i].UpdateText($"{temp.ToString(temp >= 100 ? "F0" : "F1")}"))
                _cellsCoreTemperature[i]?.CachedBackground?.Render();
        }
    }
}

internal static class VanillaWidgetDimensions
{

    /// <summary>
    /// The centered part of the HUD overlaying the region of the 4 tyre pressure bars
    /// </summary>
    public static class Tyres
    {
        public const int TotalWidth = 130;
        public const int TotalHeight = 140;

        public const int TyreWidth = 50;
        public const int TyreHeight = 56;

        public static RectangleF GetTyre(PointF tyreOrigin, float scaling) => new(tyreOrigin.X, tyreOrigin.Y, TyreWidth * scaling, TyreHeight * scaling);

        public const int TyrePressureBarWidth = 15;
        public const int TyrePressureBarGap = 2;


        public const int BrakePressureBarWidth = 10;
        public const int BrakePressureBarHeight = 36;
        public const int BrakePressureBarTopBottomMargin = 10;
    }
}
