namespace RaceElement.Broadcast.Structs;

public struct BroadcastingEvent
{
    public BroadcastingCarEventType Type { get; internal set; }
    public string Msg { get; internal set; }
    public int TimeMs { get; internal set; }
    public int CarId { get; internal set; }
    public CarInfo CarData { get; internal set; }
}
