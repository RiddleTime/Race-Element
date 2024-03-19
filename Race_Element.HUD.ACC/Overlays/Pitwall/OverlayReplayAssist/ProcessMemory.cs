using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayReplayAssist
{
    static partial class Win32
    {


        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);

        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);
    }

    static partial class User32
    {
        [LibraryImport("user32.dll")]
        public static partial IntPtr GetForegroundWindow();
    }

    public static unsafe class ProcessMemory<T> where T : unmanaged
    {
        /// <summary>
        /// Read a block of memory from the given address and process. The size
        /// to read is given by the template type [sizeof(T)]. If the read
        /// fail it will throw InvalidOperationException.
        ///
        /// Example:
        ///     int value = ProcessMemory&amp;int&gt;.Read(..., ...);
        /// </summary>
        /// <param name="process">The process to read from.</param>
        /// <param name="address">Address of memory to start to read.</param>
        /// <returns>The value read.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static T Read(Process process, nint address)
        {
            byte[] buffer = new byte[sizeof(T)];
            int bytesread;

            bool ok = Win32.ReadProcessMemory(process.Handle, new IntPtr(address), buffer, sizeof(T), out bytesread);
            if (ok == false || bytesread != sizeof(T)) throw new InvalidOperationException();

            return FromBytes(buffer);
        }

        /// <summary>
        /// Write a block if memory to the given address of the process. The
        /// size to write is given by the template argument [sizeof(T)]. If
        /// the write fails it will throw InvalidOperationException.
        ///
        /// example:
        ///     ProcessMemory&amp;int&gt;.Write(..., 3);
        /// </summary>
        /// <param name="process"></param>
        /// <param name="address"></param>
        /// <param name="value"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void Write(Process process, nint address, T value)
        {
            UIntPtr byteswritten;
            var bytes = GetBytes(value);

            bool ok = Win32.WriteProcessMemory(process.Handle, address, bytes, (uint)bytes.Length, out byteswritten);
            if (ok == false || byteswritten.ToUInt32() != bytes.Length) throw new InvalidOperationException();
        }

        /// <summary>
        /// Convert type to byte array. This function only changes the
        /// memory representation.
        /// </summary>
        /// <param name="value">The value to change the representation.</param>
        /// <returns>A byte array of the value.</returns>
        private static byte[] GetBytes(T value)
        {
            var bytes = new byte[Unsafe.SizeOf<T>()];
            Unsafe.As<byte, T>(ref bytes[0]) = value;
            return bytes;
        }

        /// <summary>
        /// Convert a byte array to a type. This function only changes the
        /// memory representation.
        /// </summary>
        /// <param name="buffer">The byte array to change the representation.</param>
        /// <returns>The value it self.</returns>
        private static T FromBytes(byte[] buffer)
        {
            T retValue = default;
            var tmp = (byte*)&retValue;

            for (var i = 0; i < buffer.Length; ++i)
            {
                tmp[i] = buffer[i];
            }

            return retValue;
        }
    }
}
