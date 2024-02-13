using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tyres;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Driving.PressureHelper
{
    [Overlay(Name = "Pressure Helper",
Description = "Helps you with tyre pressure for a pitstop.")]
    internal class PressureHelperOverlay : AbstractOverlay
    {
        private readonly PressureHelperConfiguration _config = new();
        private PressureInfoModel Model { get; set; } = new();
        private InfoPanel InfoPanel;

        private bool _isPreviewing = false;
        private sealed record PressureInfoModel
        {
            public DateTime InitialDate { get; set; } = DateTime.MinValue;
            public int TyreSet = -1;
            public float[] InitialPressures { get; set; } = new float[4];
            public float[] PressureLoss { get; set; } = new float[4];

            public float InitialAmbientTemp { get; set; } = 0;
        }

        public PressureHelperOverlay(Rectangle rectangle) : base(rectangle, "Pressure Helper")
        {
            Width = 500;
            Height = 250;
        }

        public override void SetupPreviewData()
        {
            Model = new()
            {
                InitialAmbientTemp = 23.0f,
                InitialDate = DateTime.UtcNow,
                InitialPressures = [23f, 23f, 23.5f, 23.5f],
                PressureLoss = [0.14f, 0f, 0f, 0f],
                TyreSet = 3,
            };
            pageGraphics.mfdTyreSet = 5;
            pageGraphics.mfdTyrePressureLF = 23.1f;
            pageGraphics.mfdTyrePressureRF = 23.1f;
            pageGraphics.mfdTyrePressureLR = 23.6f;
            pageGraphics.mfdTyrePressureRR = 23.6f;

            pagePhysics.AirTemp = 22.3f;

            pagePhysics.IsEngineRunning = true;
            pageGraphics.Status = ACCSharedMemory.AcStatus.AC_LIVE;

            _isPreviewing = true;
        }

        public override void BeforeStart()
        {
            InfoPanel = new(12, 400);
            RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;
            TyresTracker.Instance.OnTyresInfoChanged += Instance_OnTyresInfoChanged;
        }

        private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
        {
            Model = new();
        }
        private void Instance_OnTyresInfoChanged(object sender, TyresTracker.TyresInfo e)
        {
            Model.PressureLoss = e.PressureLoss;
        }

        public override void BeforeStop()
        {
            RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
            TyresTracker.Instance.OnTyresInfoChanged -= Instance_OnTyresInfoChanged;
        }

        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            UpdateModel();

            DrawPanel(g);
        }

        private void UpdateModel()
        {
            if (_isPreviewing) return;

            if (pageGraphics.IsSetupMenuVisible)
            {
                Model = new();
                Debug.WriteLine("Resetted pitstop helper model");
            }
            else
            {
                if (Model.InitialDate == DateTime.MinValue)
                {
                    if (pagePhysics.WheelPressure[0] > 0)
                    {
                        InitModel();
                    }
                }
                else
                {
                    if (Model.TyreSet != pageGraphics.currentTyreSet && pageGraphics.Status == ACCSharedMemory.AcStatus.AC_LIVE)
                    {
                        InitModel();
                    }
                }
            }
        }

        private void InitModel()
        {
            Model = new()
            {
                InitialDate = DateTime.UtcNow,
                InitialPressures = pagePhysics.WheelPressure,
                InitialAmbientTemp = pagePhysics.AirTemp,
                TyreSet = pageGraphics.currentTyreSet
            };
        }

        private void DrawPanel(Graphics g)
        {
            string[] wheels = ["FL", "FR", "RL", "RR"];
            float[] mfdPressures = [pageGraphics.mfdTyrePressureLF, pageGraphics.mfdTyrePressureRF, pageGraphics.mfdTyrePressureLR, pageGraphics.mfdTyrePressureRR];

            for (int i = 0; i < 4; i++)
            {
                StringBuilder sb = new($"Init: {Model.InitialPressures[i]:F1}");
                sb.Append($", MFD: {mfdPressures[i]:F1}");
                sb.Append($", Loss: {Model.PressureLoss[i]:F2}");
                InfoPanel.AddLine(wheels[i], sb.ToString());
            }

            float change = pagePhysics.AirTemp - Model.InitialAmbientTemp;
            string airDelta = change > 0 ? "+" : "";
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_LIVE && pagePhysics.IsEngineRunning)
            {
                InfoPanel.AddLine("Set", $"{pageGraphics.currentTyreSet} -> {pageGraphics.mfdTyreSet}");

                InfoPanel.AddLine($"Air Δ", $"{airDelta}{change:F3} °C");

                if (change > 0.5 || change < -0.5)
                {
                    string psiDelta = change < 0 ? "+" : "";
                    // 1 C change = 0.1 psi change?
                    InfoPanel.AddLine("PSI? Δ", $"{psiDelta}{change * -1 / 10:F2}");
                }
            }
            InfoPanel.Draw(g);
        }
    }
}
