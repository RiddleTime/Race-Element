using System.Drawing;
using System.Drawing.Drawing2D;
using ACCManager.Data.ACC.Cars;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;

namespace ACCManager.HUD.ACC.Overlays.OverlayDamage
{
    [Overlay(Name = "Damage", Version = 1.00, OverlayType = OverlayType.Release,
        Description = "Shows relevant information about damage on your car.")]
    internal sealed class DamageOverlay : AbstractOverlay
    {
        private readonly DamageConfiguration _config = new DamageConfiguration();
        private class DamageConfiguration : OverlayConfiguration
        {
            public DamageConfiguration()
            {
                AllowRescale = true;
            }
        }

        private readonly Font _font;
        private const int OriginalWidth = 100;
        private const int OriginalHeight = 200;

        private CachedBitmap _carOutline;
        private CachedBitmap _suspensionDamage;
        private CachedBitmap _bodyDamage;

        private float _damageTime = 0;

        public DamageOverlay(Rectangle rectangle) : base(rectangle, "Damage")
        {
            this.RefreshRateHz = 2;

            _font = FontUtil.FontUnispace(10 * this.Scale);
            this.Width = OriginalWidth;
            this.Height = OriginalHeight;
        }

        public override bool ShouldRender() => DefaultShouldRender();

        public override void BeforeStart()
        {
            int scaledWidth = (int)(this.Width * this.Scale);
            int scaledHeight = (int)(this.Height * this.Scale);
            _carOutline = new CachedBitmap(scaledWidth, scaledHeight, g =>
            {
                GraphicsPath path = new GraphicsPath();

                int horizontalPadding = (int)(scaledWidth * 0.05);
                int verticalPadding = (int)(scaledHeight * 0.025);

                path.AddRectangle(new Rectangle(horizontalPadding, verticalPadding, scaledWidth - horizontalPadding * 2, scaledHeight - verticalPadding * 2));

                g.DrawPath(Pens.White, path);

            });

            _suspensionDamage = new CachedBitmap(scaledWidth, scaledHeight, g => { });
            _bodyDamage = new CachedBitmap(scaledWidth, scaledHeight, g => { });
        }

        public override void BeforeStop()
        {
            _carOutline.Dispose();
            _suspensionDamage.Dispose();
            _bodyDamage.Dispose();
        }

        public override void Render(Graphics g)
        {
            float newDamageTime = Damage.GetTotalRepairTime(pagePhysics);
            if (newDamageTime != _damageTime)
            {
                _damageTime = newDamageTime;
                UpdateSuspensionDamage();
                UpdateBodyDamage();
            }

            _carOutline?.Draw(g, 0, 0, OriginalWidth, OriginalHeight);
            _suspensionDamage?.Draw(g, 0, 0, OriginalWidth, OriginalHeight);
            _bodyDamage?.Draw(g, 0, 0, OriginalWidth, OriginalHeight);
        }

        private void UpdateSuspensionDamage()
        {
            int scaledWidth = (int)(OriginalWidth * this.Scale);
            int scaledHeight = (int)(OriginalHeight * this.Scale);
            int horizontalPadding = (int)(scaledWidth * 0.1);
            int verticalPadding = (int)(scaledHeight * 0.1);

            _suspensionDamage?.SetRenderer(g =>
            {
                string text = "23.3";
                SizeF textWidth = g.MeasureString(text, _font);

                g.DrawStringWithShadow(text, _font, Brushes.White, new PointF(scaledWidth / 2 - textWidth.Width / 2, verticalPadding));
            });
        }

        private void UpdateBodyDamage()
        {
            _bodyDamage?.SetRenderer(g =>
            {
                float bodyDamageFront = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Front);
                float bodyDamageRear = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Rear);
                float bodyDamageLeft = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Left);
                float bodyDamageRight = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Right);
                float bodyDamageCentre = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Centre);


                

            });
        }
    }
}
