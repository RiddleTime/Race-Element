using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCSetupApp.SetupParser.Cars.GT3;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser
{
    public class ConversionFactory
    {
        public ICarSetupConversion GetConversion(string parseName)
        {
            switch (parseName)
            {
                case "audi_r8_lms": return new AudiR8LMS();
                case "audi_r8_lms_evo_ii": return new AudiR8LMSevoII();


                case "honda_nsx_gt3": return new HondaNsxGT3();
                case "honda_nsx_gt3_evo": return new HondaNsxGT3Evo();

                case "mclaren_720s_gt3": return new Mclaren720sGT3();


                case "porsche_991_gt3_r": return new Porsche991GT3R();
                case "porsche_991ii_gt3_r": return new Porsche911IIGT3R();



                default: return null;
            }
        }

        public string ParseCarName(string parseName)
        {
            switch (parseName)
            {
                case "alpine_a110_gt4": return "Alpine A110 GT4";
                case "amr_v8_vantage_gt3": return "Aston Martin Racing V8 Vantage GT3";
                case "amr_v8_vantage_gt4": return "Aston Martin Racing V8 Vantage GT4";
                case "amr_v12_vantage_gt3": return "Aston Martin Racing V12 Vantage GT3";
                case "audi_r8_gt4": return "Audi R8 LMS GT4";
                case "audi_r8_lms": return "Audi R8 LMS";
                case "audi_r8_lms_evo_ii": return "Audi R8 LMS evo II";


                case "honda_nsx_gt3": return "Honda NSX GT3";
                case "honda_nsx_gt3_evo": return "Honda NSX GT3 Evo";


                case "mclaren_720s_gt3": return "McLaren 720S GT3";


                case "porsche_991ii_gt3_r": return "Porsche 911 II GT3 R";
                case "porsche_991_gt3_r": return "Porsche 911 GT3 R";


                default: return parseName;
            }
        }

    }
}
