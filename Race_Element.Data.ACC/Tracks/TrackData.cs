using RaceElement.Util.DataTypes;
using ACCManager.Data.ACC.Tracks.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace RaceElement.Data.ACC.Tracks
{
    public class TrackData
    {
        public abstract class AbstractTrackData
        {
            public abstract Guid Guid { get; }
            public abstract string GameName { get; }
            public abstract string FullName { get; }
            public abstract int TrackLength { get; }

            /// <summary>
            /// the float range is the normalized track position(spline position).
            /// (int, string) => (corner number, corner name). -1 will only show "name", use for straights.
            /// </summary>
            public abstract Dictionary<FloatRangeStruct, (int, string)> CornerNames { get; }
            public abstract List<float> Sectors { get; }
        }

        private static readonly List<AbstractTrackData> _tracks = new List<AbstractTrackData>();
        public static List<AbstractTrackData> Tracks
        {
            get
            {
                if (!_tracks.Any())
                {
                    foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && x.UnderlyingSystemType.BaseType == typeof(AbstractTrackData)))
                        _tracks.Add((AbstractTrackData)Activator.CreateInstance(type));

                    _tracks.Sort((x, y) => x.GameName.CompareTo(y.GameName));
                }

                return _tracks;
            }
        }

        public static AbstractTrackData GetCurrentTrack(string gameName)
        {
            if (gameName == string.Empty) return null;

            return Tracks.Find(x => x.GameName == gameName);
        }
    }
}
