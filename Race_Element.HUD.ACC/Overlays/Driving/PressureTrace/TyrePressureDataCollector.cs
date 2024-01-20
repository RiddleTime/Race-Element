using RaceElement.Util.SystemExtensions;
using System.Collections.Generic;
using System.Threading;
using static RaceElement.ACCSharedMemory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.OverlayPressureTrace;

internal class TyrePressureDataCollector
{
    private bool IsCollecting = false;
    public int TraceCount = 300;

    public LinkedList<float> FrontLeft = new();
    public LinkedList<float> FrontRight = new();
    public LinkedList<float> RearLeft = new();
    public LinkedList<float> RearRight = new();

    public void Collect(SPageFilePhysics filePhysics)
    {
        lock (FrontLeft)
        {
            FrontLeft.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.FrontLeft]));
            if (FrontLeft.Count > TraceCount)
            {
                FrontLeft.RemoveLast();
            }
        }
        lock (FrontRight)
        {
            FrontRight.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.FrontRight]));
            if (FrontRight.Count > TraceCount)
            {
                FrontRight.RemoveLast();
            }
        }
        lock (RearLeft)
        {
            RearLeft.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.RearLeft]));
            if (RearLeft.Count > TraceCount)
            {
                RearLeft.RemoveLast();
            }
        }
        lock (RearRight)
        {
            RearRight.AddFirst(CorrectToBounds(filePhysics.WheelPressure[(int)Wheel.RearRight]));
            if (RearRight.Count > TraceCount)
            {
                RearRight.RemoveLast();
            }
        }

    }

    private float CorrectToBounds(float value)
    {
        float min = (float)(TyrePressureGraph.PressureRange.OptimalMinimum - TyrePressureGraph.Padding);
        float max = (float)(TyrePressureGraph.PressureRange.OptimalMaximum + TyrePressureGraph.Padding);
        value.Clip(min, max);
        return value;
    }

    public void Start()
    {
        IsCollecting = true;

        new Thread(x =>
        {
            while (IsCollecting)
            {
                Thread.Sleep(1000 / 3);
                if (PressureTraceOverlay.Instance != null && PressureTraceOverlay.Instance.pagePhysics != null)
                {
                    Collect(PressureTraceOverlay.Instance.pagePhysics);
                    PressureTraceOverlay.Instance.RequestRedraw();
                }
            }
        }).Start();
    }

    public void Stop()
    {
        IsCollecting = false;
    }
}
