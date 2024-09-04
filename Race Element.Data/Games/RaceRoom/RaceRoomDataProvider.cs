using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.RaceRoom.DataMapper;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.RaceRoom
{
    internal sealed class RaceRoomDataProvider : AbstractSimDataProvider
    {
        public override List<string> GetCarClasses()
        {
            return [];
        }

        public override Color GetColorForCarClass(string carClass)
        {
            return Color.White;
        }

        public override bool HasTelemetry()
        {
            return false;
        }

        public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
        {
            Shared sharedMemory = R3eSharedMemory.ReadSharedMemory();
            RR3LocalCarMapper.AddR3SharedMemory(sharedMemory, localCar);
        }

        internal override void Stop()
        {

        }
    }
}
