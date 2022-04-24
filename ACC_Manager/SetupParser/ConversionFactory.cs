using System.Collections.Generic;
using System.Linq;
using ACCSetupApp.SetupParser.Cars.GT3;
using ACCSetupApp.SetupParser.Cars.GT4;
using static ACCSetupApp.SetupParser.SetupConverter;
using static ACCSetupApp.SetupParser.ConversionFactory.CarModels;

namespace ACCSetupApp.SetupParser
{
    public class ConversionFactory
    {
        private static readonly Dictionary<string, ICarSetupConversion> carConversions = new Dictionary<string, ICarSetupConversion>()
        {
            {"amr_v8_vantage_gt3", new AMRV8VantageGT3() },
            {"amr_v12_vantage_gt3", new AMRV12VantageGT3() },
            {"audi_r8_lms", new AudiR8LMS() },
            {"audi_r8_lms_evo_ii", new AudiR8LMSevoII() },
            {"bentley_continental_gt3_2018", new BentleyContinentalGT3_2018() },
            {"bmw_m4_gt3", new BmwM4GT3() },
            {"jaguar_g3",new JaguarG3GT3() },
            {"ferrari_488_gt3", new Ferrari488GT3() },
            {"ferrari_488_gt3_evo", new Ferrari488GT3evo() },
            {"honda_nsx_gt3", new HondaNsxGT3() },
            {"honda_nsx_gt3_evo", new HondaNsxGT3Evo() },
            {"lamborghini_huracan_gt3",new LamborghiniHuracanGT3() },
            {"lamborghini_huracan_gt3_evo", new LamborghiniHuracanGT3evo() },
            {"lexus_rc_f_gt3", new LexusRcfGT3() },
            {"mclaren_720s_gt3", new Mclaren720sGT3() },
            {"mercedes_amg_gt3_evo", new MercedesAMGGT3evo() },
            {"nissan_gt_r_gt3_2017", new NissanGtrGT3_2015() },
            {"nissan_gt_r_gt3_2018", new NissanGtrGT3_2018() },
            {"porsche_991ii_gt3_r", new Porsche911IIGT3R() },
            {"porsche_991_gt3_r", new Porsche991GT3R() },
            {"alpine_a110_gt4", new AlpineA110GT4() },
            {"amr_v8_vantage_gt4", new AMRV8VantageGT4() },
            {"audi_r8_gt4", new AudiR8GT4() },
            {"bmw_m4_gt4", new BMWM4GT4() },
            {"chevrolet_camaro_gt4r", new ChevroletCamaroGT4R() },
            {"ginetta_g55_gt4", new GinettaG55GT4() },
            {"ktm_xbow_gt4", new KTMXbowGT4() },
            {"maserati_mc_gt4", new MaseratiMCGT4() },
            {"mclaren_570s_gt4", new Mclaren570SGT4() },
            {"mercedes_amg_gt4", new MercedesAMGGT4() },
            {"porsche_718_cayman_gt4_mr", new Porsche718CaymanGT4MR() }
        };
        internal ICarSetupConversion GetConversion(string parseName)
        {
            if (ParseNames.ContainsKey(parseName))
                return carConversions[parseName];
            else
                return null;
        }

        public static List<string> GetAllNamesByClass(CarClasses carClass)
        {
            List<string> classNames = new List<string>();
            carConversions.ToList().ForEach(x =>
            {
                if (x.Value.CarClass == carClass)
                {
                    classNames.Add(CarNames[x.Value.CarModel]);
                }
            });

            return classNames;
        }

        public enum CarModels
        {
            None,
            Aston_Martin_V8_Vantage_GT3_2019,
            Aston_Martin_Vantage_V12_GT3_2013,
            Audi_R8_LMS_2015,
            Audi_R8_LMS_Evo_II_2022,
            Bentley_Continental_GT3_2018,
            BMW_M4_GT3_2021,
            Emil_Frey_Jaguar_G3_2021,
            Ferrari_488_GT3_2018,
            Ferrari_488_GT3_Evo_2020,
            Honda_NSX_GT3_2017,
            Honda_NSX_GT3_Evo_2019,
            Lexus_RCF_GT3_2016,
            Lamborhini_Huracán_GT3_2015,
            Lamborhini_Huracán_GT3_Evo_2019,
            Mclaren_650S_GT3_2015,
            McLaren_720S_GT3_2019,
            Mercedes_AMG_GT3_2015,
            Mercedes_AMG_GT3_2020,
            Nissan_GT_R_Nismo_GT3_2015,
            Nissan_GT_R_Nismo_GT3_2018,
            Porsche_911_II_GT3_R_2019,
            Porsche_911_GT3_R_2018,
            Alpine_A110_GT4_2018,
            Aston_Martin_Vantage_AMR_GT4_2018,
            Audi_R8_LMS_GT4_2016,
            BMW_M4_GT4_2018,
            Chevrolet_Camaro_GT4_R_2017,
            Ginetta_G55_GT4_2012,
            KTM_Xbow_GT4_2016,
            Maserati_Gran_Turismo_MC_GT4_2016,
            McLaren_570s_GT4_2016,
            Mercedes_AMG_GT4_2016,
            Porsche_718_Cayman_GT4_MR_2019
        }

