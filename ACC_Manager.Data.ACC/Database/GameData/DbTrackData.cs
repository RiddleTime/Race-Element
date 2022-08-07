using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Database.GameData
{
    internal class DbTrackData
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid _id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public string ParseName { get; set; }
    }

    internal class TrackDataCollection
    {
        public static DbTrackData GetTrackData(string trackParseName)
        {
            var collection = LocalDatabase.Database.GetCollection<DbTrackData>();

            var result = collection.FindOne(x => x.ParseName == trackParseName);
            if (result == null)
            {
                CreateNewTrack(trackParseName);
                result = collection.FindOne(x => x.ParseName == trackParseName);
            }

            return result;
        }

        private static void CreateNewTrack(string trackParseName)
        {
            Insert(new DbTrackData() { ParseName = trackParseName });
        }

        public static void Insert(DbTrackData track)
        {
            var collection = LocalDatabase.Database.GetCollection<DbTrackData>();
            collection.EnsureIndex(x => x._id, true);
            collection.Insert(track);

            Debug.WriteLine($"Inserted new track data {track._id} {track.ParseName}");

        }
    }
}
