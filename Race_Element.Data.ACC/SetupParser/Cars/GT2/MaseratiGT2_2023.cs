using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Data.ACC.SetupParser.Cars.GT2;

internal class MaseratiGT2_2023 : ICarSetupConversion
{
    public ConversionFactory.CarModels CarModel => CarModels.Maserati_GT2_2023;

    public CarClasses CarClass => CarClasses.GT2;

    public DryTyreCompounds DryTyreCompound => DryTyreCompounds.DHF2023_GT4;

    public AbstractTyresSetup TyresSetup => throw new NotImplementedException();

    public IDamperSetup DamperSetup => throw new NotImplementedException();

    public IMechanicalSetup MechanicalSetup => throw new NotImplementedException();

    public IAeroBalance AeroBalance => throw new NotImplementedException();
}
