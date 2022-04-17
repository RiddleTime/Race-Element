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
using static ACCSetupApp.Controls.LiveryBrowser;
using static ACCSetupApp.Controls.LiveryTagging;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for LiveryTagger.xaml
    /// </summary>
    public partial class LiveryTagger : UserControl
    {
        public static LiveryTagger Instance { get; private set; }

        internal List<LiveryTreeCar> Cars = new List<LiveryTreeCar>();

        public LiveryTagger()
        {
            InitializeComponent();

            buttonAdd.Click += ButtonAdd_Click;
            buttonCancel.Click += ButtonCancel_Click;

            tagList.SelectionChanged += TagList_SelectionChanged;

            Visibility = Visibility.Hidden;
            Instance = this;
        }

        private void TagList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in tagList.Items)
            {
                var listBoxItem = (item as ListBoxItem);
                listBoxItem.Background = Brushes.Transparent;
            }

            foreach (var item in tagList.SelectedItems)
            {
                var listBoxItem = (item as ListBoxItem);
                listBoxItem.Background = Brushes.OrangeRed;
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void UpdateTagList()
        {
            tagList.Items.Clear();
            LiveryTagging.GetAllTags().ForEach(tag =>
            {
                ListBoxItem item = new ListBoxItem()
                {
                    DataContext = tag,
                    Content = tag.Name
                };
                tagList.Items.Add(item);
            });
        }

        internal void Open(LiveryTreeCar car)
        {
            Cars.Clear();
            Cars.Add(car);

            UpdateTagList();

            this.Visibility = Visibility.Visible;
        }

        internal void Open(List<LiveryTreeCar> cars)
        {
            Cars.Clear();
            Cars = cars;

            UpdateTagList();

            this.Visibility = Visibility.Visible;
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (tagList.Items.Count == 0)
            {
                MainWindow.Instance.EnqueueSnackbarMessage("Please add a new tag first.");
                goto close;
            }

            if (tagList.SelectedItems.Count == 0)
            {
                MainWindow.Instance.EnqueueSnackbarMessage("Please select 1 or more tags.");
                return;
            }

            foreach (var listItem in tagList.SelectedItems)
            {
                var listBox = listItem as ListBoxItem;
                LiveryTag tag = listBox.DataContext as LiveryTag;
                foreach (var car in Cars)
                {
                    LiveryTagging.AddToTag(tag, car);
                }
            }
        close:
            this.Visibility = Visibility.Hidden;
            LiveryBrowser.Instance.FetchAllCars();
        }
    }
}
