using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private List<GForceDataChunk> _chunks = [];

    private GForceDataJob _dataJob;

    public override void BeforeStart()
    {
        if (IsPreviewing) return;

        _dataJob = new GForceDataJob() { IntervalMillis = 20 };
        _dataJob.OnNewDataChunk += OnNewDataChunk;
        _dataJob.Run();
    }

    private void OnNewDataChunk(GForceDataChunk chunk)
    {
        if (_chunks.Count >= _config.Chunks.MaxChunks)
            _chunks.RemoveAt(0);
        _chunks.Add(chunk);

        Debug.WriteLine(_chunks.Count);
    }

    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        _dataJob.OnNewDataChunk -= OnNewDataChunk;
        _dataJob.CancelJoin();
    }

    public override void Render(Graphics g)
    {

    }

    internal sealed class GForceDataJob : AbstractLoopJob
    {
        private GForceDataChunk _dataChunk = new();

        private int index = 0;
        public sealed override void RunAction() => Collect();

        public Action<GForceDataChunk> OnNewDataChunk;
        private void Collect()
        {
            if (index < _dataChunk.X.Length - 1)
            {
                _dataChunk.X[index] = 0f;
                _dataChunk.Y[index] = 0f;
                index++;
            }
            else
            {
                OnNewDataChunk(_dataChunk);
                _dataChunk = new();
                index = 0;
            }
        }
    }

    internal readonly record struct GForceDataChunk
    {
        private const int ChunkSize = 256;
        public readonly float[] X { get; init; } = new float[ChunkSize];
        public readonly float[] Y { get; init; } = new float[ChunkSize];
        public GForceDataChunk()
        {
            X = new float[ChunkSize];
            Y = new float[ChunkSize];
        }
    }
}
