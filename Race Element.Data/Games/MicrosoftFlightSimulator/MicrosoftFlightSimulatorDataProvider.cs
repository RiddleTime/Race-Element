using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Common.SimulatorData.LocalPlane;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceElement.Data.Games.MicrosoftFlightSimulator;

internal sealed class MicrosoftFlightSimulatorDataProvider : AbstractSimDataProvider
{
    internal override int PollingRate() => 100;


    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;



    public void UpdateFlightData(ref LocalPlaneData localPlane)
    {
        localPlane.GroundSpeed = 40.0d;
    }


    /// <summary>
    /// Unused, as this is a flight simulator
    /// </summary>
    /// <param name="localCar"></param>
    /// <param name="sessionData"></param>
    /// <param name="gameData"></param>
    [Obsolete("Don't use as this data provider is a flight simulator and not a racing game.")]
    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
    }


    internal override void Start()
    {
    }

    internal override void Stop()
    {
    }
}