        public static readonly Dictionary<CarModels, string> CarNames = new Dictionary<CarModels, string>() {
            {None, "Unknown car model" },
            {Aston_Martin_V8_Vantage_GT3_2019, "Aston Martin V8 Vantage GT3 2019"},
            {Aston_Martin_Vantage_V12_GT3_2013, "Aston Martin Vantage V12 GT3 2013"},
            {Audi_R8_LMS_2015, "Audi R8 LMS 2015" },
            {Audi_R8_LMS_Evo_II_2022, "Audi R8 LMS Evo II 2022" },
            {Bentley_Continental_GT3_2018, "Bentley Continental GT3 2018" },
            {BMW_M4_GT3_2021, "BMW M4 GT3 2021" },
            {Emil_Frey_Jaguar_G3_2021, "Emil Frey Jaguar G3 2012" },
            {Ferrari_488_GT3_2018, "Ferrari 488 GT3 2018" },
            {Ferrari_488_GT3_Evo_2020, "Ferrari 488 GT3 Evo 2020" },
            {Honda_NSX_GT3_2017, "Honda NSX GT3 2017" },
            {Honda_NSX_GT3_Evo_2019, "Honda NSX GT3 Evo 2019" },
            {Lexus_RCF_GT3_2016, "Lexus RCF GT3 2016" },
            {Lamborhini_Huracán_GT3_2015,"Lamborghini Huracán GT3 2015" },
            {Lamborhini_Huracán_GT3_Evo_2019,"Lamborghini Huracán GT3 Evo 2019" },
            {McLaren_720S_GT3_2019, "McLaren 720S GT3 2019" },
            {Mercedes_AMG_GT3_2020 , "Mercedes-AMG GT3 2020"},
            {Nissan_GT_R_Nismo_GT3_2015, "Nissan GT-R Nismo GT3 2015" },
            {Nissan_GT_R_Nismo_GT3_2018, "Nissan GT-R Nismo GT3 2018" },
            {Porsche_911_II_GT3_R_2019, "Porsche 911 II GT3 R 2019" },
            {Porsche_911_GT3_R_2018, "Porsche 911 GT3 R 2018"},
            {Alpine_A110_GT4_2018, "Alpine A110 GT4 2018"},
            {Aston_Martin_Vantage_AMR_GT4_2018, "Aston Martin Vantage AMR GT4 2018"},
            {Audi_R8_LMS_GT4_2016, "Audi R8 LMS GT4 2016"},
            {BMW_M4_GT4_2018, "BMW M4 GT4 2018"},
            {Chevrolet_Camaro_GT4_R_2017, "Chevrolet Camaro GT4 R 2017"},
            {Ginetta_G55_GT4_2012, "Ginetta G55 GT4 2012"},
            {KTM_Xbow_GT4_2016, "KTM Xbow GT4 2016"},
            {Maserati_Gran_Turismo_MC_GT4_2016, "Maserati Gran Turismo MC GT4 2016"},
            {McLaren_570s_GT4_2016, "McLaren 570s GT4 2016"},
            {Mercedes_AMG_GT4_2016, "Mercedes AMG GT4 2016"},
            {Porsche_718_Cayman_GT4_MR_2019, "Porsche 718 Cayman GT4 MR 2019"}
        };

