using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
//using Svg;
using System.Drawing;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayTrackMap;

#if DEBUG
[Overlay(Name = "Track Map",
    Description = "Shows a track map",
    OverlayCategory = OverlayCategory.Track,
    OverlayType = OverlayType.Drive)]
#endif
internal sealed class TrackMapOverlay : AbstractOverlay
{
    private readonly TrackMapConfiguration _config = new();
    private class TrackMapConfiguration : OverlayConfiguration
    {

    }

    private Bitmap map;
    public TrackMapOverlay(Rectangle rectangle) : base(rectangle, "Track Map")
    {
        Width = 520;
        Height = 380;
        RefreshRateHz = 1;
    }

    public override void BeforeStart()
    {

    }

    public override void BeforeStop()
    {
        map?.Dispose();
    }

    //private void ProcessNodes(IEnumerable<SvgElement> nodes, SvgPaintServer colorServer)
    //{
    //    foreach (var node in nodes)
    //    {
    //        if (node.Fill != SvgPaintServer.None) node.Fill = colorServer;
    //        if (node.Color != SvgPaintServer.None) node.Color = colorServer;
    //        if (node.Stroke != SvgPaintServer.None) node.Stroke = colorServer;
    //        node.StrokeWidth = 1;

    //        ProcessNodes(node.Descendants(), colorServer);
    //    }
    //}

    public override void Render(Graphics g)
    {
        //var names = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.EndsWith(".svg"));

        //var name = names.ElementAt(new Random().Next(0, names.Count() - 1));

        //XmlDocument doc = new XmlDocument();
        //doc.Load(Assembly.GetExecutingAssembly().GetManifestResourceStream(name));
        //var svgDoc = SvgDocument.Open(doc);
        //svgDoc.Fill = new SvgColourServer(Color.FromArgb(130, 0, 0, 0));
        //ProcessNodes(svgDoc.Descendants(), new SvgColourServer(Color.White));
        //map = svgDoc.Draw();

        //g.DrawImage(map, 0, 0, Width, Height);

    }
}
