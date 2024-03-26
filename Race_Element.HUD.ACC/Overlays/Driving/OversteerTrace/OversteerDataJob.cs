using RaceElement.Core.Jobs.LoopJob;
using RaceElement.HUD.ACC.Overlays.Driving.GForceTrace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RaceElement.HUD.ACC.Overlays.Driving.OversteerTrace
{
    internal sealed class OversteerDataJob : AbstractLoopJob
    {
        public OversteerTraceOverlay Overlay { get; private init; }
        public int DataCount { get; private init; }

        public readonly LinkedList<int> Oversteer = [];
        public readonly LinkedList<int> Understeer = [];

        public OversteerDataJob(OversteerTraceOverlay overlay, int dataCount)
        {
            Overlay = overlay;
            DataCount = dataCount;

            int presetOversteer = 0, presetUndersteer = 0;
            if (Overlay.IsPreviewing)
                presetUndersteer = 2;

            for (int i = 0; i < DataCount; i++)
            {
                Oversteer.AddFirst(presetOversteer);
                Understeer.AddFirst(presetUndersteer);
            }
        }

        public sealed override void RunAction()
        {

        }
    }
}
