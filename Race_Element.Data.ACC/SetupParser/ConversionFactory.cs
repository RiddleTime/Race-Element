using Newtonsoft.Json;
using RaceElement.Data.ACC.SetupParser.Cars.GT2;
using RaceElement.Data.Cars.GT3;
using RaceElement.Data.Cars.GT4;
using RaceElement.Data.Cars.GTC;
using RaceElement.Data.Cars.TCX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static RaceElement.Data.ConversionFactory.CarModels;
using static RaceElement.Data.SetupConverter;
using static RaceElement.Data.SetupJson;

namespace RaceElement.Data;

public static class ConversionFactory
{
    public enum CarModels
    {
        None,
        Alpine_A110_GT4_2018,
        Aston_Martin_V8_Vantage_GT3_2019,
        Aston_Martin_Vantage_AMR_GT4_2018,
        Aston_Martin_Vantage_V12_GT3_2013,
        Audi_R8_LMS_2015,
        Audi_R8_LMS_GT4_2016,
        Audi_R8_LMS_Evo_2019,
        Audi_R8_LMS_Evo_II_2022,
        Audi_R8_LMS_GT2_2021,
        Bentley_Continental_GT3_2015,
        Bentley_Continental_GT3_2018,
        BMW_M2_Cup_2020,
        BMW_M4_GT3_2021,
        BMW_M4_GT4_2018,
        BMW_M6_GT3_2017,
        Chevrolet_Camaro_GT4_R_2017,
        Emil_Frey_Jaguar_G3_2021,
        Ferrari_296_GT3_2023,
        Ferrari_488_Challenge_Evo_2020,
        Ferrari_488_GT3_2018,
        Ferrari_488_GT3_Evo_2020,
        Ginetta_G55_GT4_2012,
        Honda_NSX_GT3_2017,
        Honda_NSX_GT3_Evo_2019,
        KTM_Xbow_GT2_2021,
        KTM_Xbow_GT4_2016,
        Lexus_RCF_GT3_2016,
        Lamborghini_Huracan_GT3_2015,
        Lamborghini_Huracan_GT3_Evo_2019,
        Lamborghini_Huracan_GT3_Evo2_2023,
        Lamborghini_Huracan_ST_2015,
        Lamborghini_Huracan_ST_Evo2_2021,
        Lamborghini_Gallardo_G3_Reiter_2017,
        Maserati_Gran_Turismo_MC_GT4_2016,
        Maserati_GT2_2023,
        McLaren_570s_GT4_2016,
        McLaren_650S_GT3_2015,
        McLaren_720S_GT3_2019,
        McLaren_720s_GT3_Evo_2023,
        Mercedes_AMG_GT2_2023,
        Mercedes_AMG_GT3_2015,
        Mercedes_AMG_GT3_Evo_2020,
        Mercedes_AMG_GT4_2016,
        Nissan_GT_R_Nismo_GT3_2015,
        Nissan_GT_R_Nismo_GT3_2018,
        Porsche_718_Cayman_GT4_MR_2019,
        Porsche_935_2019,
        Porsche_991_II_GT2_RS_CS_EVO_2023,
        Porsche_991_II_GT3_Cup_2017,
        Porsche_911_II_GT3_R_2019,
        Porsche_911_GT3_R_2018,
        Porsche_992_GT3_Cup_2021,
        Porsche_992_GT3_R_2023
    }

