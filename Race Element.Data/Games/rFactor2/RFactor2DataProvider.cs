using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.rFactor2.SharedMemory;
using RaceElement.Data.Games.rFactor2.SharedMemory.rF2SharedMemory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.Games.rFactor2.SharedMemory.SharedMemoryStructs;

namespace RaceElement.Data.Games.rFactor2
{
    class RFactor2DataProvider : AbstractSimDataProvider
    {
        private MappedBuffer<RF2Telemetry> _telemetryBuffer = new(Constants.MM_TELEMETRY_FILE_NAME, true, true);
        private MappedBuffer<RF2Scoring> _scoringBuffer = new(Constants.MM_SCORING_FILE_NAME, true, true);
        private RF2Telemetry _telemetry = new();
        private RF2Scoring _scoring = new();
        private uint _lastUpdate = 0;
        private uint _sameFrames = 0;

        private bool _initialized = false;

        public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {
            if (!GameManager.IsGameRunning) return;

            if (!_initialized && !Initialize()) return;

            _telemetryBuffer.GetMappedData(ref _telemetry);
            _scoringBuffer.GetMappedData(ref _scoring);

            if (_lastUpdate == _telemetry.mVersionUpdateBegin)
            {
                _sameFrames++;

                if (_sameFrames > 100)
                {
                    gameData.IsGamePaused = true;
                    return;
                }
            }
            else
            {
                _sameFrames = 0;
                gameData.IsGamePaused = false;
            }
            _lastUpdate = _telemetry.mVersionUpdateBegin;

            if (_telemetry.mNumVehicles == 0) return;

            int localVehicleIndex = GetPlayerVehicleIndex();
            if (localVehicleIndex == -1) return;
            RF2VehicleTelemetry localVehicle = _telemetry.mVehicles[localVehicleIndex];

            localCar.Inputs.Throttle = (float)localVehicle.mUnfilteredThrottle;
            localCar.Inputs.Brake = (float)localVehicle.mUnfilteredBrake;
            localCar.Inputs.Clutch = (float)localVehicle.mUnfilteredClutch;
            localCar.Inputs.Steering = (float)localVehicle.mUnfilteredSteering;
            localCar.Inputs.MaxSteeringAngle = localVehicle.mPhysicalSteeringWheelRange;

            localCar.Electronics.TractionControlActivation = localVehicle.mFilteredThrottle < localVehicle.mUnfilteredThrottle ? 1 : 0;
            localCar.Electronics.AbsActivation = localVehicle.mFilteredBrake < localVehicle.mUnfilteredBrake ? 1 : 0;


            localCar.Engine.IsIgnitionOn = localVehicle.mIgnitionStarter == 1;
            localCar.Engine.MaxRpm = (int)localVehicle.mEngineMaxRPM;
            localCar.Engine.Rpm = (int)localVehicle.mEngineRPM;
            localCar.Engine.IsRunning = localCar.Engine.Rpm > 0;

            var speedMetersPerSecond = Math.Sqrt((localVehicle.mLocalVel.x * localVehicle.mLocalVel.x)
                + (localVehicle.mLocalVel.y * localVehicle.mLocalVel.y)
                + (localVehicle.mLocalVel.z * localVehicle.mLocalVel.z));
            localCar.Physics.Velocity = (float)(speedMetersPerSecond * 3.6f);
            localCar.Physics.Acceleration = new((float)localVehicle.mLocalAccel.x / 9.80665f, (float)localVehicle.mLocalAccel.y / 9.80665f, (float)-localVehicle.mLocalAccel.z / 9.80665f);


            localCar.Tyres.SlipRatio = [
                CalculateWheelSlipRatio(localVehicle.mWheels[0]),
                CalculateWheelSlipRatio(localVehicle.mWheels[1]),
                CalculateWheelSlipRatio(localVehicle.mWheels[2]),
                CalculateWheelSlipRatio(localVehicle.mWheels[3]),
            ];
        }

        public static float CalculateWheelSlipRatio(RF2Wheel wheel)
        {
            // Convert tire radius from centimeters to meters
            double tireRadius = wheel.mStaticUndeflectedRadius / 100.0;

            // Calculate wheel tangential velocity (Vw = ω * r)
            double wheelVelocity = wheel.mRotation * tireRadius;

            // Ground velocity (Vg)
            double groundVelocity = wheel.mLongitudinalGroundVel;

            // Avoid division by zero using a small epsilon
            double epsilon = 0.1;
            double denominator = Math.Max(Math.Abs(groundVelocity), epsilon);

            // Calculate slip ratio: (Vw - Vg) / max(|Vg|, ε)
            float slipRatio = (float)((wheelVelocity - groundVelocity) / denominator);

            return slipRatio;
        }

        private int GetPlayerVehicleIndex()
        {
            var idsToTelIndices = new Dictionary<long, int>();
            for (int i = 0; i < _telemetry.mNumVehicles; ++i)
            {
                if (!idsToTelIndices.ContainsKey(_telemetry.mVehicles[i].mID))
                    idsToTelIndices.Add(_telemetry.mVehicles[i].mID, i);
            }

            var playerVehScoring = GetPlayerScoring(ref this._scoring);

            var scoringPlrId = playerVehScoring.mID;
            if (idsToTelIndices.ContainsKey(scoringPlrId))
                return idsToTelIndices[scoringPlrId];

            return -1;
        }

        private static RF2VehicleScoring GetPlayerScoring(ref RF2Scoring scoring)
        {
            var playerVehScoring = new RF2VehicleScoring();
            for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
            {
                var vehicle = scoring.mVehicles[i];
                switch ((Constants.RF2Control)vehicle.mControl)
                {
                    case Constants.RF2Control.AI:
                    case Constants.RF2Control.Player:
                    case Constants.RF2Control.Remote:
                        if (vehicle.mIsPlayer == 1)
                            playerVehScoring = vehicle;

                        break;

                    default:
                        continue;
                }

                if (playerVehScoring.mIsPlayer == 1)
                    break;
            }

            return playerVehScoring;
        }

        internal override int PollingRate() => 200;

        internal override void Start()
        {
            Initialize();
        }

        private bool Initialize()
        {
            if (_telemetryBuffer.Connect() && _scoringBuffer.Connect())
            {
                _telemetry = new();
                _scoring = new();
                _initialized = true;
                return true;
            }

            return false;
        }

        internal override void Stop()
        {
            _telemetryBuffer.Disconnect();
            _scoringBuffer.Disconnect();
            _initialized = false;
        }

        public override List<string> GetCarClasses() => [];

        public override bool HasTelemetry() => false;
    }
}
