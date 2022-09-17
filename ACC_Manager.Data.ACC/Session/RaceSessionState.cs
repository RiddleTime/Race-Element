using ACCManager.Broadcast;

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
