using Gma.System.MouseKeyHook;
using ProcessMemoryUtilities.Managed;
using ProcessMemoryUtilities.Native;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayReplayAssist;

#if DEBUG
[Overlay(Name = "Replay Assist",
Description = "Proof of concept for replay utility",
OverlayType = OverlayType.Pitwall,
Authors = ["Reinier Klarenberg"]
)]
#endif
internal class ReplayAssistOverlay : AbstractOverlay
{
    private InfoPanel _panel;
    private Process _accProcess;
    private IntPtr _processHandle = IntPtr.Zero;

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
    private readonly MultiLevelPointer MlpHoveredReplayHasFocus = new(0x051AE868, [0x20, 0x20, 0x778, 0x20, 0x20, 0x521]);



    // perhaps new?
    private readonly MultiLevelPointer MlpReplayIsReversed = new(0x04B8A6C0, [0x118, 0x620, 0x1170]);

    private readonly MultiLevelPointer MlpReplayPlayPause = new(0x04B8A6C0, [0x118, 0x620, 0x1187]);

    private IKeyboardMouseEvents _globalKbmHook;

    public ReplayAssistOverlay(Rectangle rectangle) : base(rectangle, "Replay Assist")
    {
        RefreshRateHz = 1f;
    }

    public override void BeforeStart()
    {
        Width = 400;
        Height = 400;
        _panel = new InfoPanel(10, 300);

        if (IsPreviewing)
            return;

        SetProcessHandle();
        _globalKbmHook = Hook.GlobalEvents();
        _globalKbmHook.KeyUp += GlobalKbmHook_KeyUp;
    }

    private void SetProcessHandle()
    {
        Debug.WriteLine("Setting _accProcess and _processHandle");
        _accProcess?.Dispose();
        _accProcess = GetAccProcess();

        _processHandle = NativeWrapper.OpenProcess(ProcessAccessFlags.ReadWrite, true, _accProcess.Id);
    }

