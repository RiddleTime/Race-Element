using RaceElement.Data;
using RaceElement.Data.ACC.EntryList;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util.SystemExtensions;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static RaceElement.Data.ACC.EntryList.EntryListTracker;
using static RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime;

[Overlay(Name = "Broadcast Realtime", Version = 1.00, OverlayType = OverlayType.Pitwall,
    Description = "A panel showing live broadcast realtime data.")]
internal sealed class BroadcastRealtimeOverlay : AbstractOverlay
{
    private DebugConfig _config = new();

    private InfoTable _table;

    public BroadcastRealtimeOverlay(Rectangle rectangle) : base(rectangle, "Broadcast Realtime")
    {
        this.AllowReposition = false;
        this.RefreshRateHz = 5;
        this.Width = 370;
        this.Height = 280;

        _table = new InfoTable(9, [250]);
    }

    private void Instance_WidthChanged(object sender, bool e)
    {
        if (e)
            this.X = DebugInfoHelper.Instance.GetX(this);
    }

    public sealed override void BeforeStart()
    {
        if (this._config.Dock.Undock)
            this.AllowReposition = true;
        else
        {
            DebugInfoHelper.Instance.WidthChanged += Instance_WidthChanged;
            DebugInfoHelper.Instance.AddOverlay(this);
            this.X = DebugInfoHelper.Instance.GetX(this);
            this.Y = 0;
        }
    }

    public sealed override void BeforeStop()
    {
        if (!this._config.Dock.Undock)
        {
            DebugInfoHelper.Instance.RemoveOverlay(this);
            DebugInfoHelper.Instance.WidthChanged -= Instance_WidthChanged;
        }
    }

    public sealed override void Render(Graphics g)
    {
        lock (EntryListTracker.Instance.Cars)
        {
            foreach (KeyValuePair<int, CarData> carData in EntryListTracker.Instance.Cars)
            {

                if (carData.Key == broadCastRealTime.FocusedCarIndex)
                {
                    if (carData.Value.CarInfo == null)
                        continue;

                    string driverName = carData.Value.CarInfo.GetCurrentDriverName();

                    string firstName = carData.Value.CarInfo.Drivers[carData.Value.CarInfo.CurrentDriverIndex].FirstName;
                    _table.AddRow("P", [$"{carData.Value.RealtimeCarUpdate.Position}"]);
                    _table.AddRow($"", [$"#{carData.Value.CarInfo.RaceNumber.ToString().FillEnd(4, ' ')}- {firstName} {driverName}"]);
                    _table.AddRow("Team", [$"{carData.Value.CarInfo.TeamName}"]);

                    var name = ConversionFactory.GetCarName(carData.Value.CarInfo.CarModelType);
                    _table.AddRow("Car", [$"{name}"]);


                    string lapType = string.Empty;
                    if (carData.Value.RealtimeCarUpdate.CurrentLap != null)
                        lapType = $" - {carData.Value.RealtimeCarUpdate.CurrentLap.Type}";
                    _table.AddRow("Lap", [$"{carData.Value.RealtimeCarUpdate.Laps}{lapType}"]);

                    _table.AddRow("Speed", [$"{carData.Value.RealtimeCarUpdate.Kmh} km/h"]);

                    if (carData.Value.RealtimeCarUpdate.CurrentLap != null && carData.Value.RealtimeCarUpdate.CurrentLap.LaptimeMS.HasValue)
                    {
                        _table.AddRow("", [carData.Value.RealtimeCarUpdate.CurrentLap.LaptimeMS.HasValue ? $"{carData.Value.RealtimeCarUpdate.CurrentLap.LaptimeMS.Value / 1000}" : ""]);
                    }

                    _table.AddRow("BroadCst", [$"X: {carData.Value.RealtimeCarUpdate.WorldPosX:F2}, Y: {carData.Value.RealtimeCarUpdate.WorldPosY:F2}, H: {carData.Value.RealtimeCarUpdate.Heading:F2}"]);
                    int playerCarIndex = pageGraphics.PlayerCarID;
                    int playerIndex = 0;
                    for (int i = 0; i < pageGraphics.CarIds.Length; i++)
                        if (pageGraphics.CarIds[i] == playerCarIndex)
                        {
                            playerIndex = i;
                            break;
                        }

                    var coord = pageGraphics.CarCoordinates[playerIndex];
                    _table.AddRow("ShMem", [$"X: {coord.X:F2}, Z: {coord.Y:F2}, Y: {coord.Z:F2}"]);
                    _table.AddRow("%", [$"{carData.Value.RealtimeCarUpdate.SplinePosition:F3}"]);

                    //FieldInfo[] members = carData.Value.RealtimeCarUpdate.GetType().GetRuntimeFields().ToArray();
                    //foreach (FieldInfo member in members)
                    //{
                    //    var value = member.GetValue(carData.Value.RealtimeCarUpdate);
                    //    value = ReflectionUtil.FieldTypeValue(member, value);

                    //    if (value != null)
                    //        _table.AddRow($"{member.Name.Replace("<", "").Replace(">k__BackingField", "")}", new string[] { value.ToString() });
                    //}

                    _table.Draw(g);
                }
            }
        }
    }

    public sealed override bool ShouldRender() => true;
}
