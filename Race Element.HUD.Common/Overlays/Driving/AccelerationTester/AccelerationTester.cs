using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    private readonly InfoPanel _infoPanel;

    private readonly AccelerationTimingJob _timingJob;

    private class AccelerationTesterConfiguration : OverlayConfiguration
    {

    }

    public AccelerationTester(Rectangle rectangle) : base(rectangle, "Acceleration Tester")
    {
        Width = 600;
        Height = 300;

        _infoPanel = new InfoPanel(12, 600);
        _timingJob = new AccelerationTimingJob() { IntervalMillis = 10 };
    }

    public override void BeforeStart()
    {
        _timingJob?.Run();
    }

    public override void BeforeStop()
    {
        _infoPanel?.Dispose();
        _timingJob?.CancelJoin();
    }

    public override void Render(Graphics g)
    {
        _infoPanel.AddLine("Phase", _timingJob?.PhaseDescriptions[_timingJob.Phase]);
        _infoPanel.AddLine("0-100 km/h", _timingJob?.RecordedTimes[AccelerationTypes.ZeroToHundred] == default ? "-" : _timingJob.RecordedTimes[AccelerationTypes.ZeroToHundred].ToString(@"s\.fff") + " s");
        _infoPanel.AddLine("100-200 km/h", _timingJob?.RecordedTimes[AccelerationTypes.HundredToTwoHundred] == default ? "-" : _timingJob.RecordedTimes[AccelerationTypes.HundredToTwoHundred].ToString(@"s\.fff") + " s");
        _infoPanel.AddLine("200-300 km/h", _timingJob?.RecordedTimes[AccelerationTypes.TwoHundredToThreeHundred] == default ? "-" : _timingJob.RecordedTimes[AccelerationTypes.TwoHundredToThreeHundred].ToString(@"s\.fff") + " s");
        _infoPanel.Draw(g);
    }

    internal enum AccelerationPhase
    {
        Reset,
        HandbrakePulled,
        Ready,
        Accelerating,
        Completed
    }

    internal enum AccelerationTypes
    {
        ZeroToHundred,
        HundredToTwoHundred,
        TwoHundredToThreeHundred,
    }

    private class AccelerationTimingJob : AbstractLoopJob
    {
        private long _lastHandbrakePullTime = default;
        private long _accelerationStartTime = default;
        private static bool IsHandBrakePulled { get => SimDataProvider.LocalCar.Inputs.HandBrake > 0; }

        public AccelerationPhase Phase = AccelerationPhase.Reset;

        public Dictionary<AccelerationPhase, string> PhaseDescriptions = new()
        {
            { AccelerationPhase.Reset, "Release handbrake and stop the car." },
            { AccelerationPhase.HandbrakePulled, "Hold the handbrake for 1 second." },
            { AccelerationPhase.Ready, "Ready! Release the handbrake to start accelerating." },
            { AccelerationPhase.Accelerating, "Accelerating..." },
            { AccelerationPhase.Completed, "Completed!" }
        };

        public Dictionary<AccelerationTypes, string> AccelerationTypeDescriptions = new()
        {
            { AccelerationTypes.ZeroToHundred, "0-100 km/h" },
            { AccelerationTypes.HundredToTwoHundred, "100-200 km/h" },
            { AccelerationTypes.TwoHundredToThreeHundred, "200-300 km/h" },
        };

        public Dictionary<AccelerationTypes, TimeSpan> RecordedTimes = new()
        {
            { AccelerationTypes.ZeroToHundred, default },
            { AccelerationTypes.HundredToTwoHundred, default },
            { AccelerationTypes.TwoHundredToThreeHundred, default },
        };

        public override void RunAction()
        {
            Debug.WriteLine(Phase);
            switch (Phase)
            {
                case AccelerationPhase.Reset:
                    {
                        RecordedTimes[AccelerationTypes.ZeroToHundred] = default;
                        RecordedTimes[AccelerationTypes.HundredToTwoHundred] = default;
                        RecordedTimes[AccelerationTypes.TwoHundredToThreeHundred] = default;

                        if (IsHandBrakePulled && SimDataProvider.LocalCar.Physics.Velocity < 0.5f)
                        {
                            _lastHandbrakePullTime = TimeProvider.System.GetTimestamp();
                            Phase = AccelerationPhase.HandbrakePulled;
                        }

                        break;
                    }
                case AccelerationPhase.HandbrakePulled:
                    {
                        if (!IsHandBrakePulled)
                        {
                            _lastHandbrakePullTime = default;
                            Phase = AccelerationPhase.Reset;
                        }

                        if (SimDataProvider.LocalCar.Physics.Velocity < 0.1f)
                        {
                            if (TimeProvider.System.GetElapsedTime(_lastHandbrakePullTime) >= TimeSpan.FromSeconds(1))
                                Phase = AccelerationPhase.Ready;
                        }
                        break;
                    }
                case AccelerationPhase.Ready:
                    {
                        if (!IsHandBrakePulled)
                        {
                            Phase = AccelerationPhase.Accelerating;
                            _accelerationStartTime = TimeProvider.System.GetTimestamp();
                            break;
                        }

                        break;
                    }
                case AccelerationPhase.Accelerating:
                    {
                        if (SimDataProvider.LocalCar.Inputs.Throttle < 0.1f)
                        {
                            Phase = AccelerationPhase.Reset;
                            break;
                        }

                        if (RecordedTimes[AccelerationTypes.ZeroToHundred] == default && SimDataProvider.LocalCar.Physics.Velocity >= 100f)
                        {
                            RecordedTimes[AccelerationTypes.ZeroToHundred] = TimeProvider.System.GetElapsedTime(_accelerationStartTime);
                        }

                        if (RecordedTimes[AccelerationTypes.HundredToTwoHundred] == default && SimDataProvider.LocalCar.Physics.Velocity >= 200f)
                        {
                            RecordedTimes[AccelerationTypes.HundredToTwoHundred] = TimeProvider.System.GetElapsedTime(_accelerationStartTime) - RecordedTimes[AccelerationTypes.ZeroToHundred];
                        }

                        if (RecordedTimes[AccelerationTypes.TwoHundredToThreeHundred] == default && SimDataProvider.LocalCar.Physics.Velocity >= 300f)
                        {
                            RecordedTimes[AccelerationTypes.TwoHundredToThreeHundred] = TimeProvider.System.GetElapsedTime(_accelerationStartTime) - RecordedTimes[AccelerationTypes.ZeroToHundred] - RecordedTimes[AccelerationTypes.HundredToTwoHundred];
                            Phase = AccelerationPhase.Completed;
                        }

                        break;
                    }
                case AccelerationPhase.Completed:
                    {
                        if (SimDataProvider.LocalCar.Physics.Velocity < 1 && IsHandBrakePulled)
                        {
                            Phase = AccelerationPhase.Reset;
                        }
                        break;
                    }
            }
        }
    }
}
