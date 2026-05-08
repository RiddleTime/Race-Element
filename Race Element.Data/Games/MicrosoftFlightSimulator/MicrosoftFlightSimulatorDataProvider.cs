using RaceElement.Data.Common;
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
    private SimConnectClient? _simConnectClient;

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
        _simConnectClient?.DisconnectAsync().Wait();
        if (_simConnectClient != null) _simConnectClient.ConnectionStatusChanged -= SimConnectClient_ConnectionStatusChanged;
        _simConnectClient?.Dispose();
    }


    private double _lastAnimationTime = 0;

    public void UpdateFlightData(ref LocalPlaneData localPlane)
    {
        if (_simConnectClient == null)
            return;

        if (!_simConnectClient.IsConnected)
        {
            SimDataProvider.GameData.IsRunning = false;
            localPlane = new();
            Connect();
            Thread.Sleep(5000);
            return;
        }
        else
        {
            SimDataProvider.GameData.IsRunning = true;
        }

        try
        {
            double lastAnimateTime = _simConnectClient.SimVars.GetAsync<double>("ANIMATION DELTA TIME", "seconds", 0).ConfigureAwait(false).GetAwaiter().GetResult();
            if (lastAnimateTime != _lastAnimationTime)
                SimDataProvider.GameData.IsGamePaused = false;
            else
                SimDataProvider.GameData.IsGamePaused = true;
            _lastAnimationTime = lastAnimateTime;


            MapEngineData(ref localPlane);

            MapAtcData(ref localPlane);

            AircraftMotion motion = _simConnectClient.Aircraft.GetMotionAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            localPlane.Physics.IndicatedAirSpeed = motion.IndicatedAirspeed;
            localPlane.Physics.GroundSpeed = motion.GroundSpeed;
            localPlane.Physics.VerticalSpeed = motion.VerticalSpeed;

            AircraftPosition position = _simConnectClient.Aircraft.GetPositionAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            localPlane.Physics.Latitude = position.Latitude;
            localPlane.Physics.Longitude = position.Longitude;
            localPlane.Physics.Orientation = GetForwardVector(position.TrueHeading, position.Pitch, position.Bank);
            localPlane.Physics.AltitudeSea = position.Altitude;
            localPlane.Physics.AltitudeGround = position.AltitudeAboveGround;

            double collectivePosition = _simConnectClient.SimVars.GetAsync<double>("COLLECTIVE POSITION", "percent over 100", 0).ConfigureAwait(false).GetAwaiter().GetResult();
            localPlane.Helicopter.CollectivePosition = collectivePosition;
        }
        catch (Exception e)
        {
            if (e is SimConnectException)
            {
                Debug.WriteLine("Received SimConnectException, disconnected client");
                _simConnectClient.DisconnectAsync().Wait();
            }
            return;
        }
    }

    private void MapAtcData(ref LocalPlaneData localPlane)
    {
        try
        {
            string? atcId = _simConnectClient?.SimVars.GetAsync<string>("ATC ID").ConfigureAwait(false).GetAwaiter().GetResult();
            string? atcModel = _simConnectClient?.SimVars.GetAsync<string>("ATC MODEL").ConfigureAwait(false).GetAwaiter().GetResult();
            string? atcType = _simConnectClient?.SimVars.GetAsync<string>("ATC TYPE").ConfigureAwait(false).GetAwaiter().GetResult();


            string? airportName = _simConnectClient?.SimVars.GetAsync<string>("ATC RUNWAY AIRPORT NAME").ConfigureAwait(false).GetAwaiter().GetResult();

            double suggestedRunwayLandingFeet = _simConnectClient.SimVars.GetAsync<double>($"ATC SUGGESTED MIN RWY LANDING", "feet", 0).ConfigureAwait(false).GetAwaiter().GetResult();
            double suggestedRunwayTakeOffFeet = _simConnectClient.SimVars.GetAsync<double>($"ATC SUGGESTED MIN RWY TAKEOFF", "feet", 0).ConfigureAwait(false).GetAwaiter().GetResult();

            localPlane.ATC = new()
            {
                Identifier = atcId ?? "",
                Model = atcModel ?? "",
                Type = atcType ?? "",
                SuggestedMinimumRunwayLandingLength = suggestedRunwayLandingFeet,
                SuggestedMinimumRunwayTakeoffLength = suggestedRunwayTakeOffFeet,
                AirportName = airportName ?? "",
            };
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    private void MapEngineData(ref LocalPlaneData localPlane)
    {
        uint engineCount = (uint)_simConnectClient.SimVars.GetAsync<int>("NUMBER OF ENGINES", "number", 0).ConfigureAwait(false).GetAwaiter().GetResult();
        if (localPlane.General.EngineCount != engineCount) localPlane.Engines = [];
        localPlane.General.EngineCount = (uint)engineCount;

        if (engineCount > 0)
        {
            int engineTypeNumber = _simConnectClient.SimVars.GetAsync<int>("ENGINE TYPE", "enum").ConfigureAwait(false).GetAwaiter().GetResult();
            string engineType = engineTypeNumber switch
            {
                0 => "Piston",
                1 => "Jet",
                2 => "None",
                3 => "Helo[Bell] Turbine",
                4 => "Unsupported",
                5 => "Turboprop",
                6 => "Electric",
                _ => "",
            };

            for (int i = 0; i < engineCount; i++)
            {
                AircraftEngine engine = _simConnectClient.Aircraft.GetEngineAsync(i + 1).ConfigureAwait(false).GetAwaiter().GetResult();
                double maxRatedEngineRPM = _simConnectClient.SimVars.GetAsync<double>($"MAX RATED ENGINE RPM:{i + 1}", "rpm", 0).ConfigureAwait(false).GetAwaiter().GetResult();
                double maxReachedEngineRPM = _simConnectClient.SimVars.GetAsync<double>($"GENERAL ENG MAX REACHED RPM:{i + 1}", "rpm", 0).ConfigureAwait(false).GetAwaiter().GetResult();

                Common.SimulatorData.LocalPlane.EngineData engineData = new()
                {
                    EngineIndex = (uint)i,
                    IsRunning = engine.IsRunning,
                    Rpm = engine.Rpm,
                    MaxRatedEngineRpm = maxRatedEngineRPM,
                    MaxReachedEngineRpm = maxReachedEngineRPM,
                    ThrottlePosition = engine.ThrottlePosition,
                    EngineType = engineType,
                };

                var existingItem = localPlane.Engines.FirstOrDefault(x => x.EngineIndex == i);
                if (existingItem == null)
                    localPlane.Engines.Add(engineData);
                else
                    localPlane.Engines[i] = engineData;
            }
        }
    }

    /// <summary>
    /// Z-forward, Y-up, X-right - Standard right-handed system.
    /// </summary>
    /// <param name="headingDegrees"></param>
    /// <param name="pitchDegrees"></param>
    /// <param name="bankAngleDegrees"></param>
    /// <returns></returns>
    private static Vector3 GetForwardVector(double headingDegrees, double pitchDegrees, double bankAngleDegrees)
    {
        // Convert to radians
        double headingRad = headingDegrees * Math.PI / 180.0;
        double pitchRad = pitchDegrees * Math.PI / 180.0;
        double bankRad = bankAngleDegrees * Math.PI / 180.0;

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
            _simConnectClient?.ConnectAsync().Wait();
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
