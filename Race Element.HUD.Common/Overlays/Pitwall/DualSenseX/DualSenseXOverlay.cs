using Newtonsoft.Json;
using RaceElement.Data.Games;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static RaceElement.HUD.Common.Overlays.Pitwall.DualSenseX.DualSenseXResources;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DualSenseX;

[Overlay(Name = "DualSense X",
    Description = "Adds active triggers for the DualSense 5 controller using DSX on steam.\n See Guide in the Discord of Race Element for instructions.",
    OverlayCategory = OverlayCategory.Inputs,
    OverlayType = OverlayType.Pitwall,
    Game = Game.RaceRoom | Game.AssettoCorsa1,
    Authors = ["Reinier Klarenberg"]
)]
internal sealed class DualSenseXOverlay : CommonAbstractOverlay
{
    internal readonly DualSenseXConfiguration _config = new();
    private DualSenseXJob _dsxJob;

    internal UdpClient _client;
    internal IPEndPoint _endPoint;
    private DateTime _timeSent;

    public DualSenseXOverlay(Rectangle rectangle) : base(rectangle, "DualSense X")
    {
        Width = 1; Height = 1;
        RefreshRateHz = 1;
        AllowReposition = false;
    }

    public override void BeforeStart()
    {
        if (IsPreviewing) return;

        _dsxJob = new DualSenseXJob(this) { IntervalMillis = 1000 / 100 };
        _dsxJob.Run();
    }
    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        _dsxJob?.CancelJoin();
        _client?.Close();
        _client?.Dispose();
    }

    public override bool ShouldRender() => DefaultShouldRender() && !IsPreviewing;

    public override void Render(Graphics g) { }

    internal void SetLighting()
    {
        Debug.WriteLine("Changing RGB");
        Packet p = new();
        int controllerIndex = 0;

        p.instructions = new Instruction[1];  // send only 1 instruction
        p.instructions[0].type = InstructionType.RGBUpdate;
        p.instructions[0].parameters = [controllerIndex, 255, 69, 0];

        Send(p);
        ServerResponse lightingReponse = Receive();
        HandleResponse(lightingReponse);
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
