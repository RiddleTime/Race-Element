namespace RaceElement.HUD.ACC.Overlays.OverlayPressureTrace;

internal static class TyrePressures
{
    public static TyrePressureRange DRY_DHE2020_GT4 = new()
    {
        OptimalMinimum = 26.8,
        OptimalMaximum = 27.4
    };

    public static TyrePressureRange DRY_DHE2020 = new()
    {
        OptimalMinimum = 27.3,
        OptimalMaximum = 27.9
    };
    public static TyrePressureRange WET_ALL = new()
    {
        OptimalMinimum = 29.5,
        OptimalMaximum = 30.5
    };

    public static TyrePressureRange DRY_DHF2023 = new()
    {
        OptimalMinimum = 26.0,
        OptimalMaximum = 27.3,
    };

    // public static TyrePressureRange WET_WH2023 = new TyrePressureRange()
    // {

    // };


    public static TyrePressureRange GetCurrentRange(string compound, string carModel)
    {
        if (compound == "wet_compound")
            return WET_ALL;

        return DRY_DHF2023;
    }
}

public class TyrePressureRange
{
    public double OptimalMinimum;
    public double OptimalMaximum;
}
