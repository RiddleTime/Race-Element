using System.Drawing;
using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Games;

public abstract class AbstractSimDataProvider
{
    abstract public Color GetColorForCarClass(String carClass);
    abstract public List<string> GetCarClasses();
    // TODO: Should we allow for both Update being driven and the simracing data provider doing its own polls? E.g. Update could return true if 
    // all future updates can be handes w/o calls to Update
    abstract public void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData);

    abstract internal void Stop();

    abstract public bool HasTelemetry();

    public virtual void SetupPreviewData() { }



    
}
