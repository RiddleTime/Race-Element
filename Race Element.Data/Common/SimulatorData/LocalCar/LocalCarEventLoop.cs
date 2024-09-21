﻿using DeepCopy;
using RaceElement.Core.Jobs.LoopJob;

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

        CheckGearChange();

        CheckLapDrivenChange();
        CheckGlobalPositionChange();
        CheckClassPositionChange();

        CheckCarModelGameNameChange();

        _previous = DeepCopier.Copy(_current);
    }

    private void CheckGlobalPositionChange()
    {
        if (_previous.Race.GlobalPosition != _current.Race.GlobalPosition)
            SimDataProvider.localCarEvents.Race.GlobalPositionChanged(new() { Previous = _previous.Race.GlobalPosition, Next = _current.Race.GlobalPosition });
    }

    private void CheckClassPositionChange()
    {
        if (_previous.Race.ClassPosition != _current.Race.ClassPosition)
            SimDataProvider.localCarEvents.Race.ClassPositionChanged(new() { Previous = _previous.Race.ClassPosition, Next = _current.Race.ClassPosition });
    }

    private void CheckLapDrivenChange()
    {
        if (_previous.Race.LapsDriven != _current.Race.LapsDriven)
            SimDataProvider.localCarEvents.Race.LapsDrivenChanged(new() { Previous = _previous.Race.LapsDriven, Next = _current.Race.LapsDriven });
    }

    private void CheckGearChange()
    {
        if (_previous.Inputs.Gear != _current.Inputs.Gear)
            SimDataProvider.localCarEvents.Inputs.GearChanged(new() { Previous = _previous.Inputs.Gear, Next = _current.Inputs.Gear });
    }

    private void CheckCarModelGameNameChange()
    {
        if (_previous.CarModel.GameName != _current.CarModel.GameName)
            SimDataProvider.localCarEvents.CarModel.GameNameChanged(new() { Previous = _previous.CarModel.GameName, Next = _current.CarModel.GameName });
    }
}
