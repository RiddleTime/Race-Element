using ACCManager.Broadcast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Session
{
    public class RaceSessionState
    {
        public static bool IsPreSession(bool globalRed, SessionPhase phase)
        {
            return globalRed && phase == SessionPhase.PreSession;
        }
    }
}