    private Stopwatch _debounce = Stopwatch.StartNew();
    private void GlobalKbmHook_KeyUp(object sender, KeyEventArgs e)
    {
        if (_debounce.ElapsedMilliseconds > 25)
            new Thread(x =>
            {
                try
                {
                    if (_processHandle == IntPtr.Zero || _accProcess == null || !_accProcess.Responding || _accProcess.HasExited)
                    {
                        SetProcessHandle();
                    }

                    if (!AccHasFocus())
                        return;


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
                catch (AccessViolationException ex)
                {
                    Trace.WriteLine(ex);
                }
                finally
                {
                    //NativeWrapper.CloseHandle(_processHandle);

                }
                _debounce = Stopwatch.StartNew();
            }).Start();
    }

    public override void BeforeStop()
    {
        if (IsPreviewing)
            return;

        _globalKbmHook.KeyUp -= GlobalKbmHook_KeyUp;
        _globalKbmHook.Dispose();

        NativeWrapper.CloseHandle(_processHandle);
    }

    public override bool ShouldRender() => true;

    public override void Render(Graphics g)
    {
        _panel.AddProgressBarWithCenteredText("Replay Helper", 0, 1, 0);

        if (IsPreviewing) goto drawPanel;

#if DEBUG
        try
        {
            _panel.AddLine("Replay Time", $"{GetReplayTime():h\\:mm\\:ss\\.ff}");
            _panel.AddLine("Replay Speed", $"{GetReplaySpeed():F3}");
            _panel.AddLine("Replay Bar open?", $"{IsMenuBarOpen()}");
            _panel.AddLine("Replay Bar %", $"{GetReplayBarPercentage():F3}");
            _panel.AddLine("Function", $"{GetHoveredFunction():X}");
            _panel.AddLine("Has Focus?", $"{AccHasFocus()}");



            // different base pointer
            _panel.AddLine("Reversed?", $"{ReplayIsReversed()}");
            _panel.AddLine("Replay Paused", $"{ReplayIsPlaying()}");

            Process accProcess = GetAccProcess();
            if (accProcess == null || accProcess.MainModule == null) throw new Exception();

            IntPtr baseAddr = GetBaseAddress(accProcess, MlpReplayPlayPause);
            if (baseAddr == IntPtr.Zero) throw new Exception();

            IntPtr directionPtr = GetPointedAddress(baseAddr, MlpReplayIsReversed.Offsets);
            IntPtr playPausePtr = GetPointedAddress(baseAddr, MlpReplayPlayPause.Offsets);
            _panel.AddLine("play", $"{playPausePtr:X}");
            _panel.AddLine("dir", $"{directionPtr:X}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Trace.WriteLine(ex);
        }
#endif

    drawPanel:
        _panel.Draw(g);
    }

    private bool ReplayIsReversed()
    {
        IntPtr baseAddr = GetBaseAddress(_accProcess, MlpReplayIsReversed);
        if (baseAddr == IntPtr.Zero) return false;

        return ProcessMemory<byte>.Read(_processHandle, GetPointedAddress(baseAddr, MlpReplayIsReversed.Offsets)) == 1;

    }

    private bool ReplayIsPlaying()
    {
        IntPtr baseAddr = GetBaseAddress(_accProcess, MlpReplayPlayPause);
        if (baseAddr == IntPtr.Zero) return false;

        return ProcessMemory<byte>.Read(_processHandle, GetPointedAddress(baseAddr, MlpReplayPlayPause.Offsets)) == 1;
    }

    private void ToggleReplayPlayDirection()
    {
        IntPtr baseAddr = GetBaseAddress(_accProcess, MlpReplayIsReversed);
        if (baseAddr == IntPtr.Zero) return;

        IntPtr pointedAddress = GetPointedAddress(baseAddr, MlpReplayIsReversed.Offsets);
        if (pointedAddress == IntPtr.Zero) return;
        int a = ProcessMemory<int>.Read(_processHandle, pointedAddress);
        a = a switch { 0 => 1, 1 => 0, _ => 2 };
        if (a != 2) ProcessMemory<int>.Write(_processHandle, pointedAddress, a);
    }

    private void TogglePlayPause()
    {
        Process accProcess = GetAccProcess();
        if (accProcess == null || accProcess.MainModule == null) return;

        IntPtr baseAddr = GetBaseAddress(accProcess, MlpReplayPlayPause);
        if (baseAddr == IntPtr.Zero) return;

        IntPtr addrReplayIsPaused = GetPointedAddress(baseAddr, MlpReplayPlayPause.Offsets);

        if (addrReplayIsPaused != IntPtr.Zero)
        {
            int a = ProcessMemory<int>.Read(_processHandle, addrReplayIsPaused);
            a = a switch { 0 => 1, 1 => 0, _ => 0 };
            ProcessMemory<int>.Write(_processHandle, addrReplayIsPaused, a);
        }
    }

    private bool AccHasFocus()
    {
        Process accProcess = GetAccProcess();
        return User32.GetForegroundWindow() == accProcess.MainWindowHandle;
    }

    private IntPtr GetHoveredFunction()
    {
        if (IsPreviewing) return IntPtr.Zero;

        Process accProcess = GetAccProcess();
        if (accProcess == null || accProcess.MainModule == null) return IntPtr.Zero;

        IntPtr baseAddr = GetBaseAddress(accProcess, PtrHoveredMenuBarFunction);
        if (baseAddr == IntPtr.Zero) return IntPtr.Zero;

        IntPtr functionPtr = GetPointedAddress(baseAddr, PtrHoveredMenuBarFunction.Offsets);

        if (functionPtr == IntPtr.Zero) return IntPtr.Zero;
        IntPtr functionAddr = ProcessMemory<IntPtr>.Read(_processHandle, functionPtr);

        IntPtr a = ProcessMemory<IntPtr>.Read(_processHandle, functionAddr);
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

        IntPtr replayTimeAddr = GetPointedAddress(baseAddr, PtrReplayTimeSeconds.Offsets);

        if (replayTimeAddr != IntPtr.Zero)
        {
            var replayTime = ProcessMemory<int>.Read(_processHandle, replayTimeAddr);
            replayTimeSpan = new TimeSpan(0, 0, 0, replayTime);
        }

        return replayTimeSpan;
    }
    private static IntPtr GetBaseAddress(Process process, MultiLevelPointer pointer)
    {
        if (process == null || process.MainModule == null) return IntPtr.Zero;
        return process.MainModule.BaseAddress + pointer.MainModuleOffset;
    }

    private Process GetAccProcess()
    {
        try
        {
            _accProcess = Process.GetProcessesByName("AC2-Win64-Shipping").FirstOrDefault();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        return _accProcess;
    }

    private bool IsReplayPaused()
    {
        if (IsPreviewing) return true;
        Process accProcess = GetAccProcess();

        if (accProcess == null || accProcess.MainModule == null) return true;

        IntPtr baseAddr = GetBaseAddress(accProcess, PtrReplayIsPaused);
        if (baseAddr == IntPtr.Zero) return true;

        IntPtr addrReplayIsPaused = GetPointedAddress(baseAddr, PtrReplayIsPaused.Offsets);

        if (addrReplayIsPaused != IntPtr.Zero)
            return ProcessMemory<byte>.Read(_processHandle, addrReplayIsPaused) == 1;
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

        IntPtr replaySpeedAddr = GetPointedAddress(baseAddr, PtrReplaySpeedMultiplier.Offsets);
        if (replaySpeedAddr != IntPtr.Zero)
            speed = ProcessMemory<float>.Read(_processHandle, replaySpeedAddr);

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

        IntPtr replayTimeAddr = GetPointedAddress(baseAddr, PtrReplayBarPercentage.Offsets);
        if (replayTimeAddr != IntPtr.Zero)
            speed = ProcessMemory<float>.Read(_processHandle, replayTimeAddr);

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

        IntPtr replayTimeAddr = GetPointedAddress(baseAddr, PtrMenuBarOpened.Offsets);
        if (replayTimeAddr != IntPtr.Zero)
            isOpen = ProcessMemory<byte>.Read(_processHandle, replayTimeAddr) != 1;

        return isOpen;
    }

    private IntPtr GetPointedAddress(IntPtr baseAddress, int[] offsets)
    {
        var nextBase = baseAddress;

        foreach (var offset in offsets)
        {
            var ptr = ProcessMemory<nint>.Read(_processHandle, nextBase);
            if (ptr == IntPtr.Zero || ptr == IntPtr.MaxValue)
                return IntPtr.Zero;

            nextBase = ptr + offset;
        }

        return nextBase;
    }

    private static void LogPointer(IntPtr address)
    {
        Debug.WriteLine($"0x{address.ToInt64():X} - {address}");
    }
}
