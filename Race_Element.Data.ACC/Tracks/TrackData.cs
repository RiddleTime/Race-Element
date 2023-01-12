using ACC_Manager.Util.DataTypes;
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
            /// </summary>
            public abstract Dictionary<FloatRangeStruct, string> CornerNames { get; }
        }

        /// <summary>
        /// (folder/code name, Name )
        /// </summary>
        public static readonly ImmutableDictionary<string, AbstractTrackData> Tracks = new Dictionary<string, AbstractTrackData>() {
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
            {"watkins_glen",  new WatkinsGlen() },
            {"Zandvoort",  new Zandvoort() },
            {"Zolder",  new Zolder() },
        }.ToImmutableDictionary();


    }
}
