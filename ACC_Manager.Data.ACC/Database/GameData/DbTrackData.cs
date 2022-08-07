using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACCManager.Data.ACC.Database.GameData
{
    internal class DbTrackData
    {
        public Guid TrackGuid { get; set; }
        public string ParseName { get; set; }
    }

    internal class TrackDataDB
    {
        public static void UpsertTrack(DbTrackData track)
        {
            using (var db = LocalDatabase.Instance.Database)
            {
                db.GetCollection<DbTrackData>().Upsert(track);
            }
        }
    }
}
