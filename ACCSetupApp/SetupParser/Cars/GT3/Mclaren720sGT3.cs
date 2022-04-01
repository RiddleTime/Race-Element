using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.SetupParser.Cars.GT3
{
    public class Mclaren720sGT3 : ICarSetupConversion
    {
        public string CarName => "McLaren 720S GT3";

        public string ParseName => "mclaren_720s_gt3";

        public CarClasses CarClass => CarClasses.GT3;


        public AbstractTyresSetup TyresSetup => new TyreSetup();
        public class TyreSetup : AbstractTyresSetup
        {
            public override double Camber(Wheel wheel, List<int> rawValue)
            {
                throw new NotImplementedException();
            }

            public override double Caster(int rawValue)
            {
                throw new NotImplementedException();
            }

            public override double Toe(Wheel wheel, List<int> rawValue)
            {
                throw new NotImplementedException();
            }
        }

        public IMechanicalSetup MechanicalSetup => throw new NotImplementedException();

        public IAeroBalance AeroBalance => throw new NotImplementedException();
    }
}
