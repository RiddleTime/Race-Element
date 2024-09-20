using DeepCopy;
using RaceElement.Core.Jobs.LoopJob;

namespace RaceElement.Data.Common.SimulatorData;
internal sealed class LocalCarEventLoop : AbstractLoopJob
{
    private LocalCarData _previous = new();
    private LocalCarData _current = new();

    public LocalCarEventLoop() => IntervalMillis = 50;

    public override void AfterCancel()
    {
        _previous = new();
        _current = new();
    }

    public sealed override void RunAction()
    {
        _current = SimDataProvider.LocalCar;

        CheckGearChange();

        CheckLapGained();
        CheckGlobalPositionChange();
        CheckClassPositionChange();

        _previous = DeepCopier.Copy(_current);
    }

    private void CheckGlobalPositionChange()
    {
        if (_previous.Race.GlobalPosition != _current.Race.GlobalPosition)
            SimDataProvider.localCarEvents.GlobalPositionChanged(new() { Previous = _previous.Race.GlobalPosition, Next = _current.Race.GlobalPosition });
    }

    private void CheckClassPositionChange()
    {
        if (_previous.Race.ClassPosition != _current.Race.ClassPosition)
            SimDataProvider.localCarEvents.ClassPositionChanged(new() { Previous = _previous.Race.ClassPosition, Next = _current.Race.ClassPosition });
    }

    private void CheckLapGained()
    {
        if (_previous.Race.LapsDriven != _current.Race.LapsDriven)
            SimDataProvider.localCarEvents.GainLap(_current.Race.LapsDriven);
    }

    private void CheckGearChange()
    {
        if (_previous.Inputs.Gear != _current.Inputs.Gear)
            SimDataProvider.localCarEvents.GearChanged(new() { Previous = _previous.Inputs.Gear, Next = _current.Inputs.Gear, });
    }
}

