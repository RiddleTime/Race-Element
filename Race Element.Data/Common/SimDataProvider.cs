using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsaCompetizione;
using RaceElement.Data.Games;
using RaceElement.Data.Games.AssettoCorsa;
using RaceElement.Data.Games.iRacing;
using RaceElement.Data.Games.RaceRoom;

namespace RaceElement.Data.Common
{
    public static class SimDataProvider
    {
        public static AbstractSimDataProvider? Instance { get; internal set; }

        private static LocalCarData _localCarData = new();
        public static LocalCarData LocalCar { get => _localCarData; }


        private static LocalCarEvents _localCarEvents = new();
        public static LocalCarEvents LocalCarEvents { get => _localCarEvents; }


        private static SessionData _session = new();
        public static SessionData Session { get => _session; }

        private static GameData _gameData = new();
        public static GameData GameData { get => _gameData; }

        public static void Update(bool clear = false)
        {
            if (clear) Clear();

            switch (GameManager.CurrentGame)
            {
                case Game.AssettoCorsa1:
                    {
                        Instance ??= new AssettoCorsa1DataProvider();
                        Instance.Start();
                        Instance.Update(ref _localCarData, ref _session, ref _gameData);
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
                        Instance.Start();
                        Instance.Update(ref _localCarData, ref _session, ref _gameData);
                        break;
                    }
                case Game.RaceRoom:
                    {
                        Instance ??= new RaceRoomDataProvider();
                        Instance.Start();
                        Instance.Update(ref _localCarData, ref _session, ref _gameData);
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
            switch (GameManager.CurrentGame)
            {
                case Game.AssettoCorsa1:
                    {
                        Instance?.Stop();
                        break;
                    }
                case Game.AssettoCorsaCompetizione:
                    {
                        // TODO
                        break;
                    }
                case Game.iRacing:
                    {
                        Instance?.Stop();
                        break;
                    }
                case Game.RaceRoom:
                    {
                        Instance?.Stop();
                        break;
                    }
                default: { break; }
            }

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
}
