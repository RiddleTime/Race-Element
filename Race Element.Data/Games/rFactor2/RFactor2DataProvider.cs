using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.rFactor2.DataMapper;
using RaceElement.Data.Games.rFactor2.SharedMemory;
using RaceElement.Data.Games.rFactor2.SharedMemory.rF2SharedMemory;

using static RaceElement.Data.Games.rFactor2.SharedMemory.SharedMemoryStructs;

namespace RaceElement.Data.Games.rFactor2;
sealed class RFactor2DataProvider : AbstractSimDataProvider
{
    private readonly MappedBuffer<RF2Telemetry> _telemetryBuffer = new(Constants.MM_TELEMETRY_FILE_NAME, true, true);
    private readonly MappedBuffer<RF2Scoring> _scoringBuffer = new(Constants.MM_SCORING_FILE_NAME, true, true);
    private RF2Telemetry _telemetry = new();
    private RF2Scoring _scoring = new();
    private uint _lastFrameId = 0;
    private uint _sameFrames = 0;

    private bool _initialized = false;

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        if (!GameManager.IsGameRunning) return;

        if (!_initialized && !Initialize()) return;

        _telemetryBuffer.GetMappedData(ref _telemetry);
        _scoringBuffer.GetMappedData(ref _scoring);

        if (_lastFrameId == _telemetry.mVersionUpdateBegin)
        {
            _sameFrames++;

            if (_sameFrames > 50)
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
        _lastFrameId = _telemetry.mVersionUpdateBegin;

        if (_telemetry.mNumVehicles == 0) return;

        int localVehicleIndex = GetPlayerVehicleIndex();
        if (localVehicleIndex == -1) return;
        RF2VehicleTelemetry localVehicle = _telemetry.mVehicles[localVehicleIndex];
        LocalCarMapper.MapLocalCar(ref localCar, localVehicle, GetPlayerScoring(ref _scoring));
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

    internal override void Start() => Initialize();

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
