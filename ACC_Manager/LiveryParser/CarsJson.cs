using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.LiveryParser
{
    public class CarsJson
    {
        public class Root
        {
            public int CarGuid { get; set; }
            public int TeamGuid { get; set; }
            public int RaceNumber { get; set; }
            public int RaceNumberPadding { get; set; }
            public int AuxLightKey { get; set; }
            public int AuxLightColor { get; set; }
            public int SkinTemplateKey { get; set; }
            public int SkinColor1Id { get; set; }
            public int SkinColor2Id { get; set; }
            public int SkinColor3Id { get; set; }
            public int SponsorId { get; set; }
            public int SkinMaterialType1 { get; set; }
            public int SkinMaterialType2 { get; set; }
            public int SkinMaterialType3 { get; set; }
            public int RimColor1Id { get; set; }
            public int RimColor2Id { get; set; }
            public int RimMaterialType1 { get; set; }
            public int RimMaterialType2 { get; set; }
            public string TeamName { get; set; }
            public int Nationality { get; set; }
            public string DisplayName { get; set; }
            public string CompetitorName { get; set; }
            public int CompetitorNationality { get; set; }
            public int TeamTemplateKey { get; set; }
            public int CarModelType { get; set; }
            public int CupCategory { get; set; }
            public int LicenseType { get; set; }
            public int UseEnduranceKit { get; set; }
            public string CustomSkinName { get; set; }
            public int BannerTemplateKey { get; set; }
        }
    }
}
