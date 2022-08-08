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

            this.Loaded += (s, e) => FillTrackComboBox();

            comboTracks.SelectionChanged += ComboTracks_SelectionChanged;
            comboCars.SelectionChanged += ComboCars_SelectionChanged;
        }

        private Guid GetSelectedTrack()
        {
            if (comboTracks.SelectedIndex == -1) return Guid.Empty;
            return (Guid)(comboTracks.SelectedItem as ComboBoxItem).DataContext;
        }

        private Guid GetSelectedCar()
        {
            if (comboCars.SelectedIndex == -1) return Guid.Empty;
            return (Guid)(comboCars.SelectedItem as ComboBoxItem).DataContext;
        }

        private void ComboCars_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadSessionList();
        }

        private void ComboTracks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FillCarComboBox();
        }

        public void FillCarComboBox()
        {
            List<Guid> carGuidsForTrack = RaceSessionCollection.GetAllCarsForTrack(GetSelectedTrack());
            List<DbCarData> allCars = CarDataCollection.GetAll();

            comboCars.Items.Clear();
            foreach (DbCarData carData in allCars.Where(x => carGuidsForTrack.Contains(x._id)))
            {
                var carModel = ConversionFactory.ParseCarName(carData.ParseName);
                string carName = ConversionFactory.GetNameFromCarModel(carModel);
                ComboBoxItem item = new ComboBoxItem() { DataContext = carData._id, Content = carName };
                comboCars.Items.Add(item);
            }
            comboCars.SelectedIndex = 0;
        }

        public void FillTrackComboBox()
        {
            comboTracks.Items.Clear();
            foreach (DbTrackData track in TrackDataCollection.GetAll())
            {
                TrackNames.Tracks.TryGetValue(track.ParseName, out string trackName);
                ComboBoxItem item = new ComboBoxItem() { DataContext = track._id, Content = trackName };
                comboTracks.Items.Add(item);
            }
        }

        public void LoadSessionList()
        {
            List<DbRaceSession> allsessions = RaceSessionCollection.GetAll();

            listViewRaceSessions.Items.Clear();
            foreach (DbRaceSession session in allsessions.Where(x => x.TrackGuid == GetSelectedTrack() && x.CarGuid == GetSelectedCar()))
            {
                DbCarData carData = CarDataCollection.GetCarData(session.CarGuid);
                DbTrackData trackData = TrackDataCollection.GetTrackData(session.TrackGuid);

                var carModel = ConversionFactory.ParseCarName(carData.ParseName);
                string carName = ConversionFactory.GetNameFromCarModel(carModel);
                TrackNames.Tracks.TryGetValue(trackData.ParseName, out string trackName);

                listViewRaceSessions.Items.Add($"{session.UtcStart.ToLocalTime():U}");
            }
        }
    }
}
