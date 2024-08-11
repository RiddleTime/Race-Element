using Gma.System.MouseKeyHook;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using static RaceElement.Data.ACC.EntryList.EntryListTracker;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayPositionsTimestamp;

[Overlay(Name = "Positions Timestamp",
Description = "Shows the car numbers, positions laps driven at a timestamp when the hotkey(control + s) was pressed during a race session.\nPress hotkey to reset and hide hud again.",
Authors = ["Reinier Klarenberg", "Kenneth Portis(idea)"],
OverlayType = OverlayType.Pitwall)]
internal sealed class PositionsTimestamp : AbstractOverlay
{
    private readonly PositionsTimestampConfiguration _config = new();
    private sealed class PositionsTimestampConfiguration : OverlayConfiguration
    {
        public PositionsTimestampConfiguration() => GenericConfiguration.AllowRescale = false;

        [ConfigGrouping("Text Settings", "Adjust the look and feel of the text")]
        public TextGrouping Text { get; init; } = new TextGrouping();
        public sealed class TextGrouping
        {
            [ToolTip("Sets the size of the font.")]
            [IntRange(10, 20, 1)]
            public int FontSize { get; init; } = 12;
        }
    }

    private IKeyboardMouseEvents m_GlobalHook;
    private DateTime TimeStamped;
    private List<KeyValuePair<int, CarData>> TimeStampedCars = new();

    private Font _font;

    public PositionsTimestamp(Rectangle rectangle) : base(rectangle, "Positions Timestamp")
    {
        RefreshRateHz = 2;
        TimeStamped = DateTime.MinValue;
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        _font = FontUtil.FontSegoeMono(_config.Text.FontSize);

        m_GlobalHook = Hook.GlobalEvents();
        m_GlobalHook.OnCombination(new Dictionary<Combination, Action> {
        {
            Combination.FromString("Control+S"), () => {
                if (broadCastRealTime.SessionType == Broadcast.RaceSessionType.Race)
                    if (TimeStamped == DateTime.MinValue){
                        TimeStampedCars = Instance.Cars.OrderBy(x => x.Value.RealtimeCarUpdate.Position).ToList();
                        TimeStamped = DateTime.UtcNow;
                    } else {
                        TimeStamped = DateTime.MinValue;
                        TimeStampedCars.Clear();
                    }
            }
        }});
    }

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;
        m_GlobalHook?.Dispose();
    }

    public sealed override void Render(Graphics g)
    {
        if (IsPreviewing) return;

        if (TimeStampedCars.Count == 0)
            return;

        float maxWidth = 0;
        List<string> strings = [];
        for (int i = 0; i < TimeStampedCars.Count; i++)
        {
            string row = $"{TimeStampedCars[i].Value.RealtimeCarUpdate.Position} - #{TimeStampedCars[i].Value.CarInfo.RaceNumber} | L{TimeStampedCars[i].Value.RealtimeCarUpdate.Laps}";
            maxWidth.ClipMin(g.MeasureString(row, _font).Width);
            strings.Add(row);
        }
        int totalHeight = (int)Math.Ceiling(strings.Count * _font.GetHeight());
        Width = (int)maxWidth;
        Height = totalHeight;

        g.FillRectangle(Brushes.Black, new Rectangle(0, 0, Width, Height));
        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        for (int i = 0; i < strings.Count; i++)
        {
            g.DrawStringWithShadow(strings[i], _font, Brushes.White, new PointF(0, i * _font.GetHeight()));
        }
    }
}
