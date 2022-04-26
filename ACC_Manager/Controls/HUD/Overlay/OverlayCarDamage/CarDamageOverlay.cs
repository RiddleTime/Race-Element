using ACCSetupApp.Controls.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCSetupApp.SetupParser.SetupConverter;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayCarDamage
{
    internal class CarDamageOverlay : AbstractOverlay
    {
        private float MagicDamageMultiplier = 0.282f;

        public CarDamageOverlay(Rectangle rectangle) : base(rectangle)
        {
        }

        public override void BeforeStart()
        {
            throw new NotImplementedException();
        }

        public override void BeforeStop()
        {
            throw new NotImplementedException();
        }

        public override void Render(Graphics g)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldRender()
        {
            return HasAnyDamage();
        }

        private bool HasAnyDamage()
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
        private float GetSuspensionDamage(Wheel wheel)
        {
            return pagePhysics.SuspensionDamage[(int)wheel] * 30;
        }

        /// <summary>
        /// Gets the amount of bodywork damage/repair-time for the given car damage position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private float GetBodyWorkDamage(CarDamagePosition position)
        {
            return pagePhysics.CarDamage[(int)position] * MagicDamageMultiplier;
        }

        private enum CarDamagePosition : int
        {
            Front,
            Rear,
            Left,
            Right,
            Centre
        }
    }
}
