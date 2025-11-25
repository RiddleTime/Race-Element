using System.Net.Sockets;
using System.Net;

namespace RaceElement.Data.Games.ProjectMotorRacing.ProjectMotorRacingUDP;

internal sealed class UDPThread
{
    static IPEndPoint m_endPoint = null;
    static IPAddress m_multicastGroup = null;
    static UdpClient m_udpClient = null;
    static bool m_multiCast = true;
    static int m_defaultPort = 7576;
    static string m_defaultMulticastGroup = "224.0.0.150";
    public static ushort m_expectedVersion = 1;

    private DataStore m_dataStore = null;

    public UDPThread(DataStore ds, string[] args)
    {
        m_dataStore = ds;

        m_multiCast = GetOptionValue(args, "multicast", true);
        int port = GetOptionValue(args, "port", m_defaultPort);
        string multicastGroup = GetOptionValue(args, "multicast_group", m_defaultMulticastGroup);

        if (m_multiCast)
        {
            m_udpClient = new UdpClient(port);
            m_multicastGroup = IPAddress.Parse(multicastGroup);
            m_endPoint = new IPEndPoint(m_multicastGroup, port);
            m_udpClient.JoinMulticastGroup(m_multicastGroup);
        }
        else
        {
            m_endPoint = new IPEndPoint(IPAddress.Any, port);
            m_udpClient = new UdpClient(m_endPoint);
        }
    }

    private T GetOptionValue<T>(string[] args, string option, T defaultValue)
    {
        foreach (string s in args)
        {
            if (s.Contains(option))
            {
                string[] split = s.Split('=');
                if (split.Length > 1)
                {
                    return (T)Convert.ChangeType(split[1], typeof(T));
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        return defaultValue;
    }

    private void DecodePacket(ref byte[] data)
    {
        byte packetType = data[0];
        if (packetType == (byte)UDPPacketType.RaceInfo)
        {
            UDPRaceInfo p = UDPRaceInfo.Decode(ref data, 1);
            m_dataStore.WriteRaceInfo(p);
        }
        else if (packetType == (byte)UDPPacketType.ParticipantRaceState)
        {
            UDPParticipantRaceState p = UDPParticipantRaceState.Decode(ref data, 1);
            m_dataStore.WriteRaceState(p);
        }
        else if (packetType == (byte)UDPPacketType.ParticipantVehicleTelemetry)
        {
            UDPVehicleTelemetry p = UDPVehicleTelemetry.Decode(ref data, 1);
            m_dataStore.WriteTelemetry(p);
        }

        m_dataStore.WriteTimestamp();
    }

    private void DoWork(object thread)
    {
        // Create an endpoint to store the sender's address
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

        while (!m_aborted)
        {
            // Receive data from a client
            try
            {
                byte[] data = m_udpClient.Receive(ref sender);
                DecodePacket(ref data);
            }
            catch (SocketException)
            {
                Console.WriteLine("UDPClient Closed");
            }
        }
    }

    public void StartThread()
    {
        m_updateThread = new Thread(new ParameterizedThreadStart(DoWork));
        m_updateThread.Start(this);
    }

    public void Shutdown(object sender, EventArgs e)
    {
        m_aborted = true;
        m_udpClient.Close();
    }

    private bool m_aborted = false;
    private Thread m_updateThread;
}
