using Newtonsoft.Json;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Pitwall.GameData;

[Overlay(
    Name = "Common GameData",
    Description = "Provides info about the common local car data.",
    OverlayType = OverlayType.Pitwall,
    Authors = ["Reinier Klarenberg"],
    Game = Data.Games.Game.Any
)]
internal sealed class GameDataOverlay : CommonAbstractOverlay
{
    private readonly LocalCarDataConfig _config = new();
    private sealed class LocalCarDataConfig : OverlayConfiguration
    {
        public LocalCarDataConfig() => this.GenericConfiguration.AllowRescale = false;

        //[ConfigGrouping("Visible Members", "Adjust the members visible in the debug menu")]
        public VisibleMemberGrouping VisibleMember { get; init; } = new();
        public sealed class VisibleMemberGrouping
        {
            // add members here to show if we got inner objects for the game data...
        }

        [ConfigGrouping("Data", "Adjust the members visible in the debug menu")]
        public DataGrouping Data { get; init; } = new();
        public sealed class DataGrouping
        {
            [IntRange(1, 30, 1)]
            public int RefreshRateHz { get; init; } = 1;
        }
    }

    private Font? _font;
    public GameDataOverlay(Rectangle rectangle) : base(rectangle, "Common GameData")
    {
        RefreshRateHz = _config.Data.RefreshRateHz;
        Width = 330;
    }

    public sealed override void BeforeStart() => _font = FontUtil.FontSegoeMono(10);
    public sealed override void BeforeStop() => _font?.Dispose();
    public sealed override bool ShouldRender() => true;

    public sealed override void Render(Graphics g)
    {
        using SolidBrush blackBrush = new(Color.FromArgb(196, 0, 0, 0));
        g.FillRectangle(blackBrush, new() { X = 0, Y = 0, Width = Width, Height = Height });

        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        float currentY = 0;

        currentY += DrawObject(SimDataProvider.GameData, "Car Model", currentY, g).Height;


        this.Height = (int)currentY;
    }

    private SizeF DrawObject(object obj, string header, float y, Graphics g)
    {
        if (_font == null) return new();

        string carModel = JsonConvert.SerializeObject(obj, Formatting.Indented);
        carModel = $"{header}:\n{carModel.Remove(0, 3)}";
        carModel = carModel.Remove(carModel.Length - 1, 1);
        carModel = carModel.Replace("  },\r\n", "");
        carModel = carModel.Replace(" {", "");
        SizeF carModelSize = g.MeasureString(carModel, _font, Width);
        g.DrawStringWithShadow(carModel, _font, Brushes.White, new RectangleF(0, y, carModelSize.Width, carModelSize.Height));

        return carModelSize;
    }
}
