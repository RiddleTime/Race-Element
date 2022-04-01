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
                case "porsche_991ii_gt3_r": return new Porsche911II();
                case "honda_nsx_gt3_evo": return new HondaNsxEvo();

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


                case "porsche_991ii_gt3_r": return "Porsche 911 II GT3 R";
                case "honda_nsx_gt3_evo": return "Honda NSX GT3 Evo";

                default: return parseName;
            }
        }

    }
}
