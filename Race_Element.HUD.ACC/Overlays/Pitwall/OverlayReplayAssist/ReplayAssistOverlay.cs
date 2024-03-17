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
            Width = 600;
            Height = 400;
            _panel = new InfoPanel(10, 500);
        }
        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            _panel.AddLine("Replay Time", $"{GetReplayTime():hh\\:mm\\:ss\\.ff}");
            _panel.Draw(g);
        }

        private TimeSpan GetReplayTime()
        {
            if (IsPreviewing)
            {
                return TimeSpan.Zero;
            }

            TimeSpan replayTimeSpan = TimeSpan.Zero;
            Process accHandle;

            try
            {
                accHandle = Process.GetProcessesByName("AC2-Win64-Shipping").FirstOrDefault();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return replayTimeSpan;
            }

            if (accHandle == null)
            {
                Debug.WriteLine("ACC process not found. Game may not be running.");
                return replayTimeSpan;
            }

            if (accHandle.MainModule == null)
            {
                Debug.WriteLine("ACC process MainModule not found.");
                return replayTimeSpan;
            }

            IntPtr baseAddr = accHandle.MainModule.BaseAddress + 0x051AE868;
            IntPtr replayTimeAddr = GetPointedAddress(accHandle, baseAddr, [0x20, 0x20, 0x670, 0x678, 0x8, 0x0, 0xB8]);

            if (replayTimeAddr != IntPtr.Zero)
            {
                var replayTime = ProcessMemory<int>.Read(accHandle, replayTimeAddr);
                replayTimeSpan = new TimeSpan(0, 0, 0, 0, replayTime * 100);
            }

            return replayTimeSpan;
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
