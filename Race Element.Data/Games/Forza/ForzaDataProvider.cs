using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.Forza
{
    public class ForzaDataProvider : AbstractSimDataProvider
    {
        public override List<string> GetCarClasses() => [];

        public override bool HasTelemetry() => false;

        public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {

        }

        internal override int PollingRate() => 60;

        internal override void Start()
        {
        }

        internal override void Stop()
        {
        }
    }
}
