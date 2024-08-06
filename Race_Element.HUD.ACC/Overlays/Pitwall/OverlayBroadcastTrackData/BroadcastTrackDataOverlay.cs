using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.Util;
using System.Drawing;
using System.Linq;
using System.Reflection;
using static RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.DebugInfoHelper;

namespace RaceElement.HUD.ACC.Overlays.OverlayDebugInfo.OverlayBroadcastRealtime;


[Overlay(Name = "Broadcast Track Data", Version = 1.00, OverlayType = OverlayType.Pitwall,
    Description = "A panel showing live broadcast track data.")]
internal sealed class BroadcastTrackDataOverlay : ACCOverlay
{
    private readonly DebugConfig _config = new();
    private InfoTable _table;

    public BroadcastTrackDataOverlay(Rectangle rectangle) : base(rectangle, "Broadcast Track Data")
    {
        this.AllowReposition = false;
        this.RefreshRateHz = 5;
        this.Width = 300;
        this.Height = 80;

        _table = new InfoTable(9, [200]);
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
        FieldInfo[] members = broadCastTrackData.GetType().GetRuntimeFields().ToArray();
        foreach (FieldInfo member in members)
        {
            var value = member.GetValue(broadCastTrackData);
            value = ReflectionUtil.FieldTypeValue(member, value);

            if (value != null)
                _table.AddRow($"{member.Name.Replace("<", "").Replace(">k__BackingField", "")}", [value.ToString()]);
        }

        _table.Draw(g);
    }

    public sealed override bool ShouldRender() => true;
}
