// TODO: refactor to allow for non-ACC sims to not use RaceElement.Data.ACC
using RaceElement.Data.ACC.EntryList;
using RaceElement.Data.ACC.Session;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using Unglide;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;

namespace RaceElement.HUD.Common.Overlays.OverlayCurrentGear;

[Overlay(Name = "Current Gear", Version = 1.00, OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.Driving,
Description = "Shows the selected gear.",
Authors = ["Reinier Klarenberg"])]
internal sealed class CurrentGearOverlay : CommonAbstractOverlay
{
    private readonly CurrentGearConfiguration _config = new();
    private sealed class CurrentGearConfiguration : OverlayConfiguration
    {
        [ConfigGrouping("Gear", "Options for the Current Gear HUD")]
        public GearGrouping Gear { get; init; } = new GearGrouping();
        public class GearGrouping
        {
            [ToolTip("Show the current gear HUD when spectating")]
            public bool Spectator { get; init; } = true;
        }

        [ConfigGrouping("Colors", "Adjust colors")]
        public ColorsGrouping Colors { get; init; } = new ColorsGrouping();
        public class ColorsGrouping
        {
            public Color TextColor { get; init; } = Color.FromArgb(255, 255, 255, 255);
            [IntRange(75, 255, 1)]
            public int TextOpacity { get; init; } = 255;
        }

        public CurrentGearConfiguration() { GenericConfiguration.AllowRescale = true; }
    }

    private const int InitialWidth = 80;
    private const int InitialHeight = 72;
    private readonly List<CachedBitmap> _gearBitmaps = [];

    private int _lastGear = -2;
    private const float MaxOpacity = 1f;
    private float _opacity = MaxOpacity;
    private Tweener _gearTweener;
    private DateTime _gearTweenerStart = DateTime.Now;


    public CurrentGearOverlay(Rectangle rectangle) : base(rectangle, "Current Gear")
    {
        Width = InitialWidth;
        Height = InitialHeight;
        RefreshRateHz = 30;
    }

    public sealed override void SetupPreviewData()
    {
        _lastGear = 3;        
        SimDataProvider.LocalCar.Inputs.Gear = 3;
    }

    public sealed override void BeforeStart()
    {
        Font font = FontUtil.FontConthrax(50 * this.Scale);
        HatchBrush hatchBrush = new(HatchStyle.LightUpwardDiagonal, Color.FromArgb(225, Color.Black), Color.FromArgb(185, Color.Black));

        Rectangle renderRectangle = new(0, 0, (int)(InitialWidth * this.Scale), (int)(InitialHeight * this.Scale));
        for (int i = 0; i <= 8; i++)
        {
            string gear = i switch
            {
                0 => "R",
                1 => "N",
                _ => $"{i - 1}",
            };

            _gearBitmaps.Add(new CachedBitmap((int)(InitialWidth * this.Scale) + 1, (int)(InitialHeight * this.Scale) + 1, g =>
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.TextContrast = 1;

                g.FillRoundedRectangle(hatchBrush, renderRectangle, (int)(6 * this.Scale));

                int textWidth = (int)g.MeasureString(gear, font).Width;
                g.DrawStringWithShadow(gear, font, Color.FromArgb(_config.Colors.TextOpacity, _config.Colors.TextColor), new Point(renderRectangle.Width / 2 - textWidth / 2, (int)(renderRectangle.Height / 2 - font.Height / 2.18)), 1.5f * this.Scale);
            }));
        }

        font.Dispose();
        hatchBrush.Dispose();

        _gearTweener = new Tweener();
    }

    public sealed override void BeforeStop()
    {
        foreach (CachedBitmap cachedBitmap in _gearBitmaps)
            cachedBitmap?.Dispose();
    }

    public sealed override bool ShouldRender()
    {
        /* TODO: refactor to allow for non-ACC sims
        if (_config.Gear.Spectator && RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, broadCastRealTime.FocusedCarIndex))
            return true; */

        return base.ShouldRender();
    }

    private int GetCurrentGear()
    {
        int currentGear = SimDataProvider.LocalCar.Inputs.Gear;

        if (_config.Gear.Spectator)
        {
            int focusedIndex = SessionData.Instance.FocusedCarIndex;
    
            // TODO: refactor to allow for non-ACC sims
            // if (RaceSessionState.IsSpectating(pageGraphics.PlayerCarID, focusedIndex))
                lock (EntryListTracker.Instance.Cars)
                    if (EntryListTracker.Instance.Cars.Any())
                    {
                        var car = EntryListTracker.Instance.Cars.First(car => car.Key == focusedIndex);
                        currentGear = car.Value.RealtimeCarUpdate.Gear + 2;
                    }
        }

        return currentGear;
    }

    public sealed override void Render(Graphics g)
    {
        int currentGear = GetCurrentGear();

        if (_lastGear != currentGear)
        {
            _opacity = 0.8f;
            _gearTweenerStart = DateTime.Now;
            _gearTweener.Tween(this, new { _opacity = MaxOpacity }, 0.8f);
            _lastGear = currentGear;
        }

        if (_opacity < MaxOpacity)
            _gearTweener.Update(secondsElapsed: (float)DateTime.Now.Subtract(_gearTweenerStart).TotalSeconds);

        CachedBitmap bitmap = _gearBitmaps[currentGear];
        bitmap.Opacity = _opacity;
        bitmap?.Draw(g, InitialWidth, InitialHeight);
    }
}
