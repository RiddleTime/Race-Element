using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

[Overlay(Name = "G-Force Trace",
Description = "A graph that shows you both lateral and longitudinal forces over time.")]
internal sealed class GForceTraceOverlay(Rectangle rectangle) : AbstractOverlay(rectangle, "G-Force Trace")
{
    private readonly GForceTraceConfiguration _config = new();

    private GForceDataJob _dataJob;

    public override void BeforeStart()
    {
        if (IsPreviewing) return;

        _dataJob = new GForceDataJob() { IntervalMillis = 20 };
        _dataJob.Run();
    }

    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        _dataJob.CancelJoin();
    }

    public override void Render(Graphics g)
    {

    }

    internal sealed class GForceDataJob : AbstractLoopJob
    {
        public sealed override void RunAction() => Collect();

        private void Collect()
        {

        }
    }

    internal readonly record struct GForceDataChunk
    {
        public int ChunkSize { get; init; }
        public float[] X { get; init; }
        public float[] Y { get; init; }

        public GForceDataChunk(float[] x, float[] y, int chunkSize)
        {
            this.X = x;
            this.Y = y;
            this.ChunkSize = chunkSize;
        }
    }
}
