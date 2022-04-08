using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCSetupApp.SetupParser.Cars.GT3;
using ACCSetupApp.Util;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser
{
    public class ConversionFactory
    {
        private static Dictionary<string, ICarSetupConversion> carConversions = new Dictionary<string, ICarSetupConversion>()
        {
            {"audi_r8_lms", new AudiR8LMS() },
            {"audi_r8_lms_evo_ii", new AudiR8LMSevoII() },
            {"bentley_continental_gt3_2018",new BentleyContinentalGT3_2018() },
            {"bmw_m4_gt3", new BmwM4GT3() },
            {"ferrari_488_gt3_evo", new Ferrari488GT3evo() },
            {"honda_nsx_gt3", new HondaNsxGT3() },
            {"honda_nsx_gt3_evo", new HondaNsxGT3Evo() },
            {"lamborghini_huracan_gt3_evo",new LamborghiniHuracanGT3evo() },
            {"lexus_rc_f_gt3", new LexusRcfGT3() },
            {"mclaren_720s_gt3", new Mclaren720sGT3() },
            {"mercedes_amg_gt3_evo" , new MercedesAMGGT3evo() },
            {"nissan_gt_r_gt3_2018",new NissanGtrGT3_2018() },
            {"porsche_991ii_gt3_r", new Porsche911IIGT3R() },
            {"porsche_991_gt3_r", new Porsche991GT3R()}
        };
        internal ICarSetupConversion GetConversion(string parseName)
        {
            if (carParseNames.ContainsKey(parseName))
                return carConversions[parseName];
            else
                return null;
        }

        public List<string> GetAllGT3Names()
        {
            List<string> GT3Names = new List<string>();
            carConversions.ToList().ForEach(x =>
            {
                if (x.Value.CarClass == CarClasses.GT3)
                {
                    GT3Names.Add(x.Value.CarName);
                }
            });

            return GT3Names;
        }

        private static Dictionary<string, string> carParseNames = new Dictionary<string, string>()
        {
            {"audi_r8_lms", "Audi R8 LMS" },
            {"audi_r8_lms_evo_ii", "Audi R8 LMS evo II" },
            {"bentley_continental_gt3_2018", "Bentley Continental GT3 2018" },
            {"bmw_m4_gt3", "BMW M4 GT3" },
            {"ferrari_488_gt3_evo", "Ferrari 488 GT3 Evo" },
            {"honda_nsx_gt3", "Honda NSX GT3" },
            {"honda_nsx_gt3_evo", "Honda NSX GT3 Evo" },
            {"lexus_rc_f_gt3", "Lexus RC F GT3" },
            {"lamborghini_huracan_gt3_evo","Lamborghini Huracán GT3 Evo" },
            {"mclaren_720s_gt3", "McLaren 720S GT3" },
            {"mercedes_amg_gt3_evo" , "Mercedes-AMG GT3 2020"},
            {"nissan_gt_r_gt3_2018","Nissan GT-R Nismo GT3 2018" },
            {"porsche_991ii_gt3_r", "Porsche 911 II GT3 R" },
            {"porsche_991_gt3_r", "Porsche 911 GT3 R"}
        };
        internal string ParseCarName(string parseName)
        {
            if (carParseNames.ContainsKey(parseName))
                return carParseNames[parseName];
            else return parseName;
        }

        private static Dictionary<int, string> CarModelTypeIds = new Dictionary<int, string>()
        {
            {1, "Mercedes AMG GT3 2015" },
            {2, "Ferrari 488 GT3 2018"},
            {3, "Audi R8 LMS 2015" },
            {4, "Lamborghini Huracán GT3 2015"},
            {5, "McLaren 650S GT3 2015" },
            {6, "Nissan GTR Nismo GT3 2018"},
            {7, "BMW M6 GT3 2017" },
            {8, "Bentley Continental GT3 2018" },
            {9, "Porsche9 91 II GT3 Cup 2017" },
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
                return "Unknown car model";
        }



    }
}
