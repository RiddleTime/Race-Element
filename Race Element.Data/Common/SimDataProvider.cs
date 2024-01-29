using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsaCompetizione;
using RaceElement.Data.Games;
using RaceElement.Data.Games.AssettoCorsa;

namespace RaceElement.Data.Common
{
    public static class SimDataProvider
    {
        private static LocalCarData _localCarData = new();
        public static LocalCarData LocalCar { get => _localCarData; }

        private static SessionData _session = new();
        public static SessionData Session { get => _session; }

        public static void Update(bool clear = false)
        {
            if (clear) Clear();

            switch (GameManager.CurrentGame)
            {
                case Game.AssettoCorsa1:
                    {
                        AssettoCorsa1DataProvider.Update(ref _localCarData, ref _session);
                        break;
                    }
                case Game.AssettoCorsaCompetizione:
                    {
                        AssettoCorsaCompetizioneDataProvider.Update(ref _localCarData, ref _session);
                        break;
                    }
                default: { break; }
            }
        }

        private static void Clear()
        {
            _localCarData = new LocalCarData();
            _session = new SessionData();
        }
    }
}
