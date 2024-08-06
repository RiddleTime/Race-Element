namespace RaceElement.Data.Common.SimulatorData;

public struct DriverInfo
{
    public string FirstName { get; internal set; }
    public string LastName { get; internal set; }
    public string ShortName { get; internal set; }
    // Rookie/D-class/Gold. Possibly enum later
    public string Category { get; internal set; }
    // possibly enum later
    public string Nationality { get; internal set; }
    // iRating or LFM rating
    public int Rating { get; internal set; }
    // Safety rating from iRating of LFM
    public int SafetyRating { get; internal set; }

}
