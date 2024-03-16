using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayReplayAssist
{
    [Overlay(Name = "Replay Assist",
Description = "Proof of concept for replay utility",
OverlayType = OverlayType.Pitwall
)]
    internal class ReplayAssistOverlay : AbstractOverlay
    {
        private InfoPanel _panel;

        public ReplayAssistOverlay(Rectangle rectangle) : base(rectangle, "Replay Assist")
        {
            RefreshRateHz = 1f;
        }

        public override void BeforeStart()
        {
            Width = 600;
            Height = 400;
            _panel = new InfoPanel(10, 500);
        }
        public override bool ShouldRender()
        {
            return true;
        }

        public override void Render(Graphics g)
        {
            _panel.AddLine("Replay Time", $"{GetReplayTime():hh\\:mm\\:ss}");
            _panel.Draw(g);
        }

        private TimeSpan GetReplayTime()
        {
            TimeSpan replayTimeSpan = TimeSpan.Zero;
            unsafe
            {
                if (IsPreviewing) return replayTimeSpan;

                try
                {
                    Process acc = Process.GetProcessesByName("AC2-Win64-Shipping").FirstOrDefault();

                    if (acc != null)
                    {
                        IntPtr basePtr = acc.MainModule.BaseAddress + 0x051AE868;
                        IntPtr replayTimePtr = GetPointedAddress(acc, basePtr, [0x20, 0x20, 0x670, 0x678, 0x8, 0x0, 0xB8]);
                        if (replayTimePtr != IntPtr.Zero)
                        {
                            int replayTime = (int)(ProcessMemory.ReadInt32(acc, replayTimePtr) / 10f);
                            replayTimeSpan = new TimeSpan(0, 0, replayTime);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ACC NULL");
                    }
                }
                catch (Exception e)
                {
                    //Debug.WriteLine(e);
                }
            }
            return replayTimeSpan;
        }

        private IntPtr GetPointedAddress(Process process, IntPtr baseAddress, int[] offsets)
        {
            IntPtr nextBase = baseAddress;

            unsafe
            {
                for (int i = 0; i < offsets.Length; i++)
                {
                    nint ptr = (nint)ProcessMemory.ReadInt64(process, nextBase);
                    ptr += offsets[i];
                    nextBase = new IntPtr(ptr);
                }
            }
            return nextBase;
        }

        private void LogPointer(IntPtr address)
        {
            Debug.WriteLine($"0x{address.ToInt64():X} - {address}");
        }
    }
}
