using System.Drawing;
using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Games;

public abstract class AbstractSimDataProvider
{
    // Get color to be used for a car class (GT3, GT4,..)
    abstract public Color GetColorForCarClass(String carClass);

    // Get color to be used for a driver's category (Gold, A-License,..)
    abstract public Color? GetColorForCategory(string category);
    abstract public List<string> GetCarClasses();    
    abstract public void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData);

    abstract internal void Stop();

    abstract public bool HasTelemetry();

    public virtual void SetupPreviewData() { }

    public virtual bool IsSpectating(int playerCarIndex, int focusedIndex)
    {
        throw new NotImplementedException();
    }        
}
