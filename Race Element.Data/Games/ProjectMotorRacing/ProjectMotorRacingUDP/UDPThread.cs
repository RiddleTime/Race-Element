using System.Net.Sockets;
using System.Net;
using System.Diagnostics;

namespace RaceElement.Data.Games.ProjectMotorRacing.ProjectMotorRacingUDP;

internal sealed class UDPThread
{
    static IPEndPoint m_endPoint = null;
    static IPAddress m_multicastGroup = null;
    static UdpClient m_udpClient = null;
    static bool m_multiCast = true;
    static int m_defaultPort = 7577;
    static string m_defaultMulticastGroup = "224.0.0.150";
    public static ushort m_expectedVersion = 1;

    private DataStore m_dataStore = null;

    public UDPThread(DataStore ds, string[] args)
    {
        m_dataStore = ds;

        m_multiCast = GetOptionValue(args, "multicast", true);
        int port = GetOptionValue(args, "port", m_defaultPort);
        string multicastGroup = GetOptionValue(args, "multicast_group", m_defaultMulticastGroup);
        m_multicastGroup = IPAddress.Parse(multicastGroup);

        // Create the client without immediate binding
        m_udpClient = new();

        // Allow multiple apps to bind to the same port
        m_udpClient.ExclusiveAddressUse = false; // Recommended, though the next line is the key on Windows
        m_udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

        if (m_multiCast)
        {
            // Bind explicitly to Any + port
            m_udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, port));

            // Join the multicast group AFTER binding
            m_udpClient.JoinMulticastGroup(m_multicastGroup);

            m_endPoint = new IPEndPoint(m_multicastGroup, port);
        }
        else // unicast mode
        {
            m_endPoint = new IPEndPoint(IPAddress.Any, port);
            m_udpClient.Client.Bind(m_endPoint);
            // No JoinMulticastGroup needed for unicast
        }
    }

    private static T GetOptionValue<T>(string[] args, string option, T defaultValue)
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

    private async Task DoWork()
    {
        // Create an endpoint to store the sender's address
        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

        while (!m_aborted)
        {
            try
            {
                var packet = await m_udpClient.ReceiveAsync();
                byte[] data = packet.Buffer;
                DecodePacket(ref data);
            }
            catch (Exception e)
            {

            }
        }
        Debug.WriteLine("--- Stopping PMR UDP thread");

    }


    public void StartThread()
    {
        m_updateThread = new Thread(async () => await DoWork());
        m_updateThread.Start();
    }

    public void Shutdown(object sender, EventArgs e)
    {
        m_aborted = true;
        m_udpClient?.DropMulticastGroup(m_multicastGroup);
        m_udpClient?.Close();
        m_udpClient?.Dispose();
        m_updateThread?.Join();
    }

    private bool m_aborted = false;
    private Thread m_updateThread;
}
