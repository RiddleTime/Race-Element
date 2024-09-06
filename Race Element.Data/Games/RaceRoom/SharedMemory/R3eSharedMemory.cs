using RaceElement.Data.SharedMemory;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;

namespace RaceElement.Data.Games.RaceRoom.SharedMemory
{
    internal sealed class R3eSharedMemory
    {
        public static SharedMemory.Shared Memory;

        public static Shared ReadSharedMemory(bool fromCache = false)
        {
            if (fromCache) return Memory;
            return Memory = MemoryMappedFile.CreateOrOpen(Constants.SharedMemoryName, sizeof(byte), MemoryMappedFileAccess.ReadWrite).ToStruct<Shared>(Shared.Buffer);
        }
    }

    public sealed class Utilities
    {
        public static Single RpsToRpm(Single rps)
        {
            return rps * (60 / (2 * (Single)Math.PI));
        }

        public static Single MpsToKph(Single mps)
        {
            return mps * 3.6f;
        }

        public static bool IsRrreRunning()
        {
            return Process.GetProcessesByName("RRRE").Length > 0 || Process.GetProcessesByName("RRRE64").Length > 0;
        }
    }
}
