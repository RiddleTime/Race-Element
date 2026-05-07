using Newtonsoft.Json;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Text;
using System.Text.RegularExpressions;

namespace RaceElement.HUD.Common.Overlays.Pitwall.LocalPlaneData;

[Overlay(
    Name = "Common LocalPlaneData",
    Description = "Provides info about the common local lane data.",
    OverlayType = OverlayType.Pitwall,
    Authors = ["Reinier Klarenberg"],
    SupportedGames = Data.Games.Game.MicrosoftFlightSimulator2020
)]
internal sealed partial class LocalPlaneDataOverlay : CommonAbstractOverlay
{
    private readonly LocalPlaneDataConfig _config = new();
    private sealed class LocalPlaneDataConfig : OverlayConfiguration
    {
        public LocalPlaneDataConfig() => this.GenericConfiguration.AllowRescale = false;

        [ConfigGrouping("Visible Members", "Adjust the members visible in the debug menu")]
        public VisibleMemberGrouping VisibleMember { get; init; } = new();
        public sealed class VisibleMemberGrouping
        {
            public bool General { get; init; } = true;
            public bool Physics { get; init; } = true;

            public bool Engines { get; init; } = true;
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
    public LocalPlaneDataOverlay(Rectangle rectangle) : base(rectangle, "Common LocalPlaneData")
    {
        RefreshRateHz = _config.Data.RefreshRateHz;
        Width = 380;
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

        if (_config.VisibleMember.General)
            currentY += DrawObject(SimDataProvider.LocalPlane.General, "General", currentY, g).Height;

        if (_config.VisibleMember.Physics)
            currentY += DrawObject(SimDataProvider.LocalPlane.Physics, "Physics", currentY, g).Height;

        if (_config.VisibleMember.Engines)
            currentY += DrawObject(SimDataProvider.LocalPlane.Engines, "Engines", currentY, g).Height;

        this.Height = (int)currentY;
    }

    private SizeF DrawObject(object obj, string header, float y, Graphics g)
    {
        if (_font == null) return new();

        string carModel = JsonConvert.SerializeObject(obj, Formatting.Indented);
        if (carModel.Length < 3) return new();
        carModel = $"{header}:\n{carModel.Remove(0, 3)}";
        carModel = carModel.Remove(carModel.Length - 1, 1);
        carModel = carModel.Replace("  },\r\n", "");
        carModel = carModel.Replace("  },\r\n", "");
        carModel = carModel.Replace(" {", "");
        carModel = carModel.Replace(":\n \r\n", ":\n");
        carModel = CommaBeforeNewLine().Replace(carModel, "");
        SizeF carModelSize = g.MeasureString(carModel, _font, Width);
        g.DrawStringWithShadow(carModel, _font, Brushes.White, new RectangleF(0, y, carModelSize.Width, carModelSize.Height));

        return carModelSize;
    }

    [GeneratedRegex(@",\s*$", RegexOptions.Multiline)]
    private static partial Regex CommaBeforeNewLine();
}
