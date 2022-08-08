using ACCManager.Data;
using ACCManager.Data.ACC.Database.GameData;
using ACCManager.Data.ACC.Database.SessionData;
using ACCManager.Data.ACC.Tracks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for RaceSessionBrowser.xaml
    /// </summary>
    public partial class RaceSessionBrowser : UserControl
    {
        public RaceSessionBrowser()
        {
            InitializeComponent();

            this.Loaded += (s, e) => LoadSessionList();
        }

        public void LoadSessionList()
        {
            List<DbRaceSession> allsessions = RaceSessionCollection.GetAll();


            listViewRaceSessions.Items.Clear();
            foreach (DbRaceSession session in allsessions)
            {
                DbCarData carData = CarDataCollection.GetCarData(session.CarGuid);
                DbTrackData trackData = TrackDataCollection.GetTrackData(session.TrackGuid);

                var carModel = ConversionFactory.ParseCarName(carData.ParseName);
                string carName = ConversionFactory.GetNameFromCarModel(carModel);
                TrackNames.Tracks.TryGetValue(trackData.ParseName, out string trackName);

                listViewRaceSessions.Items.Add($"Index: {session.SessionIndex} {session.SessionType} - {carName} @ {trackName}");
            }
        }
    }
}
