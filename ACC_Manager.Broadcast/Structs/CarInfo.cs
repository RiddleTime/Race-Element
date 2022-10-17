using System.Collections.Generic;

namespace ACCManager.Broadcast.Structs
{
    public class CarInfo
    {
        public ushort CarIndex { get; }
        public byte CarModelType { get; protected internal set; }
        public string TeamName { get; protected internal set; }
        public int RaceNumber { get; protected internal set; }
        public byte CupCategory { get; protected internal set; }
        public int CurrentDriverIndex { get; protected internal set; }
        public IList<DriverInfo> Drivers { get; } = new List<DriverInfo>();
        public NationalityEnum Nationality { get; protected internal set; }

        public CarInfo(ushort carIndex)
        {
            CarIndex = carIndex;
        }

        internal void AddDriver(DriverInfo driverInfo)
        {
            Drivers.Add(driverInfo);
        }

        public string GetCurrentDriverName()
        {
            if (CurrentDriverIndex < Drivers.Count)
                return Drivers[CurrentDriverIndex].LastName;
            return "nobody(?)";
        }
    }
}
