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

    }
}
