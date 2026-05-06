using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Common.SimulatorData.LocalPlane;
using SimConnect.NET;
using SimConnect.NET.Aircraft;
using System.Diagnostics;
using System.Numerics;

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
            localPlane = new();
            Connect();
            Thread.Sleep(5000);
            return;
        }

        try
        {
            AircraftMotion motion = _simConnectClient.Aircraft.GetMotionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            AircraftPosition position = _simConnectClient.Aircraft.GetPositionAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            localPlane.Physics.IndicatedAirSpeed = motion.IndicatedAirspeed;
            localPlane.Physics.GroundSpeed = motion.GroundSpeed;
            localPlane.Physics.VerticalSpeed = motion.VerticalSpeed;

            localPlane.Physics.Latitude = position.Latitude;
            localPlane.Physics.Longitude = position.Longitude;
            localPlane.Physics.Orientation = GetForwardVector(position.TrueHeading, position.Pitch, position.Bank);
        }
        catch (Exception)
        {
            return;
        }
    }

    /// <summary>
    /// Z-forward, Y-up, X-right - Standard right-handed system.
    /// </summary>
    /// <param name="headingDegrees"></param>
    /// <param name="pitchDegrees"></param>
    /// <param name="bankDegrees"></param>
    /// <returns></returns>
    private static Vector3 GetForwardVector(double headingDegrees, double pitchDegrees, double bankDegrees)
    {
        // Convert to radians
        double headingRad = headingDegrees * Math.PI / 180.0;
        double pitchRad = pitchDegrees * Math.PI / 180.0;
        double bankRad = bankDegrees * Math.PI / 180.0;

        // Trigonometric values
        double cosH = Math.Cos(headingRad);
        double sinH = Math.Sin(headingRad);
        double cosP = Math.Cos(pitchRad);
        double sinP = Math.Sin(pitchRad);
        double cosB = Math.Cos(bankRad);
        double sinB = Math.Sin(bankRad);

        // Calculate forward vector (Z-forward, Y-up, X-right - standard right-handed system)
        Vector3 forward = new()
        {
            X = (float)(cosP * sinH * cosB + sinP * sinB),          // East component
            Y = (float)(-sinP * cosB + cosP * sinH * sinB),         // Up component  (negative because pitch up = positive climb)
            Z = (float)(cosP * cosH * cosB - sinP * sinH * sinB)    // North component
        };

        forward = Vector3.Normalize(forward);

        return forward;
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