    private static readonly Dictionary<CarModels, ISetupChanger> Changers = new()
    {
        // GT3
        {Aston_Martin_V8_Vantage_GT3_2019, new AMRV8VantageGT3() },
        {Aston_Martin_Vantage_V12_GT3_2013, new AMRV12VantageGT3() },
        //{Audi_R8_LMS_2015, new AudiR8LMS() },
        //{Audi_R8_LMS_Evo_2019, new AudiR8LMSevo() },
        //{Audi_R8_LMS_Evo_II_2022, new AudiR8LMSevoII() },
        //{Bentley_Continental_GT3_2015, new BentleyContinentalGT3_2015() },
        //{Bentley_Continental_GT3_2018, new BentleyContinentalGT3_2018() },
        //{BMW_M4_GT3_2021, new BmwM4GT3() },
        //{BMW_M6_GT3_2017, new BmwM6GT3_2017() } ,
        //{Emil_Frey_Jaguar_G3_2021, new JaguarG3GT3() },
        //{Ferrari_488_GT3_2018, new Ferrari488GT3() },
        //{Ferrari_488_GT3_Evo_2020, new Ferrari488GT3evo() },
        //{Honda_NSX_GT3_2017, new HondaNsxGT3() },
        //{Honda_NSX_GT3_Evo_2019, new HondaNsxGT3Evo() },
        //{Lamborghini_Gallardo_G3_Reiter_2017, new LamborghiniGallardoG3Reiter_2017() },
        //{Lamborghini_Huracan_GT3_2015, new LamborghiniHuracanGT3() },
        //{Lamborghini_Huracan_GT3_Evo_2019, new LamborghiniHuracanGT3evo() },
        //{Lamborghini_Huracan_ST_2015, new LamborghiniHuracanST_2015() },
        //{Lamborghini_Huracan_ST_Evo2_2021, new LamborghiniHuracanSTEvo22021() },
        //{Lexus_RCF_GT3_2016, new LexusRcfGT3() },
        //{McLaren_650S_GT3_2015, new Mclaren650sGT3_2015() },
        //{McLaren_720S_GT3_2019, new Mclaren720sGT3() },
        //{Mercedes_AMG_GT3_2015, new MercedesAMGGT3_2015() },
        //{Mercedes_AMG_GT3_Evo_2020, new MercedesAMGGT3evo() },
        //{Nissan_GT_R_Nismo_GT3_2015, new NissanGtrGT3_2015() },
        //{Nissan_GT_R_Nismo_GT3_2018, new NissanGtrGT3_2018() },
        {Porsche_911_II_GT3_R_2019, new Porsche911IIGT3R() },
        {Porsche_992_GT3_R_2023, new Porsche992GT3R() },
        //{Porsche_911_GT3_R_2018, new Porsche991GT3R() },
        
        // GT4
        {Alpine_A110_GT4_2018, new AlpineA110GT4() },
        //{Aston_Martin_Vantage_AMR_GT4_2018, new AMRV8VantageGT4() },
        //{Audi_R8_LMS_GT4_2016, new AudiR8GT4() },
        //{BMW_M4_GT4_2018, new BMWM4GT4() },
        //{Chevrolet_Camaro_GT4_R_2017, new ChevroletCamaroGT4R() },
        //{Ginetta_G55_GT4_2012, new GinettaG55GT4() },
        //{KTM_Xbow_GT4_2016, new KTMXbowGT4() },
        //{Maserati_Gran_Turismo_MC_GT4_2016, new MaseratiMCGT4() },
        //{McLaren_570s_GT4_2016, new Mclaren570SGT4() },
        //{Mercedes_AMG_GT4_2016, new MercedesAMGGT4() },
        //{Porsche_718_Cayman_GT4_MR_2019, new Porsche718CaymanGT4MR() },

        // GTC 
        //{Porsche_991_II_GT3_Cup_2017, new Porsche991IIGT3Cup_2017() },
        //{Porsche_992_GT3_Cup_2021, new Porsche992GT3Cup_2021() },
        //{Ferrari_488_Challenge_Evo_2020, new Ferrari488ChallengeEvo() },

        // TCX
        {BMW_M2_Cup_2020, new BMWM2CSRacing() }
    };

    public static ISetupChanger GetChanger(CarModels model)
    {
        bool found = Changers.TryGetValue(model, out var changer);
        if (found) return changer;
        return null;
    }

