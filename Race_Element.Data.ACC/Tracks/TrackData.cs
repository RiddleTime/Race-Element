using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RaceElement.Data.ACC.Tracks;

public sealed class TrackData
{
    public abstract class AbstractTrackData
    {
        public abstract Guid Guid { get; }

        /// <summary>
        /// Get the track name defined by the game.
        /// </summary>
        public abstract string GameName { get; }

        /// <summary>
        /// Get official track name.
        /// </summary>
        public abstract string FullName { get; }

        /// <summary>
        /// Get track length in meters.
        /// </summary>
        public abstract int TrackLength { get; }

        /// <summary>
        /// Get Map scale factor.
        /// </summary>
        public abstract float FactorScale { get; }

        /// <summary>
        /// Get time in seconds a car needs to do drive through.
        /// </summary>
        public abstract float PitLaneTime { get; }

        /// <summary>
        /// the float range is the normalized track position(spline position).
        /// (int, string) => (corner number, corner name). -1 will only show "name", use for straights.
        /// </summary>
        public abstract Dictionary<FloatRangeStruct, (int, string)> CornerNames { get; }

        /// <summary>
        /// Spline position where each sector ends (last sector not is implicit to the end of the spline).
        /// </summary>
        public abstract List<float> Sectors { get; }
    }

    private static readonly List<AbstractTrackData> _tracks = [];
    public static List<AbstractTrackData> Tracks
    {
        get
        {
            if (_tracks.Count == 0)
            {
                foreach (var type in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsClass && x.UnderlyingSystemType.BaseType == typeof(AbstractTrackData)))
                    _tracks.Add((AbstractTrackData)Activator.CreateInstance(type));

                _tracks.Sort((x, y) => x.GameName.CompareTo(y.GameName));
            }

            return _tracks;
        }
    }

    public static AbstractTrackData GetCurrentTrackByFullName(string fullName)
    {
        if (fullName == string.Empty) return null;
        return Tracks.Find(x => x.FullName == fullName);
    }

    public static AbstractTrackData GetCurrentTrack(string gameName)
    {
        if (gameName == string.Empty) return null;
        return Tracks.Find(x => x.GameName == gameName);
    }
}
