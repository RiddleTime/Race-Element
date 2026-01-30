using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Drawing;

namespace RaceElement.HUD.Common.Overlays.Driving.AccelerationTester;

[Overlay(
    Name = "Acceleration Tester",
    Description = "Ready? Set! Go! The Acceleration Tester." +
    "\nPrecision is Limited to Simulator Specification.",
    Game = Game.ForzaHorizon5,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class AccelerationTester : CommonAbstractOverlay
{
    private readonly InfoPanel _infoPanel;
    private readonly AccelerationTimingJob _timingJob;

    public AccelerationTester(Rectangle rectangle) : base(rectangle, "Acceleration Tester")
    {
        Width = 400;
        Height = 200;

        _infoPanel = new InfoPanel(12, 400);
        _timingJob = new AccelerationTimingJob() { IntervalMillis = 5 };
    }

    public sealed override void BeforeStart()
    {
        if (!IsPreviewing)
            _timingJob?.Run();
    }


    public sealed override void BeforeStop()
    {
        _infoPanel?.Dispose();
        if (!IsPreviewing)
            _timingJob?.CancelJoin();
    }

    public sealed override void Render(Graphics g)
    {
        if (_timingJob == null)
            return;

        _infoPanel.AddLine("Phase", _timingJob?.PhaseDescriptions[_timingJob.Phase]);

        TimeSpan previous = TimeSpan.Zero;
        foreach (var accType in Enum.GetValues<AccelerationTimingJob.AccelerationTypes>())
        {
            TimeSpan fromZero = _timingJob.RecordedTimes[accType];
            TimeSpan fromPrevious = fromZero - previous;
            if (accType != AccelerationTimingJob.AccelerationTypes.ZeroToHundred) // show delta times
                _infoPanel.AddLine(_timingJob.DeltaAccelerationTypeDescriptions[accType], _timingJob.RecordedTimes[accType] == default ? "-" : fromPrevious.ToString(@"s\.fff") + " s");

            _infoPanel.AddLine(_timingJob.ZeroToAccelerationTypeDescriptions[accType], _timingJob.RecordedTimes[accType] == default ? "-" : fromZero.ToString(@"s\.fff") + " s");
            previous += fromPrevious;
        }

        _infoPanel.Draw(g);
    }

    private sealed class AccelerationTimingJob : AbstractLoopJob
    {
        private long _lastHandbrakePullTime = default;
        private long _accelerationStartTime = default;
        private static bool IsHandBrakePulled { get => SimDataProvider.LocalCar.Inputs.HandBrake > 0; }

        public AccelerationPhase Phase = AccelerationPhase.Reset;

        public enum AccelerationPhase
        {
            Reset,
            HandbrakePulled,
            Ready,
            Accelerating,
            Completed
        }

        public enum AccelerationTypes
        {
            ZeroToHundred,
            HundredToTwoHundred,
            TwoHundredToThreeHundred,
            ThreeHundredToFourHundred,
        }


        public readonly Dictionary<AccelerationPhase, string> PhaseDescriptions = new()
        {
            { AccelerationPhase.Reset, "Stop car & Hold Handbrake" },
            { AccelerationPhase.HandbrakePulled, "Hold Handbrake for 1 Sec" },
            { AccelerationPhase.Ready, "Ready? Release Handbrake!" },
            { AccelerationPhase.Accelerating, "Accelerating..." },
            { AccelerationPhase.Completed, "Completed!" }
        };

        public readonly Dictionary<AccelerationTypes, string> DeltaAccelerationTypeDescriptions = new()
        {
            { AccelerationTypes.ZeroToHundred, "0-100 km/h" },
            { AccelerationTypes.HundredToTwoHundred, "100-200 km/h" },
            { AccelerationTypes.TwoHundredToThreeHundred, "200-300 km/h" },
            { AccelerationTypes.ThreeHundredToFourHundred, "300-400 km/h" },
        };

        public readonly Dictionary<AccelerationTypes, string> ZeroToAccelerationTypeDescriptions = new()
        {
            { AccelerationTypes.ZeroToHundred, "0-100 km/h" },
            { AccelerationTypes.HundredToTwoHundred, "0-200 km/h" },
            { AccelerationTypes.TwoHundredToThreeHundred, "0-300 km/h" },
            { AccelerationTypes.ThreeHundredToFourHundred, "0-400 km/h" },
        };

        public readonly Dictionary<AccelerationTypes, TimeSpan> RecordedTimes = new()
        {
            { AccelerationTypes.ZeroToHundred, default },
            { AccelerationTypes.HundredToTwoHundred, default },
            { AccelerationTypes.TwoHundredToThreeHundred, default },
            { AccelerationTypes.ThreeHundredToFourHundred, default },
        };

        public readonly Dictionary<AccelerationTypes, float> AccelerationTresholds = new()
        {
            { AccelerationTypes.ZeroToHundred, 100 },
            { AccelerationTypes.HundredToTwoHundred, 200 },
            { AccelerationTypes.TwoHundredToThreeHundred, 300 },
            { AccelerationTypes.ThreeHundredToFourHundred, 400 },
        };

        public sealed override void RunAction()
        {
            switch (Phase)
            {
                case AccelerationPhase.Reset:
                    {
                        foreach (var accType in Enum.GetValues<AccelerationTypes>())
                            RecordedTimes[accType] = default;

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
                        if (!IsHandBrakePulled && SimDataProvider.LocalCar.Physics.Velocity > 0.1f)
                        {
                            Phase = AccelerationPhase.Accelerating;
                            _accelerationStartTime = TimeProvider.System.GetTimestamp();
                            break;
                        }

                        break;
                    }
                case AccelerationPhase.Accelerating:
                    {
                        if (SimDataProvider.LocalCar.Physics.Velocity < 1 && IsHandBrakePulled)
                        {
                            Phase = AccelerationPhase.Reset;
                        }

                        foreach (var accType in Enum.GetValues<AccelerationTypes>())
                            foreach (var treshold in AccelerationTresholds)
                                if (accType == treshold.Key && RecordedTimes[accType] == default && SimDataProvider.LocalCar.Physics.Velocity >= treshold.Value)
                                    RecordedTimes[accType] = TimeProvider.System.GetElapsedTime(_accelerationStartTime);

                        if (SimDataProvider.LocalCar.Inputs.Brake > 0)
                            Phase = AccelerationPhase.Completed;

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