    private static readonly Dictionary<CarModels, ICarSetupConversion> Conversions = new()
    {
        // GT3
        {Aston_Martin_V8_Vantage_GT3_2019, new AMRV8VantageGT3() },
        {Aston_Martin_Vantage_V12_GT3_2013, new AMRV12VantageGT3() },
        {Audi_R8_LMS_2015, new AudiR8LMS() },
        {Audi_R8_LMS_Evo_2019, new AudiR8LMSevo() },
        {Audi_R8_LMS_Evo_II_2022, new AudiR8LMSevoII() },
        {Bentley_Continental_GT3_2015, new BentleyContinentalGT3_2015() },
        {Bentley_Continental_GT3_2018, new BentleyContinentalGT3_2018() },
        {BMW_M4_GT3_2021, new BmwM4GT3() },
        {BMW_M6_GT3_2017, new BmwM6GT3_2017() } ,
        {Emil_Frey_Jaguar_G3_2021, new JaguarG3GT3() },
        {Ferrari_296_GT3_2023, new Ferrari296GT3() },
        {Ferrari_488_GT3_2018, new Ferrari488GT3() },
        {Ferrari_488_GT3_Evo_2020, new Ferrari488GT3evo() },
        {Honda_NSX_GT3_2017, new HondaNsxGT3() },
        {Honda_NSX_GT3_Evo_2019, new HondaNsxGT3Evo() },
        {Lamborghini_Gallardo_G3_Reiter_2017, new LamborghiniGallardoG3Reiter_2017() },
        {Lamborghini_Huracan_GT3_2015, new LamborghiniHuracanGT3() },
        {Lamborghini_Huracan_GT3_Evo_2019, new LamborghiniHuracanGT3evo() },
        {Lamborghini_Huracan_GT3_Evo2_2023, new LamborghiniHuracanGT3evo2() },
        {Lamborghini_Huracan_ST_2015, new LamborghiniHuracanST_2015() },
        {Lamborghini_Huracan_ST_Evo2_2021, new LamborghiniHuracanSTEvo22021() },
        {Lexus_RCF_GT3_2016, new LexusRcfGT3() },
        {McLaren_650S_GT3_2015, new Mclaren650sGT3_2015() },
        {McLaren_720S_GT3_2019, new Mclaren720sGT3() },
        {McLaren_720s_GT3_Evo_2023, new Mclaren720sEvoGT3() },
        {Mercedes_AMG_GT3_2015, new MercedesAMGGT3_2015() },
        {Mercedes_AMG_GT3_Evo_2020, new MercedesAMGGT3evo() },
        {Nissan_GT_R_Nismo_GT3_2015, new NissanGtrGT3_2015() },
        {Nissan_GT_R_Nismo_GT3_2018, new NissanGtrGT3_2018() },
        {Porsche_911_II_GT3_R_2019, new Porsche911IIGT3R() },
        {Porsche_911_GT3_R_2018, new Porsche991GT3R() },
        {Porsche_992_GT3_R_2023, new Porsche992GT3R() },
        
        // GT4
        {Alpine_A110_GT4_2018, new AlpineA110GT4() },
        {Aston_Martin_Vantage_AMR_GT4_2018, new AMRV8VantageGT4() },
        {Audi_R8_LMS_GT4_2016, new AudiR8GT4() },
        {BMW_M4_GT4_2018, new BMWM4GT4() },
        {Chevrolet_Camaro_GT4_R_2017, new ChevroletCamaroGT4R() },
        {Ginetta_G55_GT4_2012, new GinettaG55GT4() },
        {KTM_Xbow_GT4_2016, new KTMXbowGT4() },
        {Maserati_Gran_Turismo_MC_GT4_2016, new MaseratiMCGT4() },
        {McLaren_570s_GT4_2016, new Mclaren570SGT4() },
        {Mercedes_AMG_GT4_2016, new MercedesAMGGT4() },
        {Porsche_718_Cayman_GT4_MR_2019, new Porsche718CaymanGT4MR() },

        // GTC 
        {Porsche_991_II_GT3_Cup_2017, new Porsche991IIGT3Cup_2017() },
        {Porsche_992_GT3_Cup_2021, new Porsche992GT3Cup_2021() },
        {Ferrari_488_Challenge_Evo_2020, new Ferrari488ChallengeEvo() },

        // GT2
        {Audi_R8_LMS_GT2_2021, new AudiR8LMS_GT2_2021() },
        {KTM_Xbow_GT2_2021, new KtmXbowGT2_2021() },
        {Maserati_GT2_2023, new MaseratiGT2_2023() },
        {Mercedes_AMG_GT2_2023, new MercedesAmgGT2_2023() },

        // TCX
        {BMW_M2_Cup_2020, new BMWM2CSRacing() }
    };

