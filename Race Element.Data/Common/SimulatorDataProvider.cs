using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsaCompetizione;
using RaceElement.Data.Games;

namespace RaceElement.Data.Common
{
    public static class SimulatorDataProvider
    {
        private static LocalCarData _localCarData = new LocalCarData();
        public static LocalCarData LocalCar { get => _localCarData; }

        private static SessionData _session = new SessionData();
        public static SessionData Session { get => _session; }

        public static void Update(bool clear = false)
        {
            if (clear)
                Clear();

            Game game = Game.AssettoCorsaCompetizione;

            switch (game)
            {
                case Game.AssettoCorsa1:
                    {
                        break;
                    }
                case Game.AssettoCorsaCompetizione:
                    {
                        AssettoCorsaCompetizioneDataProvider.Update(ref _localCarData, ref _session); break;
                    }
            }
        }

        private static void Clear()
        {
            _localCarData = new LocalCarData();
            _session = new SessionData();
        }


    }
}
