using ACC_Manager.Util.SystemExtensions;
using ACCManager.HUD.Overlay.Configuration;
using ACCManager.HUD.Overlay.Internal;
using ACCManager.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.HUD.ACC.Overlays.OverlaySlipAngle
{
#if DEBUG
    [Overlay(Name = "Oversteer Trace", Version = 1.00,
        Description = "Shows Slip angle", OverlayType = OverlayType.Release)]
#endif
    internal sealed class OversteerOverlay : AbstractOverlay
    {
        private readonly SlipConfiguration _config = new SlipConfiguration();
        private class SlipConfiguration : OverlayConfiguration
        {
            [IntRange(1, 90, 1)]
            public int MaxSlipAngle { get; set; } = 10;

            public SlipConfiguration()
            {
                this.AllowRescale = true;
            }
        }

        internal static OversteerOverlay Instance;

        private readonly InfoPanel _panel;
        private OversteerDataCollector _collector;
        private OversteerGraph _graph;
        private int _originalWidth;
        private int _originalHeight;

        public OversteerOverlay(Rectangle rectangle) : base(rectangle, "Oversteer Trace Overlay")
        {
            _originalWidth = 300;
            _originalHeight = 150;
            //_panel = new InfoPanel(12, _originalWidth) { FirstRowLine = 0 };
            //_originalHeight = _panel.FontHeight * 5;

            this.Width = _originalWidth;
            this.Height = _originalHeight;
            this.RequestsDrawItself = true;
        }

        public sealed override void BeforeStart()
        {

            _collector = new OversteerDataCollector(this) { TraceCount = _originalWidth - 1, MaxSlipAngle = _config.MaxSlipAngle };
            _collector.Start();

            _graph = new OversteerGraph(0, 0, _originalWidth - 1, _originalHeight - 1, _collector);
            Instance = this;
        }

        public sealed override void BeforeStop()
        {
            _collector.Stop();
            _graph.Dispose();
            Instance = null;
        }

        public sealed override void Render(Graphics g)
        {
            //float slipRatioFront = (pagePhysics.WheelSlip[(int)Wheel.FrontLeft] + pagePhysics.WheelSlip[(int)Wheel.FrontRight]) / 2;
            //float slipRatioRear = (pagePhysics.WheelSlip[(int)Wheel.RearLeft] + pagePhysics.WheelSlip[(int)Wheel.RearRight]) / 2;

            //string type = "Neutral";
            //if (slipRatioRear > slipRatioFront)
            //{
            //    float diff = slipRatioRear - slipRatioFront;
            //    if (diff > 0.01)
            //        type = "Oversteer";
            //}
            //if (slipRatioRear < slipRatioFront)
            //{
            //    float diff = slipRatioFront - slipRatioRear;
            //    if (diff > 0.01)
            //        type = "Understeer";
            //}

            //_panel.AddLine("slipFront", $"{slipRatioFront:F2}");
            //_panel.AddLine("slipRear", $"{slipRatioRear:F2}");
            //_panel.AddLine("Oversteer", $"{(slipRatioRear - slipRatioFront):F2}");
            //_panel.AddLine("Balance", type);

            //_panel.Draw(g);


            _graph.Draw(g);
        }

        public sealed override bool ShouldRender() => DefaultShouldRender();
    }
}