    public static ICarSetupConversion GetConversion(CarModels model)
    {
        bool found = Conversions.TryGetValue(model, out var conversion);
        if (found) return conversion;
        return null;
    }

    public static List<string> GetAllNamesByClass(CarClasses carClass)
    {
        List<string> classNames = [];
        Conversions.ToList().ForEach(x =>
        {
            if (x.Value.CarClass == carClass)
            {
                classNames.Add(CarModelToCarName[x.Value.CarModel]);
            }
        });

        return classNames;
    }

    public static readonly Dictionary<CarModels, string> CarModelToCarName = new() {
        {None, "Unknown car model" },
        {Alpine_A110_GT4_2018, "Alpine A110 GT4 2018" },
        {Aston_Martin_V8_Vantage_GT3_2019, "Aston Martin V8 Vantage GT3 2019" },
        {Aston_Martin_Vantage_AMR_GT4_2018, "Aston Martin Vantage AMR GT4 2018" },
        {Aston_Martin_Vantage_V12_GT3_2013, "Aston Martin Vantage V12 GT3 2013"},
        {Audi_R8_LMS_GT2_2021, "Audi R8 LMS GT2 2021" },
        {Audi_R8_LMS_2015, "Audi R8 LMS 2015" },
        {Audi_R8_LMS_Evo_2019, "Audi R8 LMS Evo 2019" },
        {Audi_R8_LMS_Evo_II_2022, "Audi R8 LMS Evo II 2022" },
        {Audi_R8_LMS_GT4_2016, "Audi R8 LMS GT4 2016" },
        {Bentley_Continental_GT3_2015, "Bentley Continental GT3 2015" },
        {Bentley_Continental_GT3_2018, "Bentley Continental GT3 2018" },
        {BMW_M4_GT3_2021, "BMW M4 GT3 2021" },
        {BMW_M4_GT4_2018, "BMW M4 GT4 2018" },
        {BMW_M6_GT3_2017, "BMW M6 GT3 2017" },
        {BMW_M2_Cup_2020, "BMW M2 Cup 2020" },
        {Chevrolet_Camaro_GT4_R_2017, "Chevrolet Camaro GT4 R 2017"},
        {Emil_Frey_Jaguar_G3_2021, "Emil Frey Jaguar G3 2012" },
        {Ferrari_296_GT3_2023, "Ferrari 296 GT3 2023" },
        {Ferrari_488_Challenge_Evo_2020, "Ferrari 488 Challenge Evo 2020" },
        {Ferrari_488_GT3_2018, "Ferrari 488 GT3 2018" },
        {Ferrari_488_GT3_Evo_2020, "Ferrari 488 GT3 Evo 2020" },
        {Ginetta_G55_GT4_2012, "Ginetta G55 GT4 2012" },
        {Honda_NSX_GT3_2017, "Honda NSX GT3 2017" },
        {Honda_NSX_GT3_Evo_2019, "Honda NSX GT3 Evo 2019" },
        {KTM_Xbow_GT2_2021, "KTM X-BOW GT2 2021" },
        {KTM_Xbow_GT4_2016, "KTM Xbow GT4 2016" },
        {Lexus_RCF_GT3_2016, "Lexus RCF GT3 2016" },
        {Lamborghini_Gallardo_G3_Reiter_2017, "Lamborghini Gallardo G3 Reiter 2017" },
        {Lamborghini_Huracan_GT3_2015, "Lamborghini Huracán GT3 2015" },
        {Lamborghini_Huracan_GT3_Evo_2019, "Lamborghini Huracán GT3 Evo 2019" },
        {Lamborghini_Huracan_GT3_Evo2_2023, "Lamborghini Huracán GT3 Evo2 2023" },
        {Lamborghini_Huracan_ST_2015, "Lamborghini Huracán ST 2015" },
        {Lamborghini_Huracan_ST_Evo2_2021, "Lamborghini Huracán ST Evo2 2021" },
        {Maserati_Gran_Turismo_MC_GT4_2016, "Maserati Gran Turismo MC GT4 2016" },
        {Maserati_GT2_2023, "Maserati GT2 2023" },
        {McLaren_570s_GT4_2016, "McLaren 570s GT4 2016"},
        {McLaren_650S_GT3_2015, "McLaren 650S GT3 2015" },
        {McLaren_720S_GT3_2019, "McLaren 720S GT3 2019" },
        {McLaren_720s_GT3_Evo_2023, "McLaren 720S GT3 Evo 2023" },
        {Mercedes_AMG_GT2_2023, "Mercedes-AMG GT2 2023" },
        {Mercedes_AMG_GT3_2015, "Mercedes-AMG GT3 2015"},
        {Mercedes_AMG_GT3_Evo_2020, "Mercedes-AMG GT3 2020"},
        {Mercedes_AMG_GT4_2016, "Mercedes AMG GT4 2016"},
        {Nissan_GT_R_Nismo_GT3_2015, "Nissan GT-R Nismo GT3 2015" },
        {Nissan_GT_R_Nismo_GT3_2018, "Nissan GT-R Nismo GT3 2018" },
        {Porsche_718_Cayman_GT4_MR_2019, "Porsche 718 Cayman GT4 MR 2019" },
        {Porsche_935_2019, "Porsche 935 2019" },
        {Porsche_991_II_GT2_RS_CS_EVO_2023, "Porsche 991 II GT2 RS CS Evo 2023" },
        {Porsche_991_II_GT3_Cup_2017, "Porsche 911 II GT3 Cup 2017" },
        {Porsche_911_II_GT3_R_2019, "Porsche 911 II GT3 R 2019" },
        {Porsche_911_GT3_R_2018, "Porsche 911 GT3 R 2018" },
        {Porsche_992_GT3_Cup_2021, "Porsche 992 GT3 Cup 2021" },
        {Porsche_992_GT3_R_2023, "Porsche 992 GT3 R 2023" }
    };

