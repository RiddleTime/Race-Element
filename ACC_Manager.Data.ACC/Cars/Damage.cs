using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.ACCSharedMemory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Data.ACC.Cars
{
    public class Damage
    {
        private const float MagicDamageMultiplier = 0.282f;

        public static float GetTotalRepairTime(SPageFilePhysics pagePhysics)
        {
            float totalRepairTime = 0;

            totalRepairTime += GetBodyWorkDamage(pagePhysics, CarDamagePosition.Centre);

            foreach (Wheel wheel in Enum.GetValues(typeof(Wheel)))
                totalRepairTime += GetSuspensionDamage(pagePhysics, wheel);

            return totalRepairTime;
        }

        public static bool HasAnyDamage(SPageFilePhysics pagePhysics)
        {
            foreach (int i in Enum.GetValues(typeof(CarDamagePosition)))
                if (pagePhysics.CarDamage[i] > 0)
                    return true;

            foreach (int i in Enum.GetValues(typeof(Wheel)))
                if (pagePhysics.SuspensionDamage[i] > 0)
                    return true;

            return false;
        }

        /// <summary>
        /// Gets the amount of damage/repair-time for the given wheel
        /// </summary>
        /// <param name="wheel"></param>
        /// <returns></returns>
        public static float GetSuspensionDamage(SPageFilePhysics pagePhysics, Wheel wheel)
        {
            return pagePhysics.SuspensionDamage[(int)wheel] * 30;
        }

        /// <summary>
        /// Gets the amount of bodywork damage/repair-time for the given car damage position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public static float GetBodyWorkDamage(SPageFilePhysics pagePhysics, CarDamagePosition position)
        {
            return pagePhysics.CarDamage[(int)position] * MagicDamageMultiplier;
        }

        public enum CarDamagePosition : int
        {
            Front,
            Rear,
            Left,
            Right,
            Centre
        }
    }
}
