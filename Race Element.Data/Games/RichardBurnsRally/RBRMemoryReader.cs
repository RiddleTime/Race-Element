using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.RichardBurnsRally;

/// <summary>
/// Memory reader for RBR (Richard Burns Rally) to extract wheel speed data.
/// Based on Adaptive_Trigger_RBR.py memory reading implementation.
/// Process: RichardBurnsRally_SSE.exe
/// </summary>
internal sealed partial class RBRMemoryReader : IDisposable
{
    private const string ProcessName = "RichardBurnsRally_SSE";
    private IntPtr _processHandle = IntPtr.Zero;
    private IntPtr _baseAddress = IntPtr.Zero;
    private bool _isConnected = false;

    // Memory offsets based on Python implementation (lines 2580-2597)
    private const int BaseOffset1 = 0x492FB8; // 4796472 in decimal
    private const int Offset1032 = 1032;
    private const int Offset64 = 64;
    private const int WheelSpeedFL = 988;
    private const int WheelSpeedFR = 1676;
    private const int WheelSpeedRL = 2364;
    private const int WheelSpeedRR = 3052;

    [LibraryImport("kernel32.dll", SetLastError = true)]
    private static partial IntPtr OpenProcess(int dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool CloseHandle(IntPtr hObject);

    private const int PROCESS_VM_READ = 0x0010;
    private const int PROCESS_QUERY_INFORMATION = 0x0400;

    /// <summary>
    /// Wheel speeds in km/h
    /// </summary>
    internal readonly record struct WheelSpeeds(float FrontLeft, float FrontRight, float RearLeft, float RearRight);

    public RBRMemoryReader()
    {
        TryConnect();
    }

    private bool TryConnect()
    {
        try
        {
            var processes = Process.GetProcessesByName(ProcessName);
            if (processes.Length == 0)
            {
                _isConnected = false;
                return false;
            }

            var process = processes[0];
            _processHandle = OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, process.Id);

            if (_processHandle == IntPtr.Zero)
            {
                _isConnected = false;
                return false;
            }

            _baseAddress = process.MainModule?.BaseAddress ?? IntPtr.Zero;
            if (_baseAddress == IntPtr.Zero)
            {
                _isConnected = false;
                return false;
            }

            _isConnected = true;
            Debug.WriteLine($"[RBR] Connected to {ProcessName} (PID: {process.Id})");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RBR] Failed to connect to process: {ex.Message}");
            _isConnected = false;
            return false;
        }
    }

    private bool ReadInt32(IntPtr address, out int value)
    {
        value = 0;
        if (!_isConnected || address == IntPtr.Zero) return false;

        byte[] buffer = new byte[4];
        if (ReadProcessMemory(_processHandle, address, buffer, 4, out _))
        {
            value = BitConverter.ToInt32(buffer, 0);
            return true;
        }
        return false;
    }

    private bool ReadFloat(IntPtr address, out float value)
    {
        value = 0f;
        if (!_isConnected || address == IntPtr.Zero) return false;

        byte[] buffer = new byte[4];
        if (ReadProcessMemory(_processHandle, address, buffer, 4, out _))
        {
            value = BitConverter.ToSingle(buffer, 0);
            return true;
        }
        return false;
    }

    public bool TryReadWheelSpeeds(out WheelSpeeds wheelSpeeds)
    {
        wheelSpeeds = default;

        // Reconnect if not connected
        if (!_isConnected && !TryConnect())
            return false;

        try
        {
            // Follow pointer chain: base_address + 0x492FB8 -> +1032 -> +64
            // Python code lines 2580-2586:
            // num5 = rbr_memory_reader.read_int(rbr_memory_reader.base_address + 4796472)
            // if num5: num5 = rbr_memory_reader.read_int(num5 + 1032)
            // if num5: num5 = rbr_memory_reader.read_int(num5 + 64)

            IntPtr addr1 = _baseAddress + BaseOffset1;
            if (!ReadInt32(addr1, out int ptr1) || ptr1 == 0)
                return false;

            IntPtr addr2 = new IntPtr(ptr1) + Offset1032;
            if (!ReadInt32(addr2, out int ptr2) || ptr2 == 0)
                return false;

            IntPtr addr3 = new IntPtr(ptr2) + Offset64;
            if (!ReadInt32(addr3, out int ptr3) || ptr3 == 0)
                return false;

            // Read wheel speeds (in m/s, convert to km/h)
            // Python code lines 2594-2597:
            // wheel_speed_fl = rbr_memory_reader.read_float(num5 + 988) * 3.6
            IntPtr baseWheelAddr = new IntPtr(ptr3);

            if (!ReadFloat(baseWheelAddr + WheelSpeedFL, out float fl)) return false;
            if (!ReadFloat(baseWheelAddr + WheelSpeedFR, out float fr)) return false;
            if (!ReadFloat(baseWheelAddr + WheelSpeedRL, out float rl)) return false;
            if (!ReadFloat(baseWheelAddr + WheelSpeedRR, out float rr)) return false;

            // Convert from m/s to km/h
            wheelSpeeds = new WheelSpeeds(
                fl * 3.6f,
                fr * 3.6f,
                rl * 3.6f,
                rr * 3.6f
            );

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[RBR] Error reading wheel speeds: {ex.Message}");
            _isConnected = false; // Mark as disconnected to retry connection
            return false;
        }
    }

    public void Dispose()
    {
        if (_processHandle != IntPtr.Zero)
        {
            CloseHandle(_processHandle);
            _processHandle = IntPtr.Zero;
        }
        _isConnected = false;
    }
}