    public static string GetNameFromCarModel(CarModels model)
    {
        string name;
        CarModelToCarName.TryGetValue(model, out name);
        return name;
    }

    public static readonly Dictionary<string, CarModels> ParseNames = new()
    {
        {"alpine_a110_gt4", Alpine_A110_GT4_2018 },
        {"amr_v8_vantage_gt3", Aston_Martin_V8_Vantage_GT3_2019 },
        {"amr_v8_vantage_gt4", Aston_Martin_Vantage_AMR_GT4_2018 },
        {"amr_v12_vantage_gt3", Aston_Martin_Vantage_V12_GT3_2013 },
        {"audi_r8_lms_gt2", Audi_R8_LMS_GT2_2021 },
        {"audi_r8_gt4", Audi_R8_LMS_GT4_2016 },
        {"audi_r8_lms", Audi_R8_LMS_2015 },
        {"audi_r8_lms_evo", Audi_R8_LMS_Evo_2019 },
        {"audi_r8_lms_evo_ii", Audi_R8_LMS_Evo_II_2022 },
        {"bentley_continental_gt3_2016", Bentley_Continental_GT3_2015 },
        {"bentley_continental_gt3_2018", Bentley_Continental_GT3_2018 },
        {"bmw_m2_cs_racing", BMW_M2_Cup_2020 },
        {"bmw_m4_gt3", BMW_M4_GT3_2021 },
        {"bmw_m4_gt4", BMW_M4_GT4_2018 },
        {"bmw_m6_gt3", BMW_M6_GT3_2017 },
        {"chevrolet_camaro_gt4r", Chevrolet_Camaro_GT4_R_2017 },
        {"ferrari_296_gt3", Ferrari_296_GT3_2023 },
        {"ferrari_488_challenge_evo", Ferrari_488_Challenge_Evo_2020 },
        {"ferrari_488_gt3", Ferrari_488_GT3_2018 },
        {"ferrari_488_gt3_evo", Ferrari_488_GT3_Evo_2020 },
        {"ginetta_g55_gt4", Ginetta_G55_GT4_2012 },
        {"honda_nsx_gt3", Honda_NSX_GT3_2017 },
        {"honda_nsx_gt3_evo", Honda_NSX_GT3_Evo_2019 },
        {"jaguar_g3", Emil_Frey_Jaguar_G3_2021 },
        {"ktm_xbow_gt2", KTM_Xbow_GT2_2021 },
        {"ktm_xbow_gt4", KTM_Xbow_GT4_2016 },
        {"lamborghini_gallardo_rex", Lamborghini_Gallardo_G3_Reiter_2017 },
        {"lamborghini_huracan_gt3", Lamborghini_Huracan_GT3_2015 },
        {"lamborghini_huracan_gt3_evo", Lamborghini_Huracan_GT3_Evo_2019 },
        {"lamborghini_huracan_gt3_evo2", Lamborghini_Huracan_GT3_Evo2_2023 },
        {"lamborghini_huracan_st", Lamborghini_Huracan_ST_2015 },
        {"lamborghini_huracan_st_evo2", Lamborghini_Huracan_ST_Evo2_2021 },
        {"lexus_rc_f_gt3", Lexus_RCF_GT3_2016 },
        {"maserati_mc20_gt2", Maserati_GT2_2023 },
        {"maserati_mc_gt4", Maserati_Gran_Turismo_MC_GT4_2016 },
        {"mclaren_570s_gt4", McLaren_570s_GT4_2016 },
        {"mclaren_650s_gt3", McLaren_650S_GT3_2015 },
        {"mclaren_720s_gt3", McLaren_720S_GT3_2019 },
        {"mclaren_720s_gt3_evo", McLaren_720s_GT3_Evo_2023 },
        {"mercedes_amg_gt2", Mercedes_AMG_GT2_2023 },
        {"mercedes_amg_gt3", Mercedes_AMG_GT3_2015 },
        {"mercedes_amg_gt3_evo", Mercedes_AMG_GT3_Evo_2020 },
        {"mercedes_amg_gt4", Mercedes_AMG_GT4_2016 },
        {"nissan_gt_r_gt3_2017", Nissan_GT_R_Nismo_GT3_2015 }, // yes the parsename is 2017 and not 2015.. (kunos feature)
        {"nissan_gt_r_gt3_2018", Nissan_GT_R_Nismo_GT3_2018 },
        {"porsche_718_cayman_gt4_mr", Porsche_718_Cayman_GT4_MR_2019 },
        {"porsche_935", Porsche_935_2019 },
        {"porsche_991_gt2_rs_mr", Porsche_991_II_GT2_RS_CS_EVO_2023 },
        {"porsche_991_gt3_r", Porsche_911_GT3_R_2018 },
        {"porsche_991ii_gt3_cup", Porsche_991_II_GT3_Cup_2017 },
        {"porsche_991ii_gt3_r", Porsche_911_II_GT3_R_2019 },
        {"porsche_992_gt3_cup", Porsche_992_GT3_Cup_2021 },
        {"porsche_992_gt3_r", Porsche_992_GT3_R_2023 },
    };

