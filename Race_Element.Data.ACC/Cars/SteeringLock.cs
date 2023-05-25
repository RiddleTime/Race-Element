using System.Collections.Generic;

namespace RaceElement.Data.ACC.Cars
{
    public class SteeringLock
    {
        private static readonly Dictionary<string, int> _steeringLocks = new Dictionary<string, int>()
        {
            // CUP
            {"porsche_991ii_gt3_cup", 800},
            {"porsche_992_gt3_cup", 540},
            // ST
            {"lamborghini_huracan_st", 620},
            {"lamborghini_huracan_st_evo2", 620},
            // CHL
            {"ferrari_488_challenge_evo", 480},
            // TCX
            {"bmw_m2_cs_racing", 360},
            // GT3
            {"amr_v12_vantage_gt3", 640},
            {"amr_v8_vantage_gt3", 640},
            {"audi_r8_lms", 720},
            {"audi_r8_lms_evo", 720},
            {"audi_r8_lms_evo_ii", 720},
            {"bentley_continental_gt3_2016", 640},
            {"bentley_continental_gt3_2018", 640},
            {"bmw_m4_gt3", 516},
            {"bmw_m6_gt3", 566},
            {"jaguar_g3", 720},
            {"ferrari_296_gt3", 800 },
            {"ferrari_488_gt3", 480},
            {"ferrari_488_gt3_evo", 480},
            {"honda_nsx_gt3", 620},
            {"honda_nsx_gt3_evo", 436},
            {"lamborghini_huracan_gt3", 620},
            {"lamborghini_huracan_gt3_evo", 620},
            {"lamborghini_huracan_gt3_evo2", 620 },
            {"lexus_rc_f_gt3", 640},
            {"mclaren_650s_gt3", 480},
            {"mclaren_720s_gt3", 480},
            {"mclaren_720s_gt3_evo", 480 },
            {"mercedes_amg_gt3", 640},
            {"mercedes_amg_gt3_evo", 640},
            {"nissan_gt_r_gt3_2017", 640},
            {"nissan_gt_r_gt3_2018", 640},
            {"porsche_991_gt3_r", 800},
            {"porsche_991ii_gt3_r", 800},
            {"porsche_992_gt3_r", 800 },
            {"lamborghini_gallardo_rex", 720},
            // GT4
            {"alpine_a110_gt4", 720},
            {"amr_v8_vantage_gt4", 640},
            {"audi_r8_gt4", 720},
            {"bmw_m4_gt4", 492},
            {"chevrolet_camaro_gt4r", 720},
            {"ginetta_g55_gt4", 720},
            {"ktm_xbow_gt4", 582},
            {"maserati_mc_gt4", 900},
            {"mclaren_570s_gt4", 480},
            {"mercedes_amg_gt4", 492},
            {"porsche_718_cayman_gt4_mr", 800},
        };

        /// <summary>
        /// Returns the steering lock for the given car model, if no car model has been found, returns 360
        /// </summary>
        /// <param name="carModel"></param>
        /// <returns></returns>
        public static int Get(string carModel)
        {
            if (_steeringLocks.TryGetValue(carModel, out int steeringLock))
                return steeringLock;

            return 360;
        }

    }
}
