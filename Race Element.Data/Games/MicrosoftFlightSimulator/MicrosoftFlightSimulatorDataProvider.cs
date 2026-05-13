using RaceElement.Core.Jobs.Loop;
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
    private SlowDataJob _slowDataJob = null;
    private ControlsDataJob _controlsDataJob = null;

    internal sealed override int PollingRate() => 100;
    internal sealed override void Start()
    {
        _simConnectClient = new("Race Element")
        {
            MaxReconnectAttempts = 1,
        };
        _simConnectClient.ConnectionStatusChanged += SimConnectClient_ConnectionStatusChanged;

        _slowDataJob = new(_simConnectClient) { IntervalMillis = 100 };
        _slowDataJob.Run();
        _controlsDataJob = new(_simConnectClient) { IntervalMillis = (int)(1000d / 100d) };
        _controlsDataJob.Run();
    }

    private void SimConnectClient_ConnectionStatusChanged(object? sender, SimConnect.NET.Events.ConnectionStatusChangedEventArgs e)
    {
        if (e.IsConnected) Debug.WriteLine("Connected with SimConnect!");
        if (e.IsDisconnected) Debug.WriteLine("Disconnected from Simconnect!");
    }

    internal sealed override void Stop()
    {
        _slowDataJob?.CancelJoin();
        _controlsDataJob?.CancelJoin();

        _simConnectClient?.DisconnectAsync().Wait();
        if (_simConnectClient != null) _simConnectClient.ConnectionStatusChanged -= SimConnectClient_ConnectionStatusChanged;
        _simConnectClient?.Dispose();
    }


    private double _lastAnimationTime = 0;
    private int _pauseGameBuffer = 0;
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
            double lastAnimateTime = _simConnectClient.SimVars.GetAsync<double>("ANIMATION DELTA TIME", "seconds", 0).GetAwaiter().GetResult();
            if (lastAnimateTime != _lastAnimationTime)
            {
                SimDataProvider.GameData.IsGamePaused = false;
                _pauseGameBuffer = 0;
            }
            else
            {
                if (_pauseGameBuffer > 150)
                {
                    SimDataProvider.GameData.IsGamePaused = true;
                    localPlane = new();
                }
                else _pauseGameBuffer++;
            }
            _lastAnimationTime = lastAnimateTime;

            if (SimDataProvider.GameData.IsGamePaused) return;

            MapPhysicsData(ref localPlane);

            //double collectivePosition = _simConnectClient.SimVars.GetAsync<double>("COLLECTIVE POSITION", "percent over 100", 0).GetAwaiter().GetResult();
            //localPlane.Helicopter.CollectivePosition = collectivePosition;
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

    private void MapPhysicsData(ref LocalPlaneData localPlane)
    {
        if (_simConnectClient == null) return;

        Task<AircraftPosition> position = _simConnectClient.Aircraft.GetPositionAsync();
        Task<AircraftMotion> motion = _simConnectClient.Aircraft.GetMotionAsync();

        Task.WhenAll(position, motion).ConfigureAwait(false).GetAwaiter().GetResult();

        localPlane.Physics.IndicatedAirSpeed = motion.Result.IndicatedAirspeed;
        localPlane.Physics.GroundSpeed = motion.Result.GroundSpeed;
        localPlane.Physics.VerticalSpeed = motion.Result.VerticalSpeed;

        localPlane.Physics.Latitude = position.Result.Latitude;
        localPlane.Physics.Longitude = position.Result.Longitude;
        localPlane.Physics.Orientation = GetForwardVector(position.Result.TrueHeading, position.Result.Pitch, position.Result.Bank);
        localPlane.Physics.AltitudeSea = position.Result.Altitude;
        localPlane.Physics.AltitudeGround = position.Result.AltitudeAboveGround;
    }

    private sealed class ControlsDataJob(SimConnectClient simConnectClient) : AbstractLoopJob
    {
        private const double PercentScalar16k = 16384.0;
        public sealed override void RunAction()
        {
            if (simConnectClient == null || !simConnectClient.IsConnected || SimDataProvider.GameData.IsGamePaused)
                return;

            try
            {
                MapControlsData();

            }
            catch (Exception) { }
        }

        private void MapControlsData()
        {
            Task<double> aileronPosition = simConnectClient.SimVars.GetAsync<double>("AILERON POSITION", "percent scaler 16k");
            Task<double> elevatorPosition = simConnectClient.SimVars.GetAsync<double>("ELEVATOR POSITION", "percent scaler 16k");
            Task<double> rudderPosition = simConnectClient.SimVars.GetAsync<double>("RUDDER POSITION", "percent scaler 16k");
            Task.WhenAll(aileronPosition, elevatorPosition, rudderPosition).ConfigureAwait(false).GetAwaiter().GetResult();

            SimDataProvider.LocalPlane.Controls = new()
            {
                AileronPosition = aileronPosition.Result / PercentScalar16k,
                ElevatorPosition = elevatorPosition.Result / PercentScalar16k,
                RudderPosition = rudderPosition.Result / PercentScalar16k,
            };
        }
    }

    private sealed class SlowDataJob(SimConnectClient simConnectClient) : AbstractLoopJob
    {
        public sealed override void RunAction()
        {
            if (simConnectClient == null || !simConnectClient.IsConnected || SimDataProvider.GameData.IsGamePaused)
                return;

            try
            {
                MapAtcData();
                MapFlightModelData();
            }
            catch (Exception) { }
        }

        private void MapAtcData()
        {
            if (simConnectClient == null) return;

            Task<string> atcId = simConnectClient.SimVars.GetAsync<string>("ATC ID");
            Task<string> atcModel = simConnectClient.SimVars.GetAsync<string>("ATC MODEL");
            Task<string> atcType = simConnectClient.SimVars.GetAsync<string>("ATC TYPE");
            Task<string> airportName = simConnectClient.SimVars.GetAsync<string>("ATC RUNWAY AIRPORT NAME");
            Task<double> suggestedRunwayLandingFeet = simConnectClient.SimVars.GetAsync<double>($"ATC SUGGESTED MIN RWY LANDING", "feet");
            Task<double> suggestedRunwayTakeOffFeet = simConnectClient.SimVars.GetAsync<double>($"ATC SUGGESTED MIN RWY TAKEOFF", "feet");

            Task.WhenAll(atcId, atcModel, atcType, atcType, airportName, suggestedRunwayLandingFeet, suggestedRunwayTakeOffFeet).ConfigureAwait(false).GetAwaiter().GetResult();

            SimDataProvider.LocalPlane.ATC = new()
            {
                Identifier = atcId?.Result ?? "",
                Model = atcModel?.Result ?? "",
                Type = atcType?.Result ?? "",
                SuggestedMinRunwayLandingLength = suggestedRunwayLandingFeet?.Result ?? 0,
                SuggestedMinRunwayTakeoffLength = suggestedRunwayTakeOffFeet?.Result ?? 0,
                AirportName = airportName?.Result ?? "",
            };
        }

        private void MapFlightModelData()
        {
            if (simConnectClient == null) return;

            Task<double> currentGForce = simConnectClient.SimVars.GetAsync<double>("G FORCE", "GForce");
            Task<double> minGForceAttained = simConnectClient.SimVars.GetAsync<double>("MIN G FORCE", "GForce");
            Task<double> maxGForceAttained = simConnectClient.SimVars.GetAsync<double>("MAX G FORCE", "GForce");
            Task<double> designTakeOffSpeed = simConnectClient.SimVars.GetAsync<double>("DESIGN TAKEOFF SPEED", "Knots");
            Task<double> designCruiseAltitude = simConnectClient.SimVars.GetAsync<double>("DESIGN CRUISE ALT", "Feet");
            Task<double> designClimbSpeed = simConnectClient.SimVars.GetAsync<double>("DESIGN SPEED CLIMB", "Feet per second");
            Task.WhenAll(currentGForce, minGForceAttained, maxGForceAttained, designTakeOffSpeed, designCruiseAltitude, designClimbSpeed).ConfigureAwait(false).GetAwaiter().GetResult();
            SimDataProvider.LocalPlane.FlightModel.General = new()
            {
                CurrentGForce = currentGForce?.Result ?? 0,
                MinGForceAttained = minGForceAttained?.Result ?? 0,
                MaxGForceAttained = maxGForceAttained?.Result ?? 0,
                DesignTakeoffSpeed = designTakeOffSpeed?.Result ?? 0,
                DesignCruiseAltitude = designCruiseAltitude?.Result ?? 0,
                DesignClimbSpeed = designClimbSpeed?.Result ?? 0,
            };

            Task<double> emptyWeight = simConnectClient.SimVars.GetAsync<double>("EMPTY WEIGHT", "Pounds");
            Task<double> totalWeight = simConnectClient.SimVars.GetAsync<double>("TOTAL WEIGHT", "Pounds");
            Task<double> maxGrossWeight = simConnectClient.SimVars.GetAsync<double>("MAX GROSS WEIGHT", "Pounds");
            Task.WhenAll(emptyWeight, totalWeight, maxGrossWeight).ConfigureAwait(false).GetAwaiter().GetResult();
            SimDataProvider.LocalPlane.FlightModel.Weight = new()
            {
                EmptyWeight = emptyWeight?.Result ?? 0,
                TotalWeight = totalWeight?.Result ?? 0,
                MaxGrossWeight = maxGrossWeight?.Result ?? 0,
            };
        }

    }
    private void MapEngineData(ref LocalPlaneData localPlane)
    {
        uint engineCount = (uint)_simConnectClient.SimVars.GetAsync<int>("NUMBER OF ENGINES", "number", 0).GetAwaiter().GetResult();
        if (localPlane.General.EngineCount != engineCount) localPlane.Engines = [];
        localPlane.General.EngineCount = (uint)engineCount;

        if (engineCount > 0)
        {
            int engineTypeNumber = _simConnectClient.SimVars.GetAsync<int>("ENGINE TYPE", "enum").GetAwaiter().GetResult();
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
                AircraftEngine engine = _simConnectClient.Aircraft.GetEngineAsync(i + 1).GetAwaiter().GetResult();
                double maxRatedEngineRPM = _simConnectClient.SimVars.GetAsync<double>($"MAX RATED ENGINE RPM:{i + 1}", "rpm", 0).GetAwaiter().GetResult();
                double maxReachedEngineRPM = _simConnectClient.SimVars.GetAsync<double>($"GENERAL ENG MAX REACHED RPM:{i + 1}", "rpm", 0).GetAwaiter().GetResult();

                Common.SimulatorData.LocalPlane.EngineData newEngineData = new()
                {
                    EngineIndex = (uint)i,
                    IsRunning = engine.IsRunning,
                    Rpm = engine.Rpm,
                    MaxRatedEngineRpm = maxRatedEngineRPM,
                    MaxReachedEngineRpm = maxReachedEngineRPM,
                    ThrottlePosition = engine.ThrottlePosition,
                    EngineType = engineType,
                };

                Common.SimulatorData.LocalPlane.EngineData? existingEngineData = localPlane.Engines.FirstOrDefault(x => x.EngineIndex == i);
                if (existingEngineData == null)
                    localPlane.Engines.Add(newEngineData);
                else
                    localPlane.Engines[i] = newEngineData;
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
