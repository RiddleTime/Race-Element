using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.Telemetry.SharedMemory;
using ACCSetupApp.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayStaticInfo
{
    internal class StaticInfoOverlay : AbstractOverlay
    {
        private Font inputFont = new Font("Arial", 13);

        public StaticInfoOverlay(Rectangle rectangle) : base(rectangle)
        {
            this.Width = 275;
            this.Height = 390;
        }

        public override void BeforeStart()
        {
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            int y = 0;
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = ReflectionUtil.FieldTypeValue(member, value);

                    g.DrawString($"{member.Name}: {value}", inputFont, Brushes.White, 0, y += (int)inputFont.Size + 1);
                }
            }
        }

        public override bool ShouldRender()
        {
            return pageStatic.AssettoCorsaVersion != String.Empty;
        }
    }
}
