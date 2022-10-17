using System.Collections.Generic;

namespace ACCManager.Broadcast.Structs
{
    public class LapInfo
    {
        public int? LaptimeMS { get; internal set; }
        public List<int?> Splits { get; } = new List<int?>();
        public ushort CarIndex { get; internal set; }
        public ushort DriverIndex { get; internal set; }
        public bool IsInvalid { get; internal set; }
        public bool IsValidForBest { get; internal set; }
        public LapType Type { get; internal set; }

        public override string ToString()
        {
            return $"{LaptimeMS, 5}|{string.Join("|", Splits)}";
        }
    }
}
