using System.Drawing;
using System.Drawing.Drawing2D;
using ACCManager.Data.ACC.Cars;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.OverlayUtil;
using ACCManager.HUD.Overlay.Util;
using System.IO;

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

        private class PathShape
        {
            public RectangleF Shape { get; set; }
            public GraphicsPath Path { get; set; }
            public Brush Brush { get; set; }
        }

        private readonly Font _font;
        private const int OriginalWidth = 150;
        private const int OriginalHeight = 220;

        private CachedBitmap _carOutline;
        private CachedBitmap _suspensionDamage;
        private CachedBitmap _bodyDamage;

        private PathShape _shapeBodyFront;
        private PathShape _shapeBodyRear;
        private PathShape _shapeBodyLeft;
        private PathShape _shapeBodyRight;

        private PathShape _shapeSuspensionFrontLeft;
        private PathShape _shapeSuspensionFrontRight;
        private PathShape _shapeSuspensionRearLeft;
        private PathShape _shapeSuspensionRearRight;


        private float _damageTime = 0;

        public DamageOverlay(Rectangle rectangle) : base(rectangle, "Damage")
        {
            this.RefreshRateHz = 2;

            _font = FontUtil.FontUnispace(10 * this.Scale);
            this.Width = OriginalWidth;
            this.Height = OriginalHeight;
        }

        public override bool ShouldRender() => DefaultShouldRender();

        private void CreatePathShapes()
        {
            int scaledWidth = (int)(this.Width * this.Scale) - 1;
            int scaledHeight = (int)(this.Height * this.Scale) - 1;
            float horizontalPadding = scaledWidth * 0.05f;
            float verticalPadding = scaledHeight * 0.025f;
            Color baseColor = Color.FromArgb(185, 0, 0, 0);


            //// BODY DAMAGE
            float frontRearWidth = scaledWidth - horizontalPadding * 6;
            // body shapes Front
            RectangleF bodyFront = new RectangleF(horizontalPadding + frontRearWidth / 7, verticalPadding, frontRearWidth, verticalPadding * 4);
            GraphicsPath pathBodyFront = new GraphicsPath();
            pathBodyFront.AddArc(bodyFront, 180, 180);
            _shapeBodyFront = new PathShape()
            {
                Shape = bodyFront,
                Brush = new LinearGradientBrush(bodyFront, Color.Red, Color.Transparent, LinearGradientMode.Vertical),
                Path = pathBodyFront
            };

            // body shape rear
            RectangleF bodyRear = new RectangleF(horizontalPadding + frontRearWidth / 7, scaledHeight - verticalPadding * 5, frontRearWidth, verticalPadding * 4);
            GraphicsPath pathBodyRear = new GraphicsPath();
            pathBodyRear.AddArc(bodyRear, 180, -180);
            _shapeBodyRear = new PathShape()
            {
                Shape = bodyRear,
                Brush = new LinearGradientBrush(bodyRear, Color.Transparent, baseColor, LinearGradientMode.Vertical),
                Path = pathBodyRear,
            };

            // body shapes left and right
            float bodyLeftRightWidth = horizontalPadding * 4;
            float bodyLeftRightHeight = verticalPadding * 34;

            // body shape left
            RectangleF bodyLeft = new RectangleF(0 + horizontalPadding, scaledHeight / 2 - bodyLeftRightHeight / 2, bodyLeftRightWidth, bodyLeftRightHeight);
            GraphicsPath pathBodyLeft = new GraphicsPath();
            pathBodyLeft.AddArc(bodyLeft, 90, 180);
            _shapeBodyLeft = new PathShape()
            {
                Shape = bodyLeft,
                Brush = new LinearGradientBrush(bodyLeft, baseColor, Color.Transparent, LinearGradientMode.Horizontal),
                Path = pathBodyLeft
            };

            // Body shape right
            RectangleF bodyRight = new RectangleF(scaledWidth - bodyLeftRightWidth - horizontalPadding, scaledHeight / 2 - bodyLeftRightHeight / 2, bodyLeftRightWidth, bodyLeftRightHeight);
            GraphicsPath pathBodyRight = new GraphicsPath();
            pathBodyRight.AddArc(bodyRight, -90, 180);
            _shapeBodyRight = new PathShape()
            {
                Shape = bodyRight,
                Brush = new LinearGradientBrush(bodyRight, Color.Transparent, baseColor, LinearGradientMode.Horizontal),
                Path = pathBodyRight
            };


            //// SUSPENSION DAMAGE
            float wheelWidth = verticalPadding * 5f;
            float wheelHeight = wheelWidth * 1.3f;
            float verticalWheelPadding = verticalPadding * 4;

            // wheel left front
            RectangleF wheelFrontLeft = new RectangleF(bodyLeftRightWidth / 2 + horizontalPadding * 1.5f, verticalWheelPadding, wheelWidth, wheelHeight);
            GraphicsPath pathWheelFrontLeft = new GraphicsPath();
            pathWheelFrontLeft.AddRectangle(wheelFrontLeft);

            _shapeSuspensionFrontLeft = new PathShape()
            {
                Shape = wheelFrontLeft,
                Brush = new LinearGradientBrush(wheelFrontLeft, Color.Red, Color.Transparent, LinearGradientMode.Horizontal),
                Path = pathWheelFrontLeft,
            };

            // wheel right front
            RectangleF wheelFrontRight = new RectangleF(scaledWidth - (bodyLeftRightWidth / 2 + horizontalPadding * 1.5f + wheelWidth), verticalWheelPadding, wheelWidth, wheelHeight);
            GraphicsPath pathWheelFrontRight = new GraphicsPath();
            pathWheelFrontRight.AddRectangle(wheelFrontRight);
            _shapeSuspensionFrontRight = new PathShape()
            {
                Shape = wheelFrontRight,
                Brush = new LinearGradientBrush(wheelFrontRight, Color.Transparent, baseColor, LinearGradientMode.Horizontal),
                Path = pathWheelFrontRight,
            };


            // wheel left Rear
            RectangleF wheelRearLeft = new RectangleF(bodyLeftRightWidth / 2 + horizontalPadding * 1.5f, scaledHeight - verticalWheelPadding - wheelHeight, wheelWidth, wheelHeight);
            GraphicsPath pathWheelRearLeft = new GraphicsPath();
            pathWheelRearLeft.AddRectangle(wheelRearLeft);
            _shapeSuspensionRearLeft = new PathShape()
            {
                Shape = wheelRearLeft,
                Brush = new LinearGradientBrush(wheelRearLeft, baseColor, Color.Transparent, LinearGradientMode.Horizontal),
                Path = pathWheelRearLeft,
            };

            // wheel right Rear
            RectangleF wheelRearRight = new RectangleF(scaledWidth - (bodyLeftRightWidth / 2 + horizontalPadding * 1.5f + wheelWidth), scaledHeight - verticalWheelPadding - wheelHeight, wheelWidth, wheelHeight);
            GraphicsPath pathWheelRearRight = new GraphicsPath();
            pathWheelRearRight.AddRectangle(wheelRearRight);
            _shapeSuspensionRearRight = new PathShape()
            {
                Shape = wheelRearRight,
                Brush = new LinearGradientBrush(wheelRearRight, Color.Transparent, baseColor, LinearGradientMode.Horizontal),
                Path = pathWheelRearRight,
            };


        }

        public override void BeforeStart()
        {
            CreatePathShapes();

            int scaledWidth = (int)(this.Width * this.Scale) - 1;
            int scaledHeight = (int)(this.Height * this.Scale) - 1;
            _carOutline = new CachedBitmap(scaledWidth, scaledHeight, g =>
            {
                GraphicsPath path = new GraphicsPath();

                float horizontalPadding = scaledWidth * 0.05f;
                float verticalPadding = scaledHeight * 0.025f;

                Pen bodyOutlinePen = new Pen(new SolidBrush(Color.FromArgb(185, 255, 255, 255)), 0.8f * this.Scale);

                g.FillPath(_shapeBodyFront.Brush, _shapeBodyFront.Path);
                g.DrawPath(bodyOutlinePen, _shapeBodyFront.Path);

                g.FillPath(_shapeBodyRear.Brush, _shapeBodyRear.Path);
                g.DrawPath(bodyOutlinePen, _shapeBodyRear.Path);

                g.FillPath(_shapeBodyLeft.Brush, _shapeBodyLeft.Path);
                g.DrawPath(bodyOutlinePen, _shapeBodyLeft.Path);

                g.FillPath(_shapeBodyRight.Brush, _shapeBodyRight.Path);
                g.DrawPath(bodyOutlinePen, _shapeBodyRight.Path);

                g.FillPath(_shapeSuspensionFrontLeft.Brush, _shapeSuspensionFrontLeft.Path);
                g.DrawPath(bodyOutlinePen, _shapeSuspensionFrontLeft.Path);

                g.FillPath(_shapeSuspensionFrontRight.Brush, _shapeSuspensionFrontRight.Path);
                g.DrawPath(bodyOutlinePen, _shapeSuspensionFrontRight.Path);

                g.FillPath(_shapeSuspensionRearLeft.Brush, _shapeSuspensionRearLeft.Path);
                g.DrawPath(bodyOutlinePen, _shapeSuspensionRearLeft.Path);

                g.FillPath(_shapeSuspensionRearRight.Brush, _shapeSuspensionRearRight.Path);
                g.DrawPath(bodyOutlinePen, _shapeSuspensionRearRight.Path);
            });

            _suspensionDamage = new CachedBitmap(scaledWidth, scaledHeight, g => { });
            _bodyDamage = new CachedBitmap(scaledWidth, scaledHeight, g => { });

            UpdateSuspensionDamage();
            UpdateBodyDamage();
        }

        public override void BeforeStop()
        {
            _carOutline?.Dispose();
            _suspensionDamage?.Dispose();
            _bodyDamage?.Dispose();
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
                float suspensionDamageFrontLeft = Damage.GetSuspensionDamage(pagePhysics, ACCManager.Data.SetupConverter.Wheel.FrontLeft);
                float suspensionDamageFrontRight = Damage.GetSuspensionDamage(pagePhysics, ACCManager.Data.SetupConverter.Wheel.FrontRight);
                float suspensionDamageRearLeft = Damage.GetSuspensionDamage(pagePhysics, ACCManager.Data.SetupConverter.Wheel.RearLeft);
                float suspensionDamageRearRight = Damage.GetSuspensionDamage(pagePhysics, ACCManager.Data.SetupConverter.Wheel.RearRight);

                if (suspensionDamageFrontLeft > 0)
                    DrawTextWithOutline(g, Color.Green, $"{suspensionDamageFrontLeft:F1}", horizontalPadding * 3, verticalPadding * 2);

                if (suspensionDamageFrontRight > 0)
                    DrawTextWithOutline(g, Color.Green, $"{suspensionDamageFrontRight:F1}", scaledWidth - horizontalPadding * 3, verticalPadding * 2);

                if (suspensionDamageRearLeft > 0)
                    DrawTextWithOutline(g, Color.Green, $"{suspensionDamageRearLeft:F1}", horizontalPadding * 3, scaledHeight - verticalPadding * 2);

                if (suspensionDamageRearRight > 0)
                    DrawTextWithOutline(g, Color.Green, $"{suspensionDamageRearRight:F1}", scaledWidth - horizontalPadding * 3, scaledHeight - verticalPadding * 2);

            });
        }

        private void UpdateBodyDamage()
        {
            int scaledWidth = (int)(OriginalWidth * this.Scale);
            int scaledHeight = (int)(OriginalHeight * this.Scale);
            int horizontalPadding = (int)(scaledWidth * 0.1);
            int verticalPadding = (int)(scaledHeight * 0.1);
            _bodyDamage?.SetRenderer(g =>
            {
                float bodyDamageFront = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Front);
                float bodyDamageRear = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Rear);
                float bodyDamageLeft = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Left);
                float bodyDamageRight = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Right);
                float bodyDamageCentre = Damage.GetBodyWorkDamage(pagePhysics, Damage.CarDamagePosition.Centre);

                if (bodyDamageFront > 0)
                    DrawTextWithOutline(g, Color.Black, $"{bodyDamageFront:F1}", scaledWidth / 2, verticalPadding / 2);

                if (bodyDamageRear > 0)
                    DrawTextWithOutline(g, Color.Black, $"{bodyDamageRear:F1}", scaledWidth / 2, scaledHeight - verticalPadding * 1);

                if (bodyDamageLeft > 0)
                    DrawTextWithOutline(g, Color.Black, $"{bodyDamageLeft:F1}", horizontalPadding * 2, scaledHeight / 2);

                if (bodyDamageRight > 0)
                    DrawTextWithOutline(g, Color.Black, $"{bodyDamageRight:F1}", scaledWidth - horizontalPadding * 2, scaledHeight / 2);
            });
        }

        private void DrawTextWithOutline(Graphics g, Color textColor, string text, int x, int y)
        {
#if DEBUG
            //return;
#endif
            int textWidth = (int)g.MeasureString(text, _font).Width;
            Rectangle backgroundDimension = new Rectangle(x - textWidth / 2, y, (int)textWidth, _font.Height);
            g.FillRoundedRectangle(new SolidBrush(Color.FromArgb(210, 255, 255, 255)), backgroundDimension, 2);
            g.DrawRoundedRectangle(new Pen(Color.White), backgroundDimension, 2);
            g.DrawStringWithShadow(text, _font, textColor, new PointF(x - textWidth / 2, y));
        }
    }
}
