using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;

namespace RaceElement.Data.Games.RaceRoom.Events;
internal sealed class LocalCarEventLoop : AbstractLoopJob
{

    private LocalCarData _previous = new();
    private LocalCarData _current = new();

    public LocalCarEventLoop()
    {
        this.IntervalMillis = 50;
    }

    public sealed override void RunAction()
    {
        _current = SimDataProvider.LocalCar;

        CheckLapGained();
        CheckGearChange();

        _previous = _current;
    }

    private void CheckLapGained()
    {
        if (_previous.Race.LapsDriven < _current.Race.LapsDriven)
            SimDataProvider.LocalCarEvents.GainLap(_current.Race.LapsDriven);
    }

    private void CheckGearChange()
    {
        if (_previous.Inputs.Gear != _current.Inputs.Gear)
            SimDataProvider.LocalCarEvents.GearChanged(new()
            {
                Previous = _previous.Inputs.Gear,
                Next = _current.Inputs.Gear,
            });
    }
}
