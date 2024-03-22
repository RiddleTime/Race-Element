using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;

[Overlay(Name = "G-Force Trace",
Description = "A graph that shows you both lateral and longitudinal forces over time.")]
internal sealed class GForceTraceOverlay : AbstractOverlay
{
    private readonly GForceTraceConfiguration _config = new();
    private readonly List<GForceDataChunk> _chunks = [];
    private readonly Queue<CachedBitmap> _bitmapQueue = [];

    private GForceDataJob _dataJob;
    private InfoPanel _panel;

    public GForceTraceOverlay(global::System.Drawing.Rectangle rectangle) : base(rectangle, "G-Force Trace")
    {
        RefreshRateHz = 18;
    }

    public sealed override void BeforeStart()
    {
        _panel = new(14, 500);
        Width = 500;
        Height = 250;

        if (IsPreviewing) return;

        _dataJob = new GForceDataJob(ref pagePhysics) { IntervalMillis = 1000 / 40 };
        _dataJob.OnNewDataChunk += OnNewDataChunk;
        _dataJob.Run();
    }

    private void OnNewDataChunk(GForceDataChunk chunk)
    {
        if (_chunks.Count >= _config.Chunks.MaxChunks)
            _chunks.RemoveAt(0);
        _chunks.Add(chunk);

        if (_bitmapQueue.Count > _config.Chunks.MaxChunks)
            _bitmapQueue.Dequeue();

        int height = 100;
        _bitmapQueue.Enqueue(new(GForceDataChunk.ChunkSize, height, g =>
        {
            Span<float> lateralSpan = (Span<float>)chunk.X;
            if (lateralSpan.IsEmpty)
                return;
            PointF[] xPoints = new PointF[lateralSpan.Length];
            for (int i = 0; i < lateralSpan.Length; i++)
                xPoints[i] = new PointF(i, lateralSpan[i] + 50);
            GraphicsPath xPath = new(FillMode.Winding);
            xPath.AddLines(xPoints);
            g.DrawPath(Pens.White, xPath);

            Span<float> longitudinalSpan = (Span<float>)chunk.Y;
            PointF[] yPoints = new PointF[longitudinalSpan.Length];
            for (int i = 0; i < longitudinalSpan.Length; i++)
                yPoints[i] = new PointF(i, longitudinalSpan[i] + 75);
            GraphicsPath yPath = new();
            yPath.AddLines(yPoints);
            g.DrawPath(Pens.Yellow, yPath);
        }));
    }

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _dataJob.OnNewDataChunk -= OnNewDataChunk;
        _dataJob.CancelJoin();
    }

    public sealed override bool ShouldRender() => true;

    public sealed override void Render(Graphics g)
    {
        _panel?.AddLine("Total Data Points", $"{_config.Chunks.MaxChunks * GForceDataChunk.ChunkSize}");

        if (!IsPreviewing)
        {
            //_panel?.AddProgressBarWithCenteredText($"{_chunks.Count}/{_config.Chunks.MaxChunks}", 0, _config.Chunks.MaxChunks, _chunks.Count);
            _panel?.AddProgressBarWithCenteredText($"{_dataJob.ChunkArrayIndex}/{GForceDataChunk.ChunkSize}", 0, GForceDataChunk.ChunkSize - 2, _dataJob.ChunkArrayIndex);


            g.CompositingQuality = CompositingQuality.HighQuality;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int x = _dataJob.ChunkArrayIndex;
            for (int i = _bitmapQueue.Count - 1; i >= 0; i--)
            {
                _bitmapQueue.ElementAt(i).Draw(g, new Point(x, 0));
                x += GForceDataChunk.ChunkSize;
                if (x > this.Width) { break; }
            }
        }

        _panel?.Draw(g);
    }

    internal sealed class GForceDataJob : AbstractLoopJob
    {
        public Action<GForceDataChunk> OnNewDataChunk;

        public int ChunkArrayIndex { get; private set; }
        private GForceDataChunk _activeChunk = new();
        private readonly ACCSharedMemory.SPageFilePhysics _pagePhysics;

        public GForceDataJob(ref ACCSharedMemory.SPageFilePhysics pagePhysics)
        {
            _pagePhysics = pagePhysics;
        }

        public sealed override void RunAction()
        {
            if (ChunkArrayIndex < _activeChunk.X.Length - 1)
                Collect();
            else
                Send();
        }

        private void Send()
        {
            OnNewDataChunk(_activeChunk);
            _activeChunk = new();
            ChunkArrayIndex = 0;
        }

        private void Collect()
        {
            _activeChunk.X[ChunkArrayIndex] = _pagePhysics.AccG[0];
            _activeChunk.Y[ChunkArrayIndex] = _pagePhysics.AccG[2];
            ChunkArrayIndex++;
        }
    }

    internal readonly record struct GForceDataChunk
    {
        public const int ChunkSize = 32;
        public readonly float[] X { get; init; } = new float[ChunkSize];
        public readonly float[] Y { get; init; } = new float[ChunkSize];
        public GForceDataChunk()
        {
            X = new float[ChunkSize];
            Y = new float[ChunkSize];
        }
    }
}
