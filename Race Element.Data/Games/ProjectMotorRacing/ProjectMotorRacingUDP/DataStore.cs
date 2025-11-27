namespace RaceElement.Data.Games.ProjectMotorRacing.ProjectMotorRacingUDP;

public sealed class DataStore
{
    public void WriteRaceInfo(UDPRaceInfo raceInfo)
    {
        lock (_lock)
        {
            m_raceInfo = raceInfo;
            if (m_raceInfo.m_numParticipants == 0)
            {
                m_raceStates.Clear();
                m_statusUpdates.Clear();
                m_telemetry.Clear();
            }

            UpdateRaceInfoString();
        }
    }

    public void WriteRaceState(UDPParticipantRaceState raceState)
    {
        lock (_lock)
        {
            bool stateFound = false;
            for (int i = 0; i < m_raceStates.Count; i++)
            {
                if (m_raceStates[i].m_vehicleId == raceState.m_vehicleId)
                {
                    m_raceStates[i] = raceState;
                    stateFound = true;
                    break;
                }
            }

            if (!stateFound)
            {
                m_raceStates.Add(raceState);
            }

            UpdateStatus();
        }
    }

    public void WriteTelemetry(UDPVehicleTelemetry telemetry)
    {
        lock (_lock)
        {
            for (int i = 0; i < m_telemetry.Count; i++)
            {
                if (m_telemetry[i].m_vehicleId == telemetry.m_vehicleId)
                {
                    m_telemetry[i] = telemetry;
                    return;
                }
            }

            m_telemetry.Add(telemetry);
        }
    }

    public UDPVehicleTelemetry GetTelemetryForVehicle(int vehicleId)
    {
        lock (_lock)
        {
            for (int i = 0; i < m_telemetry.Count; i++)
            {
                UDPVehicleTelemetry t = m_telemetry[i];
                if (t.m_vehicleId == vehicleId)
                {
                    return t;
                }
            }

            return null;
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            m_raceInfo = null;
            m_raceInfoString = string.Empty;

            if (m_raceStates != null)
            {
                m_raceStates.Clear();
            }

            if (m_prevRaceStates != null)
            {
                m_prevRaceStates.Clear();
            }

            if (m_statusUpdates != null)
            {
                m_statusUpdates.Clear();
            }

            if (m_telemetry != null)
            {
                m_telemetry.Clear();
            }
        }

        WriteTimestamp();
    }

    public void UpdateRaceInfoString()
    {
        lock (_lock)
        {
            if (m_raceInfo != null)
            {
                m_raceInfoString = m_raceInfo.ToString();
            }
            else
            {
                m_raceInfoString = string.Empty;
            }
        }
    }


    public List<UDPParticipantRaceState> GetLeaderboard()
    {
        lock (_lock)
        {
            List<UDPParticipantRaceState> leaderboard = new List<UDPParticipantRaceState>();
            CopyRaceStates(m_raceStates, out leaderboard);
            leaderboard.Sort();
            return leaderboard;
        }
    }

    public void UpdateStatus()
    {
        if (m_prevRaceStates == null)
        {
            CopyRaceStates(m_raceStates, out m_prevRaceStates);
        }
        else
        {
            List<string> newUpdates = GetLapAndSectorUpdates();
            if (newUpdates != null)
            {
                m_statusUpdates.AddRange(newUpdates);
            }

            CopyRaceStates(m_raceStates, out m_prevRaceStates);
        }
    }

    public void CopyRaceStates(List<UDPParticipantRaceState> from, out List<UDPParticipantRaceState> to)
    {
        to = new List<UDPParticipantRaceState>(from.Count);

        for (int i = 0; i < from.Count; i++)
        {
            to.Add(new UDPParticipantRaceState(from[i]));
        }
    }

    private List<string> GetLapAndSectorUpdates()
    {
        if (m_raceInfo == null || m_raceInfo.m_state == UDPRaceSessionState.Inactive)
        {
            return null;
        }

        if (m_raceStates.Count != m_prevRaceStates.Count)
        {
            return null;
        }

        List<string> updates = new List<string>();
        for (int i = 0; i < m_raceStates.Count; i++)
        {
            UDPParticipantRaceState curr = m_raceStates[i];
            UDPParticipantRaceState prev = m_prevRaceStates[i];

            if (curr.m_vehicleId == prev.m_vehicleId)
            {
                if (curr.m_currentSector != prev.m_currentSector)
                {
                    updates.Add(string.Format("{0} finished sector {1} in {2:0.00} seconds!\r\n", curr.m_driverName, prev.m_currentSector + 1, prev.m_sectorTimes[prev.m_currentSector]));
                }

                if (prev.m_currentLap > 0 && curr.m_currentLap != prev.m_currentLap)
                {
                    updates.Add(string.Format("{0} finished lap {1} in {2:0.00} seconds!\r\n", curr.m_driverName, prev.m_currentLap, prev.m_currentLapTime));
                }

                if (curr.m_sessionFinished && !prev.m_sessionFinished)
                {
                    updates.Add(string.Format("{0} finished session with a best lap of {1:0.00} seconds!\r\n", curr.m_driverName, curr.m_bestLapTime));
                }

            }
        }

        return updates;
    }

    public void WriteTimestamp()
    {
        lock (_lock)
        {
            m_writeTimestamp = DateTime.Now;
        }
    }

    public TimeSpan TimeSinceLastWrite()
    {
        lock (_lock)
        {
            return DateTime.Now - m_writeTimestamp;
        }
    }

    private Lock _lock = new Lock();

    public UDPRaceInfo m_raceInfo = null;
    private List<UDPParticipantRaceState> m_raceStates = new List<UDPParticipantRaceState>();
    private List<UDPVehicleTelemetry> m_telemetry = new List<UDPVehicleTelemetry>();

    private List<UDPParticipantRaceState> m_prevRaceStates = null;
    private List<UDPParticipantRaceState> m_leaderboardEntries = new List<UDPParticipantRaceState>();

    private DateTime m_writeTimestamp = DateTime.Now;
    public string m_raceInfoString = string.Empty;
    public List<string> m_statusUpdates = new List<string>();
}
