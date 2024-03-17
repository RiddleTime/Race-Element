using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayReplayAssist
{
    [Overlay(Name = "Replay Assist",
Description = "Proof of concept for replay utility",
OverlayType = OverlayType.Pitwall,
Authors = ["Reinier Klarenberg"]
)]
    internal class ReplayAssistOverlay : AbstractOverlay
    {
        private InfoPanel _panel;

        public ReplayAssistOverlay(Rectangle rectangle) : base(rectangle, "Replay Assist")
        {
            RefreshRateHz = 5f;
        }

        public override void BeforeStart()
        {
            Width = 400;
            Height = 400;
            _panel = new InfoPanel(10, 300);
        }
        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            _panel.AddLine("Replay Time", $"{GetReplayTime():hh\\:mm\\:ss\\.ff}");
            _panel.AddLine("Replay Speed", $"{GetReplaySpeed():F3}");
            _panel.Draw(g);
        }

        private TimeSpan GetReplayTime()
        {
            if (IsPreviewing) return TimeSpan.Zero;

            TimeSpan replayTimeSpan = TimeSpan.Zero;
            Process accProcess;

            try
            {
                accProcess = GetAccProcess();
            }
            catch (Exception)
            {
                return replayTimeSpan;
            }

            if (accProcess == null || accProcess.MainModule == null)
                return replayTimeSpan;

            IntPtr baseAddr = accProcess.MainModule.BaseAddress + 0x051AE868;
            IntPtr replayTimeAddr = GetPointedAddress(accProcess, baseAddr, [0x20, 0x20, 0x670, 0x678, 0x8, 0x0, 0xB8]);

            if (replayTimeAddr != IntPtr.Zero)
            {
                var replayTime = ProcessMemory<int>.Read(accProcess, replayTimeAddr);
                replayTimeSpan = new TimeSpan(0, 0, 0, 0, replayTime * 100);
            }

            return replayTimeSpan;
        }

        private static Process GetAccProcess() => Process.GetProcessesByName("AC2-Win64-Shipping").FirstOrDefault();


        private float GetReplaySpeed()
        {
            float speed = 1;
            if (IsPreviewing) return speed;
            Process accProcess;
            try
            {
                accProcess = GetAccProcess();
            }
            catch (Exception e)
            {
                //Debug.WriteLine(e);
                return 0;
            }

            if (accProcess == null || accProcess.MainModule == null) return 0;

            IntPtr baseAddr = accProcess.MainModule.BaseAddress + 0x051BFED8;
            IntPtr replayTimeAddr = GetPointedAddress(accProcess, baseAddr, [0x118, 0x2E8, 0x28, 0x6A0, 0x290, 0xCC]);

            if (replayTimeAddr != IntPtr.Zero)
            {
                speed = ProcessMemory<float>.Read(accProcess, replayTimeAddr);
            }

            return speed;
        }

        private IntPtr GetPointedAddress(Process process, IntPtr baseAddress, int[] offsets)
        {
            var nextBase = baseAddress;

            foreach (var offset in offsets)
            {
                var ptr = ProcessMemory<nint>.Read(process, nextBase);
                nextBase = ptr + offset;
            }

            return nextBase;
        }

        private void LogPointer(IntPtr address)
        {
            Debug.WriteLine($"0x{address.ToInt64():X} - {address}");
        }
    }
}
