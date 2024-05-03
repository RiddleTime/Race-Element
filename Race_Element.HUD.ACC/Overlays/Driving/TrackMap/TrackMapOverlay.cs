using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using RaceElement.Util;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

[Overlay(Name = "Track Map",
    Description = "Shows a track map",
    OverlayCategory = OverlayCategory.Track,
    OverlayType = OverlayType.Drive)]
internal sealed class TrackMapOverlay : AbstractOverlay
{
    private  readonly TrackMapConfiguration _config = new();
    private TrackMapCreationJob _minimapCreationJob;
    private Bitmap _trackMinimap;

    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        RefreshRateHz = 1;
    }

    public override void BeforeStart()
    {
        if (IsPreviewing)
        {
            return;
        }

        if (_trackMinimap == null)
        {
            _minimapCreationJob = new TrackMapCreationJob()
            {
                IntervalMillis = 4,
                Scale = _config.Map.Scale,
                MapColor = _config.Map.MapColor,
                Thickness = _config.Map.Thickness,
                Rotation = _config.Map.Rotation
            };

            _minimapCreationJob.OnMapCreation += OnMiniMapCreated;
            _minimapCreationJob.Run();
        }
    }

    public override void BeforeStop()
    {
        if (IsPreviewing)
        {
            return;
        }

        _minimapCreationJob?.Cancel();
        _trackMinimap?.Dispose();

        _minimapCreationJob = null;
        _trackMinimap = null;
    }

    public override bool ShouldRender()
    {
        return _trackMinimap != null;
    }

    public override void Render(Graphics g)
    {
        g.DrawImage(_trackMinimap, new Point());
    }

    private void OnMiniMapCreated(object sender, Bitmap minimap)
    {
        _minimapCreationJob.Cancel();
        _trackMinimap = minimap;

        if (_config.Map.SavePreview && ACCSharedMemory.Instance.PageFileStatic.Track.Length != 0)
        {
            var trackName = ACCSharedMemory.Instance.PageFileStatic.Track.Trim() + ".jpg";
            string path = FileUtil.RaceElementTracks + trackName;
            _trackMinimap.Save(path);
        }
    }
}
