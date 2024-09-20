using System.Drawing;
using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Games;

public abstract class AbstractSimDataProvider
{
    internal abstract void Start();
    /// <summary>
    /// Update Polling Hz
    /// </summary>
    /// <returns></returns>
    internal abstract int PollingRate();
    internal abstract void Stop();


    #region Decouple
    /// <summary>
    /// Get color to be used for a car class (GT3, GT4,..)
    /// </summary>
    /// <param name="carClass">Name of the class. This depends on the sim.</param>
    /// <returns>Color to be used for this car class in HUDs.</returns>
    public virtual Color GetColorForCarClass(String carClass) => Color.White;

    /// <summary>
    /// Get color to be used for a driver's category (Gold, A-License,..)
    /// </summary>
    /// <param name="category">Sim dependent category name. E.g. in iRacing "A 2.7" for an A license driver, which returns blue. In ACC this could be "Gold" or an LFM license level.</param>
    /// <returns>The color to be used for this category in the HUDs.</returns>
    public virtual Color GetColorForCategory(string category) => Color.Gray;
    public abstract List<string> GetCarClasses();
    public abstract void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData);

    public abstract bool HasTelemetry();

    public virtual void SetupPreviewData() { }

    public virtual bool IsSpectating(int playerCarIndex, int focusedIndex)
    {
        throw new NotImplementedException();
    }

    #endregion
}
