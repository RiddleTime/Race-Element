using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text;

using static RaceElement.HUD.Common.Overlays.Driving.DualSense.DS5W;

namespace RaceElement.HUD.Common.Overlays.Driving.DualSense;

[Overlay(Name = "DualSense",
    Description = "Adds trigger effects to the DualSense Controller.\n See Guide in the Discord of Race Element for instructions.",
    OverlayCategory = OverlayCategory.Inputs,
    OverlayType = OverlayType.Drive,
    Game = Game.RaceRoom | Game.AssettoCorsa1 | Game.AssettoCorsaEvo,
    Authors = ["Reinier Klarenberg", "Guillaume Stordeur"]
)]
internal sealed class DualSenseOverlay : CommonAbstractOverlay
{
    internal readonly DualSenseConfiguration _config = new();
    private DualSenseJob _DualSenseJob;

    public DualSenseOverlay(Rectangle rectangle) : base(rectangle, "DualSense")
    {
        Width = 1; Height = 1;
        RefreshRateHz = 1;
        AllowReposition = false;
    }

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        ExtractDs5ApiDll();

        ds5w_init();
        _DualSenseJob = new DualSenseJob(this) { IntervalMillis = 1000 / 200 };
        _DualSenseJob.Run();
    }
    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _DualSenseJob?.CancelJoin();
        ds5w_shutdown();
    }

    /// <summary>
    /// Extracts the embbed dll in this namespace folder, to the executing assembly folder.
    /// only extracts if it does not exists. Does not check for version!!!
    /// </summary>
    private static void ExtractDs5ApiDll()
    {
        string dllPath = Path.Combine(AppContext.BaseDirectory, Path.GetFileName("ds5w_x64.dll"));
        FileInfo dllFile = new(dllPath);
        if (dllFile.Exists) return;

        string resourceName = "RaceElement.HUD.Common.Overlays.Driving.DualSense.ds5w_x64.dll";
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
                throw new FileNotFoundException($"Could not find resource: {resourceName}");

            using var fileStream = dllFile.Open(FileMode.Create, FileAccess.Write);
            stream.CopyTo(fileStream);
            stream.Close();
            fileStream.Close();
        }
    }

    public sealed override bool ShouldRender() => DefaultShouldRender() && !IsPreviewing;

    public sealed override void Render(Graphics g) { }

}
