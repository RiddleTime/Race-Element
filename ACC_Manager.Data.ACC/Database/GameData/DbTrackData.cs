using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Database.GameData
{
    public class DbTrackData
    {
#pragma warning disable IDE1006 // Naming Styles
        public Guid _id { get; set; }
#pragma warning restore IDE1006 // Naming Styles

        public string ParseName { get; set; }
    }

    public class TrackDataCollection
    {
        private static ILiteCollection<DbTrackData> _collection;
        private static ILiteCollection<DbTrackData> Collection
        {
            get
            {
                if (_collection == null)
                    _collection = LocalDatabase.Database.GetCollection<DbTrackData>();

                return _collection;
            }
        }

        public static List<DbTrackData> GetAll()
        {
            var allTracks = Collection.FindAll();
            if (!allTracks.Any()) return new List<DbTrackData>();

            return allTracks.OrderBy(x => x.ParseName).ToList();
        }

        public static DbTrackData GetTrackData(Guid id)
        {
            var collection = LocalDatabase.Database.GetCollection<DbTrackData>();
            return Collection.FindById(id);
        }

        public static DbTrackData GetTrackData(string trackParseName)
        {
            var result = Collection.FindOne(x => x.ParseName == trackParseName);
            if (result == null)
            {
                Insert(new DbTrackData() { ParseName = trackParseName });
                result = Collection.FindOne(x => x.ParseName == trackParseName);
            }

            return result;
        }

        public static void Insert(DbTrackData track)
        {
            Collection.EnsureIndex(x => x._id, true);
            Collection.Insert(track);

            Debug.WriteLine($"Inserted new track data {track._id} {track.ParseName}");

        }
    }
}
