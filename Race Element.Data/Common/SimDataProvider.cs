using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games;
using RaceElement.Data.Games.AssettoCorsa;
using RaceElement.Data.Games.iRacing;
using RaceElement.Data.Games.RaceRoom;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.Automobilista2;
using RaceElement.Data.Games.EuroTruckSimulator2;
using RaceElement.Data.Games.AssettoCorsaEvo;
using RaceElement.Data.Common.Graph;
using RaceElement.Graph;
using RaceElement.Data.Games.Forza;

namespace RaceElement.Data.Common;

public static class SimDataProvider
{
    public static AbstractSimDataProvider? Instance { get; internal set; }

    private static LocalCarData _localCarData = new();
    public static LocalCarData LocalCar { get => _localCarData; }

    internal static LocalCarEvents localCarEvents = new();
    public static LocalCarEvents LocalCarEvents { get => localCarEvents; }

    private static readonly LocalCarEventLoop _localCarEventLoop = new();

    private static GameData _gameData = new();
    public static GameData GameData { get => _gameData; }


    private static DataGraph _racingGraph = new();
    public static DataGraph RacingGraph { get => _racingGraph; }


    /// <summary>
    /// TODO, remove and replace with <see cref="RacingGraph"/>
    /// </summary>
    private static SessionData _session = new();
    /// <summary>
    /// TODO, remove and replace with <see cref="RacingGraph"/>
    /// </summary>
    public static SessionData Session { get => _session; }



    public static void Update(bool clear = false)
    {
        if (clear) Clear();

        switch (GameManager.CurrentGame)
        {
            case Game.AssettoCorsa1:
                {
                    Instance ??= new AssettoCorsa1DataProvider();
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }
            case Game.AssettoCorsaCompetizione:
                {
                    //  -- ACC is currently still running it's own data updater mechanisms, so this is for commented for the time being.
                    //  AssettoCorsaCompetizioneDataProvider.Update(ref _localCarData, ref _session, ref _gameData);
                    break;
                }
            case Game.iRacing:
                {
                    Instance ??= new IRacingDataProvider();
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }
            case Game.RaceRoom:
                {
                    Instance ??= new RaceRoomDataProvider();
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }
            case Game.Automobilista2:
                {
                    Instance ??= new Automobilista2DataProvider();
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }

            case Game.EuroTruckSimulator2:
                {
                    Instance ??= new SCS2DataProvider(Game.EuroTruckSimulator2);
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }

            case Game.AmericanTruckSimulator:
                {
                    Instance ??= new SCS2DataProvider(Game.AmericanTruckSimulator);
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }
            case Game.AssettoCorsaEvo:
                {
                    Instance ??= new AssettoCorsaEvoDataProvider();
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }
            case Game.ForzaHorizon5:
                {
                    Instance ??= new ForzaDataProvider(Game.ForzaHorizon5);
                    Instance.Update(ref _localCarData, ref _session, ref _gameData);
                    _localCarEventLoop.Run();
                    break;
                }
            default: { break; }
        }
    }

    internal static void Clear()
    {
        _localCarData = new LocalCarData();
        _session = new SessionData();
        _gameData = new GameData();
    }

    internal static void Stop()
    {
        _localCarEventLoop?.CancelJoin();
        Instance?.Stop();
        Instance = null;
    }

    public static bool HasTelemetry()
    {
        return (Instance != null && Instance.HasTelemetry());
    }

    public static event EventHandler<RaceSessionType> OnSessionTypeChanged;
    public static event EventHandler<SessionPhase> OnSessionPhaseChanged;
    public static event EventHandler<Status> OnStatusChanged;

    internal static void CallSessionTypeChanged(AbstractSimDataProvider simDataProvider, RaceSessionType sessionType)
    {
        OnSessionTypeChanged?.Invoke(simDataProvider, SessionData.Instance.SessionType);
    }

    internal static void CallSessionPhaseChanged(AbstractSimDataProvider simDataProvider, SessionPhase sessionPhase)
    {
        OnSessionPhaseChanged?.Invoke(simDataProvider, SessionData.Instance.Phase);
    }
}
