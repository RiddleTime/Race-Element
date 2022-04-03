using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.LiveryParser
{
    public class CarsJson
    {
        public class Root
        {
            public int carGuid { get; set; }
            public int teamGuid { get; set; }
            public int raceNumber { get; set; }
            public int raceNumberPadding { get; set; }
            public int auxLightKey { get; set; }
            public int auxLightColor { get; set; }
            public int skinTemplateKey { get; set; }
            public int skinColor1Id { get; set; }
            public int skinColor2Id { get; set; }
            public int skinColor3Id { get; set; }
            public int sponsorId { get; set; }
            public int skinMaterialType1 { get; set; }
            public int skinMaterialType2 { get; set; }
            public int skinMaterialType3 { get; set; }
            public int rimColor1Id { get; set; }
            public int rimColor2Id { get; set; }
            public int rimMaterialType1 { get; set; }
            public int rimMaterialType2 { get; set; }
            public string teamName { get; set; }
            public int nationality { get; set; }
            public string displayName { get; set; }
            public string competitorName { get; set; }
            public int competitorNationality { get; set; }
            public int teamTemplateKey { get; set; }
            public int carModelType { get; set; }
            public int cupCategory { get; set; }
            public int licenseType { get; set; }
            public int useEnduranceKit { get; set; }
            public string customSkinName { get; set; }
            public int bannerTemplateKey { get; set; }
        }


    }
}
