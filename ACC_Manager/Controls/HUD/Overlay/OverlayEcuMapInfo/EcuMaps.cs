using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.ConversionFactory;
using static ACCSetupApp.SetupParser.ConversionFactory.CarModels;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayEcuMapInfo
{
    internal class EcuMaps
    {
        // Data from: https://www.acc-wiki.info/wiki/ECU_Maps
        public static Dictionary<CarModels[], EcuMap[]> maps = new Dictionary<CarModels[], EcuMap[]>
        {
            {   new CarModels[]{ Aston_Martin_V8_Vantage_GT3_2019, Aston_Martin_Vantage_V12_GT3_2013 },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Slightly progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "More progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[]{ Audi_R8_LMS_2015, Audi_R8_LMS_Evo_II_2022},
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Slightly Wet", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[]{ Bentley_Continental_GT3_2015, Bentley_Continental_GT3_2018 },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Slightly Wet", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[]{ BMW_M4_GT3_2021, BMW_M6_GT3_2017 },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Reserve Engineer Map", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Wet", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Medium},
                }
            },
            {   new CarModels[]{ Ferrari_488_GT3_2018 },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.High, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Moderate Aggressive", FuelConsumption = FuelConsumptions.Lowest },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.High, Conditon = EcuMapConditions.Wet, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Low, Conditon = EcuMapConditions.Wet, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Low, Conditon = EcuMapConditions.Wet, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 9, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 10, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 11, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 12, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                }
            },
            {   new CarModels[]{ Ferrari_488_GT3_Evo_2020 },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Extreme Aggressive", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.High, Conditon = EcuMapConditions.Dry, ThrottleMap = "Quite Aggressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Moderate Aggressive", FuelConsumption = FuelConsumptions.Lowest },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Almost Linear", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Low, Conditon = EcuMapConditions.Wet, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 9, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 10, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 11, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                    new EcuMap(){ Index = 12, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                }
            },
            {   new CarModels[]{ Honda_NSX_GT3_2017, Honda_NSX_GT3_Evo_2019 },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Very Aggressive", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Wet, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Wet, ThrottleMap = "More Progressive", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Gradual", FuelConsumption = FuelConsumptions.Lowest},
                }
            },
            {   new CarModels[]{ Lamborghini_Huracán_GT3_2015, Lamborghini_Huracán_GT3_Evo_2019 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Emil_Frey_Jaguar_G3_2021 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Nissan_GT_R_Nismo_GT3_2015, Nissan_GT_R_Nismo_GT3_2018 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Lexus_RCF_GT3_2016 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ McLaren_650S_GT3_2015 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ McLaren_720S_GT3_2019 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Mercedes_AMG_GT3_2015, Mercedes_AMG_GT3_Evo_2020 },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[] { Porsche_911_GT3_R_2018, Porsche_911_II_GT3_R_2019 },
                new EcuMap[] {
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Medium, Conditon = EcuMapConditions.Wet, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Highest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 9, Power = PowerDelivery.Low, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 10, Power = PowerDelivery.Lowest, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest }
                }
            }
        };

        public static EcuMap GetMap(string modelName, int map)
        {
            CarModels carModel = ParseNames[modelName];

            EcuMap[] carMaps = null;
            foreach (KeyValuePair<CarModels[], EcuMap[]> maps in EcuMaps.maps.ToList())
            {
                foreach (CarModels mapModel in maps.Key)
                {
                    if (carModel == mapModel)
                    {
                        carMaps = maps.Value;
                        break;
                    }
                }
                if (carMaps != null) break;
            }

            return carMaps[map];
        }
    }

    public class EcuMap
    {
        public int Index;
        public PowerDelivery Power;
        public string ThrottleMap;
        public FuelConsumptions FuelConsumption;
        public EcuMapConditions Conditon;

    }

    public enum PowerDelivery
    {
        Lowest,
        Low,
        Medium,
        High,
        Highest,
    }

    public enum FuelConsumptions
    {
        Lowest,
        Low,
        Medium,
        High,
        Highest
    }

    public enum EcuMapConditions
    {
        Dry,
        Wet,
        PaceCar,
        LowPower
    }
}
