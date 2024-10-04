using DeepCopy;
using RaceElement.Core.Jobs.Loop;

namespace RaceElement.Data.Common.SimulatorData.LocalCar;
internal sealed class LocalCarEventLoop : AbstractLoopJob
{
    private LocalCarData _previous = new();
    private LocalCarData _current = new();

    public LocalCarEventLoop() => IntervalMillis = 50;

    public sealed override void AfterCancel()
    {
        _previous = new();
        _current = new();
    }

    public sealed override void RunAction()
    {
        _current = DeepCopier.Copy(SimDataProvider.LocalCar);

        Inputs.CheckGearChange(_previous, _current);

        Race.CheckLapDrivenChange(_previous, _current);
        Race.CheckGlobalPositionChange(_previous, _current);
        Race.CheckClassPositionChange(_previous, _current);

        CarModel.CheckCarModelGameNameChange(_previous, _current);

        Timing.CheckHasBestLaptimeChanged(_previous, _current);

        _previous = DeepCopier.Copy(_current);
    }

    private static class Inputs
    {
        public static void CheckGearChange(LocalCarData previous, LocalCarData current)
        {
            if (previous.Inputs.Gear != current.Inputs.Gear)
                SimDataProvider.localCarEvents.Inputs.GearChanged(new() { Previous = previous.Inputs.Gear, Next = current.Inputs.Gear });
        }
    }

    private static class Race
    {
        public static void CheckGlobalPositionChange(LocalCarData previous, LocalCarData current)
        {
            if (previous.Race.GlobalPosition != current.Race.GlobalPosition)
                SimDataProvider.localCarEvents.Race.GlobalPositionChanged(new() { Previous = previous.Race.GlobalPosition, Next = current.Race.GlobalPosition });
        }

        public static void CheckClassPositionChange(LocalCarData previous, LocalCarData current)
        {
            if (previous.Race.ClassPosition != current.Race.ClassPosition)
                SimDataProvider.localCarEvents.Race.ClassPositionChanged(new() { Previous = previous.Race.ClassPosition, Next = current.Race.ClassPosition });
        }

        public static void CheckLapDrivenChange(LocalCarData previous, LocalCarData current)
        {
            if (previous.Race.LapsDriven != current.Race.LapsDriven)
                SimDataProvider.localCarEvents.Race.LapsDrivenChanged(new() { Previous = previous.Race.LapsDriven, Next = current.Race.LapsDriven });
        }
    }

    private static class CarModel
    {
        public static void CheckCarModelGameNameChange(LocalCarData previous, LocalCarData current)
        {
            if (previous.CarModel.GameName != current.CarModel.GameName)
                SimDataProvider.localCarEvents.CarModel.GameNameChanged(new() { Previous = previous.CarModel.GameName, Next = current.CarModel.GameName });
        }
    }

    private static class Timing
    {
        public static void CheckHasBestLaptimeChanged(LocalCarData previous, LocalCarData current)
        {
            if (previous.Timing.HasLapTimeBest != current.Timing.HasLapTimeBest)
                SimDataProvider.localCarEvents.Timing.HasBestLaptimeChanged(new() { Previous = previous.Timing.HasLapTimeBest, Next = current.Timing.HasLapTimeBest });
        }
    }
}

