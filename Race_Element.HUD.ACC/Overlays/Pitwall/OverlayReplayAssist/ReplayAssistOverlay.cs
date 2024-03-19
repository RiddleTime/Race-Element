
using Gma.System.MouseKeyHook;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


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
        readonly record struct MultiLevelPointer(int MainModuleOffset, int[] Offsets);

        private readonly MultiLevelPointer PtrReplayIsPaused = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x528]);

        /// <summary>
        /// Shows the current selected percentage in the replay bar (only changes when using mouse)
        /// </summary>
        private readonly MultiLevelPointer PtrReplayBarPercentage = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x52c]);
        private readonly MultiLevelPointer PtrReplaySpeedMultiplier = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x530]);
        private readonly MultiLevelPointer PtrReplayTimeSeconds = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x538]);
        private readonly MultiLevelPointer PtrMenuBarOpened = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x662]);



        /// <summary>
        /// Test? perhaps this leads to the functional call of each menu button :D
        /// </summary>
        private readonly MultiLevelPointer PtrHoveredMenuBarFunction = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x2b0]);

        /// <summary>
        /// Byte value, 1 = lost focus, 0 = has focus
        /// </summary>
        private readonly MultiLevelPointer PtrHoveredReplayHasFocus = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x521]);


        private readonly MultiLevelPointer PtrReplayIsReversed = new(0x051A4520, [0x30, 0x2B0, 0x6A8, 0x360, 0x2B8, 0x0, 0x28, 0x50, 0xE48]);
        private readonly MultiLevelPointer PtrReplayPlayPause = new(0x051A4520, [0x30, 0x2B0, 0x6A8, 0x360, 0x2B8, 0x0, 0x28, 0x50, 0xE5F]);


        private IKeyboardMouseEvents _globalKbmHook;

        public ReplayAssistOverlay(Rectangle rectangle) : base(rectangle, "Replay Assist")
        {
            RefreshRateHz = 1 / 5f;
        }

        public override void BeforeStart()
        {
            Width = 400;
            Height = 400;
            _panel = new InfoPanel(10, 300);

            if (IsPreviewing) return;
            _globalKbmHook = Hook.GlobalEvents();
            _globalKbmHook.KeyUp += GlobalKbmHook_KeyUp;
        }

        private void GlobalKbmHook_KeyUp(object sender, global::System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.R:
                    {
                        ToggleReplayPlayDirection();
                        break;
                    }
                case Keys.Space:
                    {
                        TogglePlayPause();
                        break;
                    }
                default: break;
            }
        }

        public override void BeforeStop()
        {
            if (IsPreviewing) return;
            _globalKbmHook.KeyUp -= GlobalKbmHook_KeyUp;
            _globalKbmHook?.Dispose();
        }

        public override bool ShouldRender() => true;

        public override void Render(Graphics g)
        {
            _panel.AddLine("Replay Paused", $"{IsReplayPaused()}");
            _panel.AddLine("Replay Time", $"{GetReplayTime():hh\\:mm\\:ss\\.ff}");
            _panel.AddLine("Replay Speed", $"{GetReplaySpeed():F3}");
            _panel.AddLine("Replay Bar open?", $"{IsMenuBarOpen()}");
            _panel.AddLine("Replay Bar %", $"{GetReplayBarPercentage():F3}");
            _panel.AddLine("Function", $"{GetHoveredFunction():X}");
            _panel.AddLine("Has Focus?", $"{ReplayHasFocus()}");
            _panel.AddLine("Reversed?", $"{ReplayIsReversed()}");

            _panel.Draw(g);
        }

        private bool ReplayIsReversed()
        {
            if (IsPreviewing) return false;
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return false;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplayIsReversed);
            if (baseAddr == IntPtr.Zero) return false;

            IntPtr addrReplayIsPaused = GetPointedAddress(accProcess, baseAddr, PtrReplayIsReversed.Offsets);

            if (addrReplayIsPaused != IntPtr.Zero)
            {
                byte a = ProcessMemory<byte>.Read(accProcess, addrReplayIsPaused);
                a = a switch { 0 => 1, 1 => 0 };
                //ProcessMemory<byte>.Write(accProcess, addrReplayIsPaused, a);
                return a == 1;
            }
            return false;
        }

        private void ToggleReplayPlayDirection()
        {
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplayIsReversed);
            if (baseAddr == IntPtr.Zero) return;

            IntPtr addrReplayIsPaused = GetPointedAddress(accProcess, baseAddr, PtrReplayIsReversed.Offsets);

            if (addrReplayIsPaused != IntPtr.Zero)
            {
                byte a = ProcessMemory<byte>.Read(accProcess, addrReplayIsPaused);
                a = a switch { 0 => 1, 1 => 0, _ => 0 };
                ProcessMemory<byte>.Write(accProcess, addrReplayIsPaused, a);
            }
        }

        private void TogglePlayPause()
        {
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplayPlayPause);
            if (baseAddr == IntPtr.Zero) return;

            IntPtr addrReplayIsPaused = GetPointedAddress(accProcess, baseAddr, PtrReplayPlayPause.Offsets);

            if (addrReplayIsPaused != IntPtr.Zero)
            {
                byte a = ProcessMemory<byte>.Read(accProcess, addrReplayIsPaused);
                a = a switch { 0 => 1, 1 => 0, _ => 0 };
                ProcessMemory<byte>.Write(accProcess, addrReplayIsPaused, a);
            }
        }

        private bool ReplayHasFocus()
        {
            if (IsPreviewing) return false;
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return false;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrHoveredReplayHasFocus);
            if (baseAddr == IntPtr.Zero) return false;

            IntPtr addrReplayIsPaused = GetPointedAddress(accProcess, baseAddr, PtrHoveredReplayHasFocus.Offsets);

            if (addrReplayIsPaused != IntPtr.Zero)
                return ProcessMemory<byte>.Read(accProcess, addrReplayIsPaused) == 0;
            return false;
        }

        private IntPtr GetHoveredFunction()
        {
            if (IsPreviewing) return IntPtr.Zero;

            Process accProcess = GetAccProcess();
            if (accProcess == null || accProcess.MainModule == null) return IntPtr.Zero;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrHoveredMenuBarFunction);
            if (baseAddr == IntPtr.Zero) return IntPtr.Zero;

            IntPtr functionPtr = GetPointedAddress(accProcess, baseAddr, PtrHoveredMenuBarFunction.Offsets);

            if (functionPtr == IntPtr.Zero) return IntPtr.Zero;
            IntPtr functionAddr = ProcessMemory<IntPtr>.Read(accProcess, functionPtr);

            IntPtr a = ProcessMemory<IntPtr>.Read(accProcess, functionAddr);
            //Debug.WriteLine($"{baseAddr - functionAddr:X}");

            return functionAddr;
        }

        private TimeSpan GetReplayTime()
        {
            if (IsPreviewing) return TimeSpan.Zero;

            TimeSpan replayTimeSpan = TimeSpan.Zero;
            Process accProcess = GetAccProcess();
            if (accProcess == null || accProcess.MainModule == null) return replayTimeSpan;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplayTimeSeconds);
            if (baseAddr == IntPtr.Zero) return replayTimeSpan;

            IntPtr replayTimeAddr = GetPointedAddress(accProcess, baseAddr, PtrReplayTimeSeconds.Offsets);

            if (replayTimeAddr != IntPtr.Zero)
            {
                var replayTime = ProcessMemory<int>.Read(accProcess, replayTimeAddr);
                replayTimeSpan = new TimeSpan(0, 0, 0, replayTime);
            }

            return replayTimeSpan;
        }
        private static IntPtr GetBaseAddress(Process process, MultiLevelPointer pointer)
        {
            if (process == null || process.MainModule == null) return IntPtr.Zero;
            return process.MainModule.BaseAddress + pointer.MainModuleOffset;
        }

        private static Process GetAccProcess()
        {
            Process accProcess = null;

            try
            {
                accProcess = Process.GetProcessesByName("AC2-Win64-Shipping").FirstOrDefault();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
            return accProcess;
        }

        private bool IsReplayPaused()
        {
            if (IsPreviewing) return true;
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return true;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplayIsPaused);
            if (baseAddr == IntPtr.Zero) return true;

            IntPtr addrReplayIsPaused = GetPointedAddress(accProcess, baseAddr, PtrReplayIsPaused.Offsets);

            if (addrReplayIsPaused != IntPtr.Zero)
                return ProcessMemory<byte>.Read(accProcess, addrReplayIsPaused) == 1;
            return true;
        }

        private float GetReplaySpeed()
        {
            float speed = 1;
            if (IsPreviewing) return speed;
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return 0;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplaySpeedMultiplier);
            if (baseAddr == IntPtr.Zero) return speed;

            IntPtr replaySpeedAddr = GetPointedAddress(accProcess, baseAddr, PtrReplaySpeedMultiplier.Offsets);
            if (replaySpeedAddr != IntPtr.Zero)
                speed = ProcessMemory<float>.Read(accProcess, replaySpeedAddr);

            return speed;
        }

        private float GetReplayBarPercentage()
        {
            float speed = 1;
            if (IsPreviewing) return speed;
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return 0;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplayBarPercentage);
            if (baseAddr == IntPtr.Zero) return speed;

            IntPtr replayTimeAddr = GetPointedAddress(accProcess, baseAddr, PtrReplayBarPercentage.Offsets);
            if (replayTimeAddr != IntPtr.Zero)
                speed = ProcessMemory<float>.Read(accProcess, replayTimeAddr);

            return speed;
        }

        private bool IsMenuBarOpen()
        {
            bool isOpen = false;
            if (IsPreviewing) return false;
            Process accProcess = GetAccProcess();

            if (accProcess == null || accProcess.MainModule == null) return isOpen;

            IntPtr baseAddr = GetBaseAddress(accProcess, PtrMenuBarOpened);
            if (baseAddr == IntPtr.Zero) return isOpen;

            IntPtr replayTimeAddr = GetPointedAddress(accProcess, baseAddr, PtrMenuBarOpened.Offsets);
            if (replayTimeAddr != IntPtr.Zero)
                isOpen = ProcessMemory<Byte>.Read(accProcess, replayTimeAddr) != 1;

            return isOpen;
        }

        private static IntPtr GetPointedAddress(Process process, IntPtr baseAddress, int[] offsets)
        {
            var nextBase = baseAddress;

            foreach (var offset in offsets)
            {
                var ptr = ProcessMemory<nint>.Read(process, nextBase);
                nextBase = ptr + offset;
            }

            return nextBase;
        }

        private static void LogPointer(IntPtr address)
        {
            Debug.WriteLine($"0x{address.ToInt64():X} - {address}");
        }
    }
}