    public static CarModels ParseCarName(string parseName)
    {
        if (ParseNames.ContainsKey(parseName))
            return ParseNames[parseName];
        else return None;
    }

    public static readonly Dictionary<int, CarModels> IdsToCarModel = new()
    {
        {0, Porsche_911_GT3_R_2018 },
        {1, Mercedes_AMG_GT3_2015 },
        {2, Ferrari_488_GT3_2018 },
        {3, Audi_R8_LMS_2015 },
        {4, Lamborghini_Huracan_GT3_2015 },
        {5, McLaren_650S_GT3_2015 },
        {6, Nissan_GT_R_Nismo_GT3_2018 },
        {7, BMW_M6_GT3_2017 },
        {8, Bentley_Continental_GT3_2018 },
        {9, Porsche_991_II_GT3_Cup_2017 },
        {10, Nissan_GT_R_Nismo_GT3_2015 },
        {11, Bentley_Continental_GT3_2015 },
        {12, Aston_Martin_Vantage_V12_GT3_2013 },
        {13, Lamborghini_Gallardo_G3_Reiter_2017 },
        {14, Emil_Frey_Jaguar_G3_2021 },
        {15, Lexus_RCF_GT3_2016 },
        {16, Lamborghini_Huracan_GT3_Evo_2019 },
        {17, Honda_NSX_GT3_2017 },
        {18, Lamborghini_Huracan_ST_2015 },
        {19, Audi_R8_LMS_Evo_2019 },
        {20, Aston_Martin_V8_Vantage_GT3_2019 },
        {21, Honda_NSX_GT3_Evo_2019 },
        {22, McLaren_720S_GT3_2019 },
        {23, Porsche_911_II_GT3_R_2019 },
        {24, Ferrari_488_GT3_Evo_2020 },
        {25, Mercedes_AMG_GT3_Evo_2020 },
        {26, Ferrari_488_Challenge_Evo_2020 },
        {27, BMW_M2_Cup_2020 },
        {28, Porsche_992_GT3_Cup_2021 },
        {29, Lamborghini_Huracan_ST_Evo2_2021 },
        {30, BMW_M4_GT3_2021 },
        {31, Audi_R8_LMS_Evo_II_2022 },
        {32, Ferrari_296_GT3_2023 },
        {33, Lamborghini_Huracan_GT3_Evo2_2023 },
        {34, Porsche_992_GT3_R_2023 },
        {35, McLaren_720s_GT3_Evo_2023 },
        {50, Alpine_A110_GT4_2018 },
        {51, Aston_Martin_Vantage_AMR_GT4_2018 },
        {52, Audi_R8_LMS_GT4_2016 },
        {53, BMW_M4_GT4_2018 },
        {55, Chevrolet_Camaro_GT4_R_2017 },
        {56, Ginetta_G55_GT4_2012 },
        {57, KTM_Xbow_GT4_2016 },
        {58, Maserati_Gran_Turismo_MC_GT4_2016 },
        {59, McLaren_570s_GT4_2016 },
        {60, Mercedes_AMG_GT4_2016 },
        {61, Porsche_718_Cayman_GT4_MR_2019 },
        {80, Audi_R8_LMS_GT2_2021 },
        {82, KTM_Xbow_GT2_2021 },
        {83, Maserati_GT2_2023 },
        {84, Mercedes_AMG_GT2_2023 },
        {85, Porsche_991_II_GT2_RS_CS_EVO_2023 },
        {86, Porsche_935_2019 },
    };

