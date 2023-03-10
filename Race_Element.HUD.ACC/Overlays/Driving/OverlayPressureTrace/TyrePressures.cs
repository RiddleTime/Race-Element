using RaceElement.Data;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.HUD.ACC.Overlays.OverlayPressureTrace
{
    internal static class TyrePressures
    {
        public static TyrePressureRange DRY_DHE2020_GT4 = new TyrePressureRange()
        {
            OptimalMinimum = 26.8,
            OptimalMaximum = 27.4
        };

        public static TyrePressureRange DRY_DHE2020 = new TyrePressureRange()
        {
            OptimalMinimum = 27.3,
            OptimalMaximum = 27.9
        };
        public static TyrePressureRange WET_ALL = new TyrePressureRange()
        {
            OptimalMinimum = 29.5,
            OptimalMaximum = 30.5
        };


        public static TyrePressureRange GetCurrentRange(string compound, string carModel)
        {
            if (compound == "wet_compound")
            {
                return WET_ALL;
            }

            CarModels model = ConversionFactory.ParseCarName(carModel);
            ICarSetupConversion setupConversion = GetConversion(model);

            if (setupConversion != null)
                switch (setupConversion.DryTyreCompound)
                {
                    case DryTyreCompounds.DHE2020: return DRY_DHE2020;
                    default: return DRY_DHE2020_GT4;
                }

            // gotta return something.. I know isn't nice, but solutions..
            return DRY_DHE2020;
        }
    }

    public class TyrePressureRange
    {
        public double OptimalMinimum;
        public double OptimalMaximum;
    }
}
