using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Data.ACC.Database.SessionData
{
    internal class DbRaceSession
    {


        public Guid Guid { get; set; }
        public DateTime Start { get; set; }
        public AcSessionType SessionType { get; set; }
        public int SessionIndex { get; set; }
    }
}
