using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.HUD.ACC.Overlays.OverlayCornerNames
{
    [Overlay(Name = "Corner Names", Description = "Shows corner and sector names for each track.", OverlayType = OverlayType.Release, Version = 1.00)]
    internal class CornerNamesOverlay : AbstractOverlay
    {
        struct FloatRange
        {
            float From;
            float To;

            public FloatRange(float from, float to)
            {
                From = from;
                To = to;
            }

            public bool IsInRange(float value)
            {
                return value >= From && value <= To;
            }
        }

        Dictionary<FloatRange, string> cornerNames = new Dictionary<FloatRange, string>();

        private CachedBitmap _cachedBackground;
        private Font _font;

        public CornerNamesOverlay(Rectangle rectangle) : base(rectangle, "Corner Names")
        {

        }

        public override void BeforeStart()
        {
            _font = FontUtil.FontOrbitron(15);

            this.Height = (int)(_font.Height * 1.5);
            this.Width = 300;

            cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            cornerNames.Add(new FloatRange(0.1435904f, 0.1541221f), "Eau Rouge");
            cornerNames.Add(new FloatRange(0.1541221f, 0.1960247f), "Raidillon");


            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");
            //cornerNames.Add(new FloatRange(0.04729626f, 0.05776058f), "La Source");

            //        Kemmel
            //        0.225293 - 0.2399776
            //        Les Combes
            //        0.3361526 - 0.3628373
            //        Malmedy
            //        0.3697942 - 0.3857344
            //        Bruxelles
            //        0.4207456 - 04480728
            //        Speaker Corner
            //        0.463365 - 0.4780263
            //        Pouhon
            //        0.5346055 - 0.5958407
            //        Pif Paf Fagnes
            //        0.631867 - 06752521
            //        Campus
            //        0.6973619 - 0.7152601
            //        Stavelot
            //        0.7278681 - 0.7532634
            //        Courbes Freres
            //        0.785664 - 0.8633262
            //        Blanchimont
            //        0.8780463 - 0.8961564
            //        Chicane Arrêt Bus
            //        0.9576367 - 0.9743167
        }

        public override void BeforeStop()
        {
        }

        public override void Render(Graphics g)
        {
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, Width, Height));

            foreach (var corner in cornerNames)
            {
                if (corner.Key.IsInRange(pageGraphics.NormalizedCarPosition))
                {
                    float textWidht = g.MeasureString(corner.Value, _font).Width;

                    PointF location = new PointF(Width / 2 - textWidht / 2, _font.GetHeight() / 3);

                    g.DrawStringWithShadow(corner.Value, _font, Brushes.White, location);
                }
            }
        }

        public override bool ShouldRender() => DefaultShouldRender();
    }
}
