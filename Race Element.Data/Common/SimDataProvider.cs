using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsaCompetizione;
using RaceElement.Data.Games;
using RaceElement.Data.Games.AssettoCorsa;
using RaceElement.Data.Games.iRacing;

namespace RaceElement.Data.Common
{
    public static class SimDataProvider
    {
        private static LocalCarData _localCarData = new();
        public static LocalCarData LocalCar { get => _localCarData; }

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
                        AssettoCorsa1DataProvider.Update(ref _localCarData, ref _session, ref _gameData);
                        break;
                    }
                case Game.AssettoCorsaCompetizione:
                    {
                        AssettoCorsaCompetizioneDataProvider.Update(ref _localCarData, ref _session, ref _gameData);
                        break;
                    }
                case Game.iRacing:
                    {
                        IRacingDataProvider.Update(ref _localCarData, ref _session, ref _gameData);
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
    }
}
