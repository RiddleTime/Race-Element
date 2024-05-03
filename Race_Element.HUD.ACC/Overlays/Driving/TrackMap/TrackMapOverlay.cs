using RaceElement.HUD.Overlay.Internal;
using RaceElement.Util;

using System.Collections.Generic;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.TrackMap;

public struct BoundingBox
{
    public float minX, maxX;
    public float minY, maxY;
}

public struct MapInfo
{
    public Bitmap bitmap;
    public List<PointF> positions;

    public BoundingBox originalBoundingBox;
    public BoundingBox transformedBoundingBox;
}

#if DEBUG
[Overlay(Name = "Track Map",
    Description = "Shows a track map",
    OverlayCategory = OverlayCategory.Track,
    OverlayType = OverlayType.Drive)]
#endif
internal sealed class TrackMapOverlay : AbstractOverlay
{
    private  readonly TrackMapConfiguration _config = new();
    private TrackMapCreationJob _minimapCreationJob;
    private MapInfo _trackMinimap;

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

        if (_trackMinimap.bitmap == null)
        {
            _minimapCreationJob = new TrackMapCreationJob()
            {
                IntervalMillis = 4,
                Scale = _config.Map.Scale,
                Color = _config.Map.Color,
                Rotation = _config.Map.Rotation,
                Thickness = _config.Map.Thickness
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
        _trackMinimap.bitmap?.Dispose();

        _trackMinimap.bitmap = null;
        _minimapCreationJob = null;
    }

    public override bool ShouldRender()
    {
        return _trackMinimap.bitmap != null;
    }

    public override void Render(Graphics g)
    {
        g.DrawImage(_trackMinimap.bitmap, new Point());
    }

    private void OnMiniMapCreated(object sender, MapInfo mapInfo)
    {
        _minimapCreationJob.Cancel();
        _trackMinimap = mapInfo;

        Height = _trackMinimap.bitmap.Height;
        Width = _trackMinimap.bitmap.Width;

        if (_config.Map.SavePreview && ACCSharedMemory.Instance.PageFileStatic.Track.Length != 0)
        {
            var trackName = ACCSharedMemory.Instance.PageFileStatic.Track.Trim() + ".jpg";
            string path = FileUtil.RaceElementTracks + trackName;
            _trackMinimap.bitmap.Save(path);
        }
    }
}
