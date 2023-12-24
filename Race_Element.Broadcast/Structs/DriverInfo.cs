namespace RaceElement.Broadcast.Structs;

public struct DriverInfo
{
    public string FirstName { get; internal set; }
    public string LastName { get; internal set; }
    public string ShortName { get; internal set; }
    public DriverCategory Category { get; internal set; }
    public NationalityEnum Nationality { get; internal set; }
}
