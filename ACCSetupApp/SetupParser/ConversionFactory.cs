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
        private Dictionary<string, ICarSetupConversion> carConversions = new Dictionary<string, ICarSetupConversion>()
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

        private Dictionary<string, string> carParseNames = new Dictionary<string, string>()
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



    }
}
