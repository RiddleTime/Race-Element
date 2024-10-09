using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.Automobilista2.SharedMemory;

internal static class AMS2SharedMemory
{
    public static SharedMemory.Shared Memory;
    private static MemoryMappedFile _file;
    private static byte[] _buffer;

    public static Automobilista2.SharedMemory.Shared ReadSharedMemory(bool fromCache = false)
    {
        if (fromCache) return Memory;

        try
        {
            _file = MemoryMappedFile.OpenExisting(Constants.SharedMemoryName);
        }
        catch (FileNotFoundException)
        {
            return Memory;
        }

        var view = _file.CreateViewStream();
        BinaryReader stream = new(view);

        _buffer = stream.ReadBytes(Marshal.SizeOf(typeof(RaceRoom.SharedMemory.Shared)));
        GCHandle handle = GCHandle.Alloc(_buffer, GCHandleType.Pinned);
        Memory = (Shared)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Shared));
        handle.Free();

        return Memory;
    }

    public static void Clean()
    {
        Memory = new();
        _file?.Dispose();
        _buffer = [];
    }
}