        public static readonly Dictionary<string, CarModels> ParseNames = new Dictionary<string, CarModels>()
        {
            {"amr_v8_vantage_gt3", Aston_Martin_V8_Vantage_GT3_2019 },
            {"amr_v12_vantage_gt3", Aston_Martin_Vantage_V12_GT3_2013 },
            {"audi_r8_lms", Audi_R8_LMS_2015 },
            {"audi_r8_lms_evo_ii", Audi_R8_LMS_Evo_II_2022 },
            {"bentley_continental_gt3_2018", Bentley_Continental_GT3_2018 },
            {"bmw_m4_gt3", BMW_M4_GT3_2021 },
            {"jaguar_g3", Emil_Frey_Jaguar_G3_2021 },
            {"ferrari_488_gt3", Ferrari_488_GT3_2018 },
            {"ferrari_488_gt3_evo", Ferrari_488_GT3_Evo_2020 },
            {"honda_nsx_gt3", Honda_NSX_GT3_2017 },
            {"honda_nsx_gt3_evo", Honda_NSX_GT3_Evo_2019 },
            {"lexus_rc_f_gt3", Lexus_RCF_GT3_2016 },
            {"lamborghini_huracan_gt3", Lamborhini_Huracán_GT3_2015 },
            {"lamborghini_huracan_gt3_evo", Lamborhini_Huracán_GT3_Evo_2019 },
            {"mclaren_720s_gt3", McLaren_720S_GT3_2019 },
            {"mercedes_amg_gt3_evo", Mercedes_AMG_GT3_2020 },
            {"nissan_gt_r_gt3_2017", Nissan_GT_R_Nismo_GT3_2015 }, // yes the parsename is 2017 and not 2015.. (kunos feature)
            {"nissan_gt_r_gt3_2018", Nissan_GT_R_Nismo_GT3_2018 },
            {"porsche_991ii_gt3_r", Porsche_911_II_GT3_R_2019 },
            {"porsche_991_gt3_r", Porsche_911_GT3_R_2018 },
            {"alpine_a110_gt4", Alpine_A110_GT4_2018 },
            {"amr_v8_vantage_gt4", Aston_Martin_Vantage_AMR_GT4_2018 },
            {"audi_r8_gt4", Audi_R8_LMS_GT4_2016 },
            {"bmw_m4_gt4", BMW_M4_GT4_2018 },
            {"chevrolet_camaro_gt4r", Chevrolet_Camaro_GT4_R_2017 },
            {"ginetta_g55_gt4", Ginetta_G55_GT4_2012 },
            {"ktm_xbow_gt4", KTM_Xbow_GT4_2016 },
            {"maserati_mc_gt4", Maserati_Gran_Turismo_MC_GT4_2016 },
            {"mclaren_570s_gt4", McLaren_570s_GT4_2016 },
            {"mercedes_amg_gt4", Mercedes_AMG_GT4_2016 },
            {"porsche_718_cayman_gt4_mr", Porsche_718_Cayman_GT4_MR_2019 }
        };

        internal static CarModels ParseCarName(string parseName)
        {
            if (ParseNames.ContainsKey(parseName))
                return ParseNames[parseName];
            else return None;
        }

        private static readonly Dictionary<int, string> CarModelTypeIds = new Dictionary<int, string>()
        {
            {0, "Porsche 991 GT3 R 2018" },
            {1, "Mercedes AMG GT3 2015" },
            {2, "Ferrari 488 GT3 2018"},
            {3, "Audi R8 LMS 2015" },
            {4, "Lamborghini Huracán GT3 2015"},
            {5, "McLaren 650S GT3 2015" },
            {6, "Nissan GTR Nismo GT3 2018"},
            {7, "BMW M6 GT3 2017" },
            {8, "Bentley Continental GT3 2018" },
            {9, "Porsche 991 II GT3 Cup 2017" },
            {10, "Nissan GTR Nismo GT3 2015" },
            {11, "Bentley Continental GT3 2015" },
            {12, "Aston Martin Vantage V12 GT3 2013" },
            {13, "Lamborghini Gallardo G3 Reiter 2017" },
            {14, "Emil Frey Jaguar G3 2012" },
            {15, "Lexus RCF GT3 2016" },
            {16, "Lamborghini Huracán GT3 EVO 2019" },
            {17, "Honda NSX GT3 2017" },
            {18, "Lamborghini Huracán ST 2015" },
            {19, "Audi R8 LMS Evo 2019" },
            {20, "Aston Martin V8 Vantage GT3 2019" },
            {21, "Honda NSX GT3 Evo 2019" },
            {22, "McLaren 720S GT3 2019" },
            {23, "Porsche 911 II GT3 R 2019" },
            {24, "Ferrari 488 GT3 Evo 2020" },
            {25, "Mercedes AMG GT3 Evo 2020" },
            {26, "Ferrari 488 Challenge Evo 2020" },
            {27, "BMW M2 Cup 2020" },
            {28, "Porsche 992 GT3 Cup 2021" },
            {29, "Lamborghini Huracán ST Evo2 2021" },
            {30, "BMW M4 GT3 2021" },
            {31, "Audi R8 LMS Evo II 2022" },
            {50, "Alpine A110 GT4 2018" },
            {51, "Aston Martin Vantage AMR GT4 2018" },
            {52, "Audi R8 LMS GT4 2016" },
            {53, "BMW M4 GT4 2018" },
            {55, "Chevrolet Camaro GT4 R 2017" },
            {56, "Ginetta G55 GT4 2012" },
            {57, "Ktm Xbow GT4 2016" },
            {58, "Maserati Gran Turismo MC GT4 2016" },
            {59, "McLaren 570s GT4 2016" },
            {60, "Mercedes AMG GT4 2016" },
            {61, "Porsche 718 Cayman GT4 MR 2019" }
        };

        public string GetCarName(int carId)
        {
            if (CarModelTypeIds.ContainsKey(carId))
                return CarModelTypeIds[carId];
            else
                return $"Unknown: {carId}";
        }
    }
}
