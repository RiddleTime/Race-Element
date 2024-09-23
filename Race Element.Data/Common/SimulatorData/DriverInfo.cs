namespace RaceElement.Data.Common.SimulatorData;

public struct DriverInfo
{
    // "Firstname Lastname" or whatever the sim wants displayed
    public string Name { get; set; }
    public string ShortName { get; internal set; }
    // Something like Rookie/D-class/Gold.
    public string Category { get; internal set; }
    // possibly enum later
    public string Nationality { get; internal set; }
    // iRating or LFM rating
    public int Rating { get; internal set; }
    // Safety rating from iRating of LFM
    public int SafetyRating { get; internal set; }

}
