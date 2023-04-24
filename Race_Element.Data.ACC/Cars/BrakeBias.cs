using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.ACC.Cars
{
    public class BrakeBias
    {
        private static readonly Dictionary<string, int> _brakeBiasOffsets = new Dictionary<string, int>()
        {
            // CUP
            {"porsche_991ii_gt3_cup", -5},
            {"porsche_992_gt3_cup", -5},
            // ST
            {"lamborghini_huracan_st", -14},
            {"lamborghini_huracan_st_evo2", -14},
            // CHL
            {"ferrari_488_challenge_evo", -13},
            // TCX
            {"bmw_m2_cs_racing", -17},
            // GT3
            {"amr_v12_vantage_gt3", -7},
            {"amr_v8_vantage_gt3", -7},
            {"audi_r8_lms", -14},
            {"audi_r8_lms_evo", -14},
            {"audi_r8_lms_evo_ii", -14},
            {"bentley_continental_gt3_2016", -7},
            {"bentley_continental_gt3_2018", -7},
            {"bmw_m4_gt3", -14},
            {"bmw_m6_gt3", -15},
            {"jaguar_g3", -7},
            {"ferrari_296_gt3", -5},
            {"ferrari_488_gt3", -17},
            {"ferrari_488_gt3_evo", -17},
            {"honda_nsx_gt3", -14},
            {"honda_nsx_gt3_evo", -14},
            {"lamborghini_huracan_gt3", -14},
            {"lamborghini_huracan_gt3_evo", -14},
            {"lamborghini_huracan_gt3_evo2", -14 },
            {"lexus_rc_f_gt3", -14},
            {"mclaren_650s_gt3", -17},
            {"mclaren_720s_gt3", -17},
            {"mercedes_amg_gt3", -14},
            {"mercedes_amg_gt3_evo", -14},
            {"nissan_gt_r_gt3_2017", -15},
            {"nissan_gt_r_gt3_2018", -15},
            {"porsche_991_gt3_r", -21},
            {"porsche_991ii_gt3_r", -21},
            {"porsche_992_gt3_r", -21 },
            {"lamborghini_gallardo_rex", -14},
            // GT4
            {"alpine_a110_gt4", -15},
            {"amr_v8_vantage_gt4", -20},
            {"audi_r8_gt4", -15},
            {"bmw_m4_gt4", -22},
            {"chevrolet_camaro_gt4r", -18},
            {"ginetta_g55_gt4", -18},
            {"ktm_xbow_gt4", -20},
            {"maserati_mc_gt4", -15},
            {"mclaren_570s_gt4", -9},
            {"mercedes_amg_gt4", -20},
            {"porsche_718_cayman_gt4_mr", -20},
        };


        /// <summary>
        /// Returns the dash offset to get the braking bias (based on shared memory brake bias *100)
        /// </summary>
        /// <param name="carModel"></param>
        /// <returns></returns>
        public static int Get(string carModel)
        {
            if (_brakeBiasOffsets.TryGetValue(carModel, out int brakeBiasOffset))
                return brakeBiasOffset;

            return 0;
        }
    }
}
