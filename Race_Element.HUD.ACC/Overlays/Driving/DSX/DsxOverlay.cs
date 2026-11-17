using Newtonsoft.Json;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using static RaceElement.HUD.ACC.Overlays.Driving.DSX.DsxResources;

namespace RaceElement.HUD.ACC.Overlays.Driving.DSX;

[Overlay(Name = "DSX",
    Description = "Adds active triggers for the DualSense Controller using DSX on steam.\n See Guide in the Discord of Race Element for instructions.",
    OverlayCategory = OverlayCategory.Inputs,
    OverlayType = OverlayType.Drive,
Authors = ["Reinier Klarenberg"])]
internal sealed class DsxOverlay : AbstractOverlay
{
    internal readonly DsxConfiguration _config = new();
    private DsxJob _dsxJob;

    internal UdpClient _client;
    internal IPEndPoint _endPoint;
    private DateTime _timeSent;

    /// <summary>
    /// The index of the controller that packets are sent to.
    /// </summary>
    private const int ControllerIndex = 0;

    /// <summary>
    /// Set when the overlay stops. Wakes up a blinking light bar so stopping doesn't wait for the blink to finish.
    /// </summary>
    private readonly ManualResetEventSlim _stopping = new(false);

    public DsxOverlay(Rectangle rectangle) : base(rectangle, "DSX")
    {
        Width = 1; Height = 1;
        RefreshRateHz = 1;
        AllowReposition = false;
    }

    public override void BeforeStart()
    {
        if (IsPreviewing) return;

        _stopping.Reset();
        _dsxJob = new DsxJob(this) { IntervalMillis = 1000 / 100 };
        _dsxJob.Run();
    }
    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        _stopping.Set();
        _dsxJob?.CancelJoin();
        _client?.Close();
        _client?.Dispose();
    }

    public override bool ShouldRender() => DefaultShouldRender() && !IsPreviewing;

    public override void Render(Graphics g) { }

    internal void SetLighting()
    {
        Debug.WriteLine("Changing RGB");

        // when interrupted the overlay is stopping and the job's AfterCancel resets the light bar
        if (_config.LightBar.BlinkOnConnect && !BlinkLightBar())
        {
            return;
        }

        if (_config.LightBar.OverrideColor)
        {
            Send(CreateLightBarPacket(_config.LightBar.Color));
            ServerResponse lightingResponse = Receive();
            HandleResponse(lightingResponse);
        }
    }

    /// <summary>
    /// Blinks the light bar on and off. When <see cref="DsxConfiguration.LightBarConfig.OverrideColor"/> is Off, the bar is handed<br/>
    /// back to the user's DSX settings afterwards, otherwise the caller applies the <see cref="DsxConfiguration.LightBarConfig.Color"/>.
    /// </summary>
    /// <remarks>
    /// DSX has no instruction to restore only the light bar, so handing it back uses the controller-wide <see cref="InstructionType.ResetToUserSettings"/>.<br/>
    /// This is safe here because it runs once on connect, before any trigger effects are sent.
    /// </remarks>
    /// <returns><c>false</c> when interrupted because the overlay is stopping, <c>true</c> otherwise.</returns>
    private bool BlinkLightBar()
    {
        for (int i = 0; i < _config.LightBar.BlinkCount; i++)
        {
            Send(CreateLightBarPacket(_config.LightBar.BlinkColor));
            if (_stopping.Wait(_config.LightBar.BlinkInterval))
            {
                return false;
            }

            Send(CreateLightBarPacket(Color.Black));
            if (_stopping.Wait(_config.LightBar.BlinkInterval))
            {
                return false;
            }
        }

        if (!_config.LightBar.OverrideColor)
            ResetLightBar();

        return true;
    }

    /// <summary>
    /// Hands the light bar back to the user's DSX settings.
    /// </summary>
    internal void ResetLightBar()
    {
        Packet resetPacket = new();
        resetPacket.instructions = new Instruction[1];
        resetPacket.instructions[0].type = InstructionType.ResetToUserSettings;
        resetPacket.instructions[0].parameters = [ControllerIndex];
        Send(resetPacket);
    }

    private static Packet CreateLightBarPacket(Color color)
    {
        Packet p = new();
        p.instructions = new Instruction[1];  // send only 1 instruction
        p.instructions[0].type = InstructionType.RGBUpdate;
        p.instructions[0].parameters = [ControllerIndex, color.R, color.G, color.B];
        return p;
    }

    internal void CreateEndPoint()
    {
        _client = new UdpClient();
        _endPoint = new IPEndPoint(Triggers.localhost, _config.UDP.Port);
    }

    internal void Send(Packet data)
    {
        var RequestData = Encoding.ASCII.GetBytes(Triggers.PacketToJson(data));
        _client?.Send(RequestData, RequestData.Length, _endPoint);
        _timeSent = DateTime.Now;
    }

    private ServerResponse Receive()
    {
        byte[] bytesReceivedFromServer = _client.Receive(ref _endPoint);

        if (bytesReceivedFromServer.Length > 0)
        {
            ServerResponse ServerResponseJson = JsonConvert.DeserializeObject<ServerResponse>($"{Encoding.ASCII.GetString(bytesReceivedFromServer, 0, bytesReceivedFromServer.Length)}");
            return ServerResponseJson;
        }

        return null;
    }

    private void HandleResponse(ServerResponse response)
    {
        if (response != null)
        {
            Debug.WriteLine("===================================================================");

            Debug.WriteLine($"Status: {response.Status}");
            TimeSpan Timespan = DateTime.Now - _timeSent;
            // First send shows high Milliseconds response time for some reason
            Debug.WriteLine($"Time Received: {response.TimeReceived}, took: {Timespan.TotalMilliseconds} to receive response from DSX");
            Debug.WriteLine($"isControllerConnected: {response.isControllerConnected}");
            Debug.WriteLine($"BatteryLevel: {response.BatteryLevel}");

            Debug.WriteLine("===================================================================\n");
        }
    }
}
