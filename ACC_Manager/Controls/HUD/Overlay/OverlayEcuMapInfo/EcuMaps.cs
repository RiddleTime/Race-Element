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
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Slightly progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "More progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Slow, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[]{ Audi_R8_LMS_2015, Audi_R8_LMS_Evo_II_2022},
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Slightly Wet", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Slow, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[]{ Bentley_Continental_GT3_2018, /* add bentley 2016??? */ },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Slightly Wet", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Different wet", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Slow, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[]{ BMW_M4_GT3_2021, /* add bmw m6 gt3?? */},
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Ferrari_488_GT3_2018 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Ferrari_488_GT3_Evo_2020 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Honda_NSX_GT3_2017, Honda_NSX_GT3_Evo_2019 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Lamborhini_Huracán_GT3_2015, Lamborhini_Huracán_GT3_Evo_2019 },
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
            {   new CarModels[]{ McLaren_720S_GT3_2019 },
                new EcuMap[]{

                }
            },
            {   new CarModels[]{ Mercedes_AMG_GT3_2020, /*Add Mercedes AMG Gt3? */ },
                new EcuMap[]{
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Highest },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Slow, Conditon = EcuMapConditions.PaceCar, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Lowest },
                }
            },
            {   new CarModels[] { Porsche_911_GT3_R_2018, Porsche_911_II_GT3_R_2019 },
                new EcuMap[] {
                    new EcuMap(){ Index = 1, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 2, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 3, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 4, Power = PowerDelivery.Race, Conditon = EcuMapConditions.Wet, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.Medium },
                    new EcuMap(){ Index = 5, Power = PowerDelivery.Qualy, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 6, Power = PowerDelivery.Qualy, Conditon = EcuMapConditions.Dry, ThrottleMap = "Progressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 7, Power = PowerDelivery.Qualy, Conditon = EcuMapConditions.Dry, ThrottleMap = "Aggressive", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 8, Power = PowerDelivery.Qualy, Conditon = EcuMapConditions.Dry, ThrottleMap = "Linear", FuelConsumption = FuelConsumptions.High },
                    new EcuMap(){ Index = 9, Power = PowerDelivery.Low, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Low },
                    new EcuMap(){ Index = 10, Power = PowerDelivery.Slow, Conditon = EcuMapConditions.Dry, ThrottleMap = "Least progressive", FuelConsumption = FuelConsumptions.Lowest }
                }
            }
        };
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
        Slow,
        Low,
        Race,
        Qualy,
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
