using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.TyreInfo;

[Overlay(Name = "Tyre Info Overlay",
Description = "Overlays the vanilla tyre widget.",
Authors = ["Reinier Klarenberg"])]
internal sealed class TyreInfoOverlay2 : AbstractOverlay
{
    private readonly TyreInfoConfiguration _config = new();


    private GraphicsGrid _graphicsGrid;
    private DrawableTextCell[] _cellsCoreTemperature = new DrawableTextCell[4];
    private DrawableTextCell[] _pressureCells = new DrawableTextCell[4];
    private DrawableTextCell[] _pressureLossCells = new DrawableTextCell[4];
    private DrawableTextCell[] _padLifeCells = new DrawableTextCell[4];
    private DrawableTextCell[] _brakeTemperatureCells = new DrawableTextCell[4];

    public TyreInfoOverlay2(Rectangle rectangle) : base(rectangle, "Tyre Info Overlay")
    {
    }

    public override void SetupPreviewData()
    {
        pagePhysics.TyreCoreTemperature = [70, 70, 70, 70];
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

        Font fontCoreTemps = FontUtil.FontSegoeMono(10 * Scale);
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
            _cellsCoreTemperature[i].CachedBackground = new CachedBitmap((int)rectsTyres[i].Width, (int)rectsTyres[i].Height, g =>
            {
                float fontHeight = g.MeasureString("100.0", fontCoreTemps).Height;
                float y = (i >= 2) ? 0 : (float)Math.Ceiling(rectsTyres[i].Height - fontHeight);
                y -= (i >= 2) ? 1 : -1;
                int halfWidth = (int)rectsTyres[i].Width / 2;
                PointF from = (i < 2) ? new PointF(halfWidth, y - 1) : new PointF(halfWidth, y + fontHeight);
                PointF to = (i < 2) ? new PointF(halfWidth, y + fontHeight) : new PointF(halfWidth, y);
                using LinearGradientBrush pthGrBrush = new(from, to, Color.FromArgb(5, 255, 255, 255), Color.FromArgb(170, 0, 0, 0));
                g.FillRoundedRectangle(pthGrBrush, new Rectangle(0, (int)y, (int)rectsTyres[i].Width, (int)fontHeight), (int)(6 * Scale));
            });

        }
        UpdateTyreTemps();

        Width = (int)scaledTotalTyresWidth;
        Height = (int)scaledTotalTyresHeight;
    }

    public override void Render(Graphics g)
    {
        UpdateTyreTemps();

        _graphicsGrid?.Draw(g);
    }

    private void UpdateTyreTemps()
    {
        for (int i = 0; i < 4; i++)
            _cellsCoreTemperature[i].UpdateText($"{pagePhysics.TyreCoreTemperature[i]:F1}");
    }
}

internal static class VanillaWidgetDimensions
{

    /// <summary>
    /// The centered part of the HUD overlaying the region of the 4 tyre pressure bars
    /// </summary>
    public static class Tyres
    {
        public const int TotalWidth = 128;
        public const int TotalHeight = 140;

        public const int TyreWidth = 49;
        public const int TyreHeight = 56;

        public static RectangleF GetTyre(PointF tyreOrigin, float scaling) => new(tyreOrigin.X, tyreOrigin.Y, TyreWidth * scaling, TyreHeight * scaling);

        public const int TyrePressureBarWidth = 15;
        public const int TyrePressureBarGap = 2;


        public const int BrakePressureBarWidth = 10;
        public const int BrakePressureBarHeight = 36;
        public const int BrakePressureBarTopBottomMargin = 10;
    }
}
