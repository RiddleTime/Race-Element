using ACCSetupApp.Controls.HUD.Overlay.Internal;
using ACCSetupApp.Controls.HUD.Overlay.OverlayDebugInfo;
using ACCSetupApp.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ACCSetupApp.Controls.HUD.Overlay.OverlayGraphicsInfo
{
    internal class GraphicsInfoOverlay : AbstractOverlay
    {
        private Font inputFont = new Font("Arial", 10);

        public GraphicsInfoOverlay(Rectangle rectangle) : base(rectangle)
        {
            this.Width = 275;
            this.Height = 1030;

            DebugInfoHelper.Instance.WidthChanged += (sender, args) =>
            {
                if (args)
                {
                    this.X = DebugInfoHelper.Instance.GetX(this);
                }
            };
        }

        public override void BeforeStart()
        {
            DebugInfoHelper.Instance.AddOverlay(this);
            this.X = DebugInfoHelper.Instance.GetX(this);
        }

        public override void BeforeStop()
        {
            DebugInfoHelper.Instance.RemoveOverlay(this);
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(new SolidBrush(System.Drawing.Color.FromArgb(140, 0, 0, 0)), new Rectangle(0, 0, this.Width, this.Height));

            int xMargin = 5;
            int y = 0;
            FieldInfo[] members = pageGraphics.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageGraphics);
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = ReflectionUtil.FieldTypeValue(member, value);

                    g.DrawString($"{member.Name}: {value}", inputFont, Brushes.White, 0 + xMargin, y);
                    y += (int)inputFont.Size + 2;
                }
            }
        }

        public override bool ShouldRender()
        {
            return true;
        }
    }
}
