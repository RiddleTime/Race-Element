using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        // TODO smarter: https://social.msdn.microsoft.com/Forums/en-US/56509a8a-97ef-44c0-9987-56e7cc47e8c4/getting-values-of-fixed-size-buffers-arrays-using-reflection
        static TelemetryTab Instance;

        SharedMemory sharedMemory = new SharedMemory();

        public TelemetryTab()
        {
            InitializeComponent();

            Instance = this;

            updateDataButton.Click += UpdateDataButton_Click;

            UpdateStaticData();
            UpdatePhysicsData();
            UpdateGraphicsData();

        }

        private void UpdateDataButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStaticData();
            UpdatePhysicsData();
            UpdateGraphicsData();
        }

        private unsafe void UpdateStaticData()
        {
            StackPanel stacker = new StackPanel() { Orientation = Orientation.Vertical };

            SPageFileStatic pageStatic = sharedMemory.ReadStaticPageFile();
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);

                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }
                if (!isObsolete)
                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });

            }

            ScrollViewer scrollViewer = new ScrollViewer() { Margin = new Thickness(3) };
            scrollViewer.Content = stacker;


            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabStaticData.Content = scrollViewer;
            }));
        }


        private unsafe void UpdatePhysicsData()
        {
            StackPanel stacker = new StackPanel() { Orientation = Orientation.Vertical };

            SPageFilePhysics pageStatic = sharedMemory.ReadPhysicsPageFile();
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }
                if (!isObsolete)
                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
            }


            ScrollViewer scrollViewer = new ScrollViewer() { Margin = new Thickness(3) };
            scrollViewer.Content = stacker;
            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabPhysicsData.Content = scrollViewer;
            }));
        }

        private unsafe void UpdateGraphicsData()
        {
            StackPanel stacker = new StackPanel() { Orientation = Orientation.Vertical };

            SPageFileGraphic pageStatic = sharedMemory.ReadGraphicsPageFile();
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }
                if (!isObsolete)
                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
            }



            ScrollViewer scrollViewer = new ScrollViewer() { Margin = new Thickness(3) };
            scrollViewer.Content = stacker;

            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabGraphicsData.Content = scrollViewer;
            }));
        }
    }
}
