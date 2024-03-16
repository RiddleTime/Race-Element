using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayReplayAssist;

public static class ProcessMemory
{
    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

    public static byte[] ReadBytes(this Process process, IntPtr address, int size)
    {
        byte[] buf = new byte[size];
        ReadProcessMemory(process.Handle, address, buf, size, out int bytesread);
        if (bytesread != size) throw new InvalidOperationException();
        return buf;
    }

    public static byte[] ReadBytes(this Process process, int address, int size) { return ReadBytes(process, new IntPtr(address), size); }

    public static byte[] ReadBytes(this Process process, long address, int size) { return ReadBytes(process, new IntPtr(address), size); }

    public static byte ReadByte(this Process process, IntPtr address)
    {
        return ReadBytes(process, address, sizeof(byte))[0];
    }

    public static byte ReadByte(this Process process, int address) { return ReadByte(process, new IntPtr(address)); }

    public static byte ReadByte(this Process process, long address) { return ReadByte(process, new IntPtr(address)); }

    public static sbyte ReadSByte(this Process process, IntPtr address)
    {
        return unchecked((sbyte)ReadBytes(process, address, sizeof(sbyte))[0]);
    }

    public static sbyte ReadSByte(this Process process, int address) { return ReadSByte(process, new IntPtr(address)); }

    public static sbyte ReadSByte(this Process process, long address) { return ReadSByte(process, new IntPtr(address)); }

    public static short ReadInt16(this Process process, IntPtr address)
    {
        return BitConverter.ToInt16(ReadBytes(process, address, sizeof(short)), 0);
    }

    public static short ReadInt16(this Process process, int address) { return ReadInt16(process, new IntPtr(address)); }

    public static short ReadInt16(this Process process, long address) { return ReadInt16(process, new IntPtr(address)); }

    public static ushort ReadUInt16(this Process process, IntPtr address)
    {
        return BitConverter.ToUInt16(ReadBytes(process, address, sizeof(ushort)), 0);
    }

    public static ushort ReadUInt16(this Process process, int address) { return ReadUInt16(process, new IntPtr(address)); }

    public static ushort ReadUInt16(this Process process, long address) { return ReadUInt16(process, new IntPtr(address)); }

    public static int ReadInt32(this Process process, IntPtr address)
    {
        return BitConverter.ToInt32(ReadBytes(process, address, sizeof(int)), 0);
    }

    public static int ReadInt32(this Process process, int address) { return ReadInt32(process, new IntPtr(address)); }

    public static int ReadInt32(this Process process, long address) { return ReadInt32(process, new IntPtr(address)); }

    public static uint ReadUInt32(this Process process, IntPtr address)
    {
        return BitConverter.ToUInt32(ReadBytes(process, address, sizeof(uint)), 0);
    }

    public static uint ReadUInt32(this Process process, int address) { return ReadUInt32(process, new IntPtr(address)); }

    public static uint ReadUInt32(this Process process, long address) { return ReadUInt32(process, new IntPtr(address)); }

    public static long ReadInt64(this Process process, IntPtr address)
    {
        return BitConverter.ToInt64(ReadBytes(process, address, sizeof(long)), 0);
    }

    public static long ReadInt64(this Process process, int address) { return ReadInt64(process, new IntPtr(address)); }

    public static long ReadInt64(this Process process, long address) { return ReadInt64(process, new IntPtr(address)); }

    public static ulong ReadUInt64(this Process process, IntPtr address)
    {
        return BitConverter.ToUInt64(ReadBytes(process, address, sizeof(ulong)), 0);
    }

    public static ulong ReadUInt64(this Process process, int address) { return ReadUInt64(process, new IntPtr(address)); }

    public static ulong ReadUInt64(this Process process, long address) { return ReadUInt64(process, new IntPtr(address)); }

    public static float ReadSingle(this Process process, IntPtr address)
    {
        return BitConverter.ToSingle(ReadBytes(process, address, sizeof(float)), 0);
    }

    public static float ReadSingle(this Process process, int address) { return ReadSingle(process, new IntPtr(address)); }

    public static float ReadSingle(this Process process, long address) { return ReadSingle(process, new IntPtr(address)); }

    public static double ReadDouble(this Process process, IntPtr address)
    {
        return BitConverter.ToDouble(ReadBytes(process, address, sizeof(double)), 0);
    }

    public static double ReadDouble(this Process process, int address) { return ReadDouble(process, new IntPtr(address)); }

    public static double ReadDouble(this Process process, long address) { return ReadDouble(process, new IntPtr(address)); }

    public static bool ReadBoolean(this Process process, IntPtr address)
    {
        return BitConverter.ToBoolean(ReadBytes(process, address, sizeof(bool)), 0);
    }

    public static bool ReadBoolean(this Process process, int address) { return ReadBoolean(process, new IntPtr(address)); }

    public static bool ReadBoolean(this Process process, long address) { return ReadBoolean(process, new IntPtr(address)); }

    public static void Write(this Process process, IntPtr address, byte[] value)
    {
        UIntPtr byteswritten;
        WriteProcessMemory(process.Handle, address, value, (uint)value.Length, out byteswritten);
        if (byteswritten.ToUInt32() != value.Length) throw new InvalidOperationException();
    }

    public static void Write(this Process process, int address, byte[] value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, byte[] value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, byte value)
    {
        Write(process, address, new byte[] { value });
    }

    public static void Write(this Process process, int address, byte value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, byte value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, sbyte value)
    {
        Write(process, address, new byte[] { unchecked((byte)value) });
    }

    public static void Write(this Process process, int address, sbyte value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, sbyte value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, short value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, short value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, short value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, ushort value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, ushort value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, ushort value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, int value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, int value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, int value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, uint value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, uint value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, uint value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, long value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, long value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, long value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, ulong value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, ulong value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, ulong value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, float value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, float value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, float value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, double value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, double value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, double value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, IntPtr address, bool value)
    {
        Write(process, address, BitConverter.GetBytes(value));
    }

    public static void Write(this Process process, int address, bool value) { Write(process, new IntPtr(address), value); }

    public static void Write(this Process process, long address, bool value) { Write(process, new IntPtr(address), value); }
}
