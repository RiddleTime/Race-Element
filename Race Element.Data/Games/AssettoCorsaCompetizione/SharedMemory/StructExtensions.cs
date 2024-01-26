using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory;

/// <summary>
/// From ACManager.Tools https://github.com/gro-ove/actools/blob/master/AcManager.Tools/SharedMemory/StructExtension.cs
/// </summary>
public static class StructExtension
{
    public static T ToStruct<T>(this MemoryMappedFile file, byte[] buffer)
    {
        using (var stream = file.CreateViewStream())
        {
            stream.Read(buffer, 0, buffer.Length);
            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            var data = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return data;
        }
    }

    public static T ToStruct<T>(this TimestampedBytes timestampedBytes)
    {
        var handle = GCHandle.Alloc(timestampedBytes.RawData, GCHandleType.Pinned);
        var data = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();
        return data;
    }

    public static TimestampedBytes ToTimestampedBytes<T>(this T s, byte[] buffer) where T : struct
    {
        var ptr = Marshal.AllocHGlobal(buffer.Length);
        Marshal.StructureToPtr(s, ptr, false);
        Marshal.Copy(ptr, buffer, 0, buffer.Length);
        Marshal.FreeHGlobal(ptr);
        return new TimestampedBytes(buffer, DateTime.Now);
    }
}

public class TimestampedBytes
{
    public byte[] RawData;
    public DateTime IncomingDate;

    public TimestampedBytes(byte[] rawData)
    {
        RawData = rawData;
        IncomingDate = DateTime.Now;
    }

    public TimestampedBytes(byte[] rawData, DateTime incomingDate)
    {
        RawData = rawData;
        IncomingDate = incomingDate;
    }
}
