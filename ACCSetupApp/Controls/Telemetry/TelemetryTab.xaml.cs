using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
using static ACCSetupApp.SharedMemory;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for TelemetryTab.xaml
    /// </summary>
    public partial class TelemetryTab : UserControl
    {
        //private TelemetryStorage _storage = new TelemetryStorage();
        //private TelemetryRecorder _recorder;

        public TelemetryTab()
        {
            InitializeComponent();

            // look into wpf chart library or DIY 
            //
            // https://github.com/Apress/practical-wpf-charts-graphics

            //new Thread(() =>
            //{
            //    _recorder = new TelemetryRecorder(ref _storage);

            //    _recorder.Record();

            //    Thread.Sleep(30 * 1000);

            //    _recorder.Stop();

            //    long firstTime = _storage.GetAllData().First().Key;
            //    _storage.GetAllData().ToList().ForEach(data =>
            //    {
            //        Debug.Write(new DateTime(data.Key - firstTime).ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) + ": ");

            //        data.Value.ToList().ForEach(value =>
            //        {
            //            if (value.Value.GetType() == typeof(Single[]))
            //            {
            //                string print = string.Empty;
            //                Single[] arr = (Single[])value.Value;
            //                foreach (Single v in arr)
            //                {
            //                    print += $"{{{v}}}, ";
            //                }
            //                Debug.Write($"[{value.Key}, {print}], ");
            //            }
            //            else
            //                Debug.Write($"{value}, ");
            //        });

            //        Debug.WriteLine(String.Empty);
            //    });
            //}).Start();
        }
    }
}
