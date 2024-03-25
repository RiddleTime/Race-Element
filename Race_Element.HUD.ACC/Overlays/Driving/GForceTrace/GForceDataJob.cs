using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.HUD.ACC.Overlays.Driving.GForceTrace
{
    internal class GForceDataJob : AbstractLoopJob
    {
        public GForceTraceOverlay Overlay { get; init; }
        public int DataCount { get; init; }
        public readonly LinkedList<int> Lateral = [];
        public readonly LinkedList<int> Longitudinal = [];

        public GForceDataJob(GForceTraceOverlay overlay, int dataCount)
        {
            Overlay = overlay;
            DataCount = dataCount;
            for (int i = 0; i < DataCount; i++)
            {
                Lateral.AddFirst(45);
                Longitudinal.AddFirst(55);
            }
        }

        public override void RunAction()
        {
            if (!Overlay.ShouldRender())
                return;

            SPageFilePhysics filePhysics = Overlay.pagePhysics;
            lock (Lateral)
            {

                float latG = filePhysics.AccG[0] *= -1;
                float maxLatG = Overlay._config.Data.MaxLatG;
                latG.Clip(-maxLatG, maxLatG);
                latG += maxLatG;

                int data = (int)(latG * 100 / (maxLatG * 2));
                Lateral.AddFirst(data);
                if (Lateral.Count > DataCount)
                    Lateral.RemoveLast();
            }

            lock (Longitudinal)
            {

                float longG = filePhysics.AccG[2] *= -1;
                float maxLongG = Overlay._config.Data.MaxLongG;
                longG.Clip(-maxLongG, maxLongG);
                longG += maxLongG;

                int data = (int)(longG * 100 / (maxLongG * 2));
                Longitudinal.AddFirst(data);
                if (Longitudinal.Count > DataCount)
                    Longitudinal.RemoveLast();
            }
        }
    }
}
