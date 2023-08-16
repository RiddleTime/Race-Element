using RaceElement.Data;
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

namespace RaceElement.Controls.Liveries
{
    /// <summary>
    /// Interaction logic for LiveryCreator.xaml
    /// </summary>
    public partial class LiveryCreator : UserControl
    {
        private static LiveryCreator Instance { get; set; }

        public LiveryCreator()
        {
            InitializeComponent();
            buttonClose.Click += (s, e) => Instance.Visibility = Visibility.Collapsed;
            buttonCreate.Click += (s, e) => Create();
            this.Loaded += (s, e) =>
            {
                SetupData();
                LiveryCreator.Instance = this;
            };
        }

        public static void Show()
        {
            if (Instance == null)
                return;

            Instance.Visibility = Visibility.Visible;
        }

        private void SetupData()
        {
            foreach (var item in ConversionFactory.CarModelToCarName)
            {
                if (item.Key == ConversionFactory.CarModels.None)
                    continue;

                ComboBoxItem comboBoxItem = new ComboBoxItem
                {
                    DataContext = item.Key,
                    Content = item.Value
                };
                comboCarModel.Items.Add(comboBoxItem);
            }
        }

        private void Create()
        {
        }
    }
}
