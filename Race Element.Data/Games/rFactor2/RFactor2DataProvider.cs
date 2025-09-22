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

        private bool _initialized = false;

        public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {
            if (!GameManager.IsGameRunning) return;

            if (!_initialized && !Initialize()) return;

            _telemetryBuffer.GetMappedDataUnsynchronized(ref _telemetry);
            _scoringBuffer.GetMappedDataUnsynchronized(ref _scoring);

            if (_telemetry.mNumVehicles == 0) return;

            int localVehicleIndex = GetPlayerVehicleIndex();
            if (localVehicleIndex == -1) return;
            RF2VehicleTelemetry localVehicle = _telemetry.mVehicles[localVehicleIndex];



            foreach (var vehicle in _telemetry.mVehicles)
            {
                //Debug.WriteLine($"{vehicle.mID}");
            }


            localCar.Inputs.Throttle = (float)localVehicle.mUnfilteredThrottle;
            localCar.Inputs.Brake = (float)localVehicle.mUnfilteredBrake;
            localCar.Inputs.Clutch = (float)localVehicle.mUnfilteredClutch;
            localCar.Inputs.Steering = (float)localVehicle.mUnfilteredSteering;
            localCar.Inputs.MaxSteeringAngle = localVehicle.mPhysicalSteeringWheelRange;


    

            localCar.Engine.IsIgnitionOn = localVehicle.mIgnitionStarter == 1;
            localCar.Engine.MaxRpm = (int)localVehicle.mEngineMaxRPM;
            localCar.Engine.Rpm = (int)localVehicle.mEngineRPM;
            localCar.Engine.IsRunning = localCar.Engine.Rpm > 0;

            var speedMetersPerSecond = Math.Sqrt((localVehicle.mLocalVel.x * localVehicle.mLocalVel.x)
                + (localVehicle.mLocalVel.y * localVehicle.mLocalVel.y)
                + (localVehicle.mLocalVel.z * localVehicle.mLocalVel.z));

            localCar.Physics.Velocity = (float)(speedMetersPerSecond * 3.6f);

            localCar.Tyres.SlipRatio = [
                CalculateSlipRatio((float)localVehicle.mWheels[0].mLongitudinalGroundVel * 3.6f, localCar.Physics.Velocity),
                CalculateSlipRatio((float)localVehicle.mWheels[1].mLongitudinalGroundVel * 3.6f, localCar.Physics.Velocity),
                CalculateSlipRatio((float)localVehicle.mWheels[2].mLongitudinalGroundVel * 3.6f, localCar.Physics.Velocity),
                CalculateSlipRatio((float)localVehicle.mWheels[3].mLongitudinalGroundVel * 3.6f, localCar.Physics.Velocity),
            ];
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

        private static float CalculateSlipRatio(float tyreVelocity, float carVelocity)
        {
            if (tyreVelocity < 0) tyreVelocity *= -1;
            if (carVelocity < 1) return 0;
            float ratio = (tyreVelocity - carVelocity) / tyreVelocity * 10;

            if (ratio < 0) ratio *= -1;

            return ratio;
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
