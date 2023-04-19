using RaceElement.Util.DataTypes;
using ACCManager.Data.ACC.Tracks.Data;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace RaceElement.Data.ACC.Tracks
{
    public class TrackData
    {
        public abstract class AbstractTrackData
        {
            public abstract Guid Guid { get; }
            public abstract string FullName { get; }
            public abstract int TrackLength { get; }

            /// <summary>
            /// the float range is the normalized track position(spline position).
            /// (int, string) => (corner number, corner name). -1 will only show "name", use for straights.
            /// </summary>
            public abstract Dictionary<FloatRangeStruct, (int, string)> CornerNames { get; }
        }

        public static readonly Dictionary<string, AbstractTrackData> Tracks = new Dictionary<string, AbstractTrackData>() {
            {"Barcelona", new Barcelona() },
            {"brands_hatch", new BrandsHatch() },
            {"cota", new Cota() },
            {"donington", new Donington() },
            {"Hungaroring", new Hungaroring() },
            {"Imola",  new Imola() },
            {"indianapolis", new Indianapolis() },
            {"Kyalami",  new Kyalami() },
            {"Laguna_Seca", new LagunaSeca() },
            {"misano",  new Misano() },
            {"monza",  new Monza() },
            {"mount_panorama", new MountPanorama() },
            {"nurburgring", new Nurburgring() },
            {"oulton_park",  new OultonPark() },
            {"Paul_Ricard", new PaulRicard() },
            {"Silverstone",  new Silverstone() },
            {"snetterton",  new Snetterton() },
            {"Spa",  new SpaFrancorchamps() },
            {"Suzuka",  new Suzuka() },
            {"Valencia", new Valencia() },
            {"watkins_glen",  new WatkinsGlen() },
            {"Zandvoort",  new Zandvoort() },
            {"Zolder",  new Zolder() },
        };
    }
}