    public static CarModels GetCarModels(int carId)
    {
        if (IdsToCarModel.ContainsKey(carId))
        {
            return IdsToCarModel[carId];
        }
        return None;
    }

    public static string GetCarName(int carId)
    {
        if (IdsToCarModel.ContainsKey(carId))
        {
            CarModels model = IdsToCarModel[carId];
            if (CarModelToCarName.ContainsKey(model))
                return CarModelToCarName[model];
        }

        return $"Unknown: {carId}";
    }

    public static string GetCarName(string carModelName)
    {
        var model = ParseCarName(carModelName);

        if (CarModelToCarName.ContainsKey(model))
            return CarModelToCarName[model];

        return $"Unknown car";
    }

    public static Root GetSetupJsonRoot(FileInfo jsonFile)
    {
        string jsonString = string.Empty;
        try
        {
            using (FileStream fileStream = jsonFile.OpenRead())
            {
                using (StreamReader reader = new(fileStream))
                {
                    jsonString = reader.ReadToEnd();
                    reader.Close();
                    fileStream.Close();
                }
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return null;
        }

        Root setup = JsonConvert.DeserializeObject<Root>(jsonString);
        return setup;
    }

    public static Root GetSetupJsonRoot(string file)
    {
        FileInfo jsonFile = new(file);
        if (!jsonFile.Exists)
            return null;

        return GetSetupJsonRoot(jsonFile);
    }
}
