using Newtonsoft.Json;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;
using System.Drawing.Text;

namespace RaceElement.HUD.Common.Overlays.Pitwall.LocalCarData;

[Overlay(Name = "LocalCarData", Description = "Provides info about the common local car data.", OverlayType = OverlayType.Pitwall)]
internal sealed class LocalCarDataOverlay : CommonAbstractOverlay
{
    private readonly LocalCarDataConfig _config = new();
    private sealed class LocalCarDataConfig : OverlayConfiguration
    {
        public LocalCarDataConfig() => this.GenericConfiguration.AllowRescale = false;

        [ConfigGrouping("Visible Members", "Adjust the members visible in the debug menu")]
        public VisibleMemberGrouping VisibleMember { get; init; } = new();
        public sealed class VisibleMemberGrouping
        {
            public bool CarModel { get; init; } = true;
            public bool Physics { get; init; } = true;
            public bool Engine { get; init; } = true;
            public bool Inputs { get; init; } = true;
            public bool Tyres { get; init; } = true;
            public bool Brakes { get; init; } = true;
            public bool Electronics { get; init; } = true;
            public bool RaceData { get; init; } = true;
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
    public LocalCarDataOverlay(Rectangle rectangle) : base(rectangle, "LocalCarData")
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

        if (_config.VisibleMember.CarModel)
            currentY += DrawObject(SimDataProvider.LocalCar.CarModel, "Car Model", currentY, g).Height;

        if (_config.VisibleMember.Physics)
            currentY += DrawObject(SimDataProvider.LocalCar.Physics, "Physics", currentY, g).Height;

        if (_config.VisibleMember.Engine)
            currentY += DrawObject(SimDataProvider.LocalCar.Engine, "Engine", currentY, g).Height;

        if (_config.VisibleMember.Inputs)
            currentY += DrawObject(SimDataProvider.LocalCar.Inputs, "Inputs", currentY, g).Height;

        if (_config.VisibleMember.Tyres)
            currentY += DrawObject(SimDataProvider.LocalCar.Tyres, "Tyres", currentY, g).Height;

        if (_config.VisibleMember.Brakes)
            currentY += DrawObject(SimDataProvider.LocalCar.Brakes, "Brakes", currentY, g).Height;

        if (_config.VisibleMember.Electronics)
            currentY += DrawObject(SimDataProvider.LocalCar.Electronics, "Electronics", currentY, g).Height;

        if (_config.VisibleMember.RaceData)
            currentY += DrawObject(SimDataProvider.LocalCar.Race, "Race Data", currentY, g).Height;

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
