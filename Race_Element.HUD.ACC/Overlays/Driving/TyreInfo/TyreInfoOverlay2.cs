using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
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
    private DrawableTextCell[] _pressureCells;
    private DrawableTextCell[] _pressureLossCells;
    private DrawableTextCell[] _cellsCoreTemperature;
    private DrawableTextCell[] _padLifeCells;
    private DrawableTextCell[] _brakeTemperatureCells;

    public TyreInfoOverlay2(Rectangle rectangle) : base(rectangle, "Tyre Info Overlay")
    {
    }

    public override void BeforeStart()
    {
        int gridRows = 1;  // Core Tyre Temps are always visible
        if (_config.Information.PadLife) gridRows++;
        if (_config.Information.BrakeTemps) gridRows++;
        if (_config.Information.LossOfPressure) gridRows++;
        if (_config.Information.Pressures) gridRows++;
        _graphicsGrid = new(gridRows, 4);


        RectangleF[] rectsCoreTemp = []; // TODO
        Font fontCoreTemps = null; // TODO
        // core tyre temps
        for (int i = 0; i < 4; i++)
            _cellsCoreTemperature[i] = new DrawableTextCell(rectsCoreTemp[i], fontCoreTemps);

    }

    public override void Render(Graphics g)
    {

        _graphicsGrid.Draw(g);
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

        public const int TyreWidth = 64;
        public const int TyreHeight = 56;

        public const int TyrePressureBarWidth = 15;
        public const int TyrePressureBarGap = 2;

        public const int BrakePressureBarWidth = 10;
        public const int BrakePressureBarHeight = 36;
        public const int BrakePressureBarTopBottomMargin = 10;
    }
}
