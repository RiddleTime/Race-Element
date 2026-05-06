using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Common.SimulatorData.LocalPlane;
using SimConnect.NET;
using SimConnect.NET.Aircraft;
using System.Diagnostics;

namespace RaceElement.Data.Games.MicrosoftFlightSimulator;

internal sealed class MicrosoftFlightSimulatorDataProvider : AbstractSimDataProvider
{
    private SimConnectClient _simConnectClient;

    internal override int PollingRate() => 100;
    internal override void Start()
    {
        _simConnectClient = new("Race Element")
        {
            MaxReconnectAttempts = 1,
        };
        _simConnectClient.ConnectionStatusChanged += SimConnectClient_ConnectionStatusChanged;

    }

    private void SimConnectClient_ConnectionStatusChanged(object? sender, SimConnect.NET.Events.ConnectionStatusChangedEventArgs e)
    {
        if (e.IsConnected) Debug.WriteLine("Connected with SimConnect!");
        if (e.IsDisconnected) Debug.WriteLine("Disconnected from Simconnect!");
    }

    internal override void Stop()
    {
        _simConnectClient.DisconnectAsync().Wait();
        _simConnectClient.Dispose();
    }


    public void UpdateFlightData(ref LocalPlaneData localPlane)
    {
        if (_simConnectClient == null)
            return;

        if (!_simConnectClient.IsConnected)
        {
            Connect();
            Thread.Sleep(4000);
            return;
        }


        AircraftMotion motion;
        try
        {
            motion = _simConnectClient.Aircraft.GetMotionAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            return;
        }

        localPlane.IndicatedAirSpeed = motion.IndicatedAirspeed;
        localPlane.GroundSpeed = motion.GroundSpeed;
        localPlane.VerticalSpeed = motion.VerticalSpeed;
    }

    private void Connect()
    {
        try
        {
            _simConnectClient.ConnectAsync().Wait();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }


    /// <summary>
    /// Unused, as this is a flight simulator
    /// </summary>
    /// <param name="localCar"></param>
    /// <param name="sessionData"></param>
    /// <param name="gameData"></param>
    [Obsolete("Don't use as this data provider is a flight simulator and not a racing game.")]
    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData) { }

    [Obsolete]
    public override List<string> GetCarClasses() => [];

    [Obsolete]
    public override bool HasTelemetry() => false;
}
