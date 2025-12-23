using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace RaceElement.HUD.Common.Overlays.Driving.AccelerationTester;

[Overlay(
    Name = "Acceleration Tester",
    Description = "Pull the handbrake for 1 second",
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class AccelerationTester : CommonAbstractOverlay
{



    public AccelerationTester(Rectangle rectangle) : base(rectangle, "Acceleration Tester")
    {
    }

    public override void Render(Graphics g)
    {

    }

    internal enum AccelerationPhase
    {
        Reset,
        HandbrakePulled,
        Ready,
        Accelerating,
        Completed
    }

    private class AccelerationTimingJob : AbstractLoopJob
    {
        private long _lastHandbrakePullTime = default;
        private static bool IsHandBrakePulled { get => SimDataProvider.LocalCar.Inputs.HandBrake > 0; }

        public AccelerationPhase Phase = AccelerationPhase.Reset;

        public override void RunAction()
        {
            switch (Phase)
            {
                case AccelerationPhase.Reset:
                    {
                        if (IsHandBrakePulled && SimDataProvider.LocalCar.Physics.Velocity <= 0.5f)
                        {
                            if (_lastHandbrakePullTime == default)
                            {
                                _lastHandbrakePullTime = TimeProvider.System.GetTimestamp();
                            }
                            Phase = AccelerationPhase.HandbrakePulled;
                        }
                        break;
                    }
                case AccelerationPhase.HandbrakePulled:
                    {
                        break;
                    }
                case AccelerationPhase.Ready:
                    {
                        break;
                    }
                case AccelerationPhase.Accelerating:
                    {
                        break;
                    }
                case AccelerationPhase.Completed:
                    {
                        break;
                    }
            }
        }
    }
}
