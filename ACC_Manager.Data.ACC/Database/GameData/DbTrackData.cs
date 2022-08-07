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

        public int Length { get; set; }
        public string ParseName { get; set; }
    }

    internal class TrackDataCollection
    {
        public static void Insert(DbTrackData track)
        {
            Debug.WriteLine($"Inserting new track data {track._id} {track.ParseName} {track.Length}");

            var collection = LocalDatabase.Database.GetCollection<DbTrackData>();
            collection.EnsureIndex(x => x._id, true);
            collection.Insert(track);
        }
    }
}
