using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Helpers;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for DataRecorder.xaml
    /// </summary>
    public partial class DataRecorder : UserControl
    {
        private TelemetryStorage _storage = new TelemetryStorage();
        private TelemetryRecorder _recorder;

        private static DataRecorder Instance;

        private bool Recording = false;

        public DataRecorder()
        {
            InitializeComponent();


            buttonRecordData.Click += ButtonRecordData_Click;


            Instance = this;
        }

        private void ButtonRecordData_Click(object sender, RoutedEventArgs e)
        {
            if (!Recording)
            {
                new Thread(() =>
                {
                    Instance.Dispatcher.BeginInvoke(new Action(() => { buttonRecordData.Content = "Stop Recording"; }));
                    _recorder = new TelemetryRecorder(ref _storage);

                    _recorder.Record();
                    Recording = true;

                    while (Recording)
                    {
                        Thread.Sleep(100);
                    }
                    _recorder.Stop();

                    Instance.Dispatcher.BeginInvoke(new Action(() => { buttonRecordData.Content = "Start recording"; }));

                    Instance.Dispatcher.BeginInvoke(new Action(() => { UpdateChart(); }));
                }).Start();
            }
            else
            {
                Recording = false;
            }
        }

        private void UpdateChart()
        {
            chart.Series.Clear();

            Dictionary<string, ChartValues<double>> values = new Dictionary<string, ChartValues<double>>();
            Dictionary<string, Axis> axes = new Dictionary<string, Axis>();

            foreach (var data in _storage.GetAllData())
            {


                List<KeyValuePair<string, object>> keyValuePairs = data.Value.ToList();
                for (int i = 0; i < keyValuePairs.Count; i++)
                {
                    var key = keyValuePairs[i].Key;
                    var value = keyValuePairs[i].Value;

                    if (!values.ContainsKey(key))
                    {
                        values.Add(key, new ChartValues<double>() { });
                    }

                    values[key].Add(double.Parse(value.ToString()));

                    Axis axis = new Axis()
                    {
                        Title = key,
                        Position = AxisPosition.RightTop,
                        Foreground = GetAxisColor(key),
                        Visibility = Visibility.Hidden,
                        ShowLabels = false,
                    };
                    if (!axes.ContainsKey(key))
                    {
                        axes[key] = axis;
                    }
                }
            }

            chart.AxisY.Clear();
            foreach (var axis in axes)
            {
                chart.AxisY.Add(axis.Value);
            }

            for (int i = 0; i < values.Count; i++)
            {
                KeyValuePair<string, ChartValues<double>> kvPair = values.ElementAt(i);
                chart.Series.Add(new LineSeries()
                {
                    Title = kvPair.Key,
                    Values = kvPair.Value,
                    ScalesYAt = i,
                    PointGeometry = DefaultGeometries.None,
                    Fill = Brushes.Transparent,
                    Foreground = null,
                    Stroke = GetAxisColor(kvPair.Key),
                    StrokeThickness = 1,
                });
            }
        }

        private SolidColorBrush GetAxisColor(string name)
        {
            SolidColorBrush[] randomColors = new SolidColorBrush[] { Brushes.White, Brushes.Yellow };

            switch (name)
            {
                case "Throttle": { return Brushes.Green; }
                case "Brake": return Brushes.Red;
                case "Speed": return Brushes.White;
            }
            return randomColors[new Random().Next(0, randomColors.Length)];
        }


    }
}
