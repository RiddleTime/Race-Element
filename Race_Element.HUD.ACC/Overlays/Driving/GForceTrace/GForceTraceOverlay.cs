using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
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
    private InfoPanel _panel;

    public override void BeforeStart()
    {
        if (IsPreviewing) return;

        _dataJob = new GForceDataJob() { IntervalMillis = 20 };
        _dataJob.OnNewDataChunk += OnNewDataChunk;
        _dataJob.Run();

        _panel = new(14, 500);
        Width = 500;
        Height = 500;
    }

    private void OnNewDataChunk(GForceDataChunk chunk)
    {
        if (_chunks.Count >= _config.Chunks.MaxChunks)
            _chunks.RemoveAt(0);
        _chunks.Add(chunk);
    }

    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        _dataJob.OnNewDataChunk -= OnNewDataChunk;
        _dataJob.CancelJoin();
    }

    public override bool ShouldRender() => true;

    public override void Render(Graphics g)
    {
        _panel.AddProgressBarWithCenteredText($"{_chunks.Count}/{_config.Chunks.MaxChunks}", 0, _config.Chunks.MaxChunks, _chunks.Count);
        _panel.AddLine("Total Data Points", $"{_config.Chunks.MaxChunks * GForceDataChunk.ChunkSize}");
        _panel.Draw(g);
    }

    internal sealed class GForceDataJob : AbstractLoopJob
    {
        public Action<GForceDataChunk> OnNewDataChunk;

        private int _chunkArrayIndex = 0;
        private GForceDataChunk _dataChunk = new();

        public sealed override void RunAction()
        {
            if (_chunkArrayIndex < _dataChunk.X.Length - 1)
                Collect();
            else
                Send();
        }

        private void Send()
        {
            OnNewDataChunk(_dataChunk);
            _dataChunk = new();
            _chunkArrayIndex = 0;
        }

        private void Collect()
        {
            _dataChunk.X[_chunkArrayIndex] = 0f;
            _dataChunk.Y[_chunkArrayIndex] = 0f;
            _chunkArrayIndex++;
        }
    }

    internal readonly record struct GForceDataChunk
    {
        public const int ChunkSize = 256;
        public readonly float[] X { get; init; } = new float[ChunkSize];
        public readonly float[] Y { get; init; } = new float[ChunkSize];
        public GForceDataChunk()
        {
            X = new float[ChunkSize];
            Y = new float[ChunkSize];
        }
    }
}
