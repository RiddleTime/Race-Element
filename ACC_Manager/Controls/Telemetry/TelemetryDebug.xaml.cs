using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using static ACCSetupApp.SharedMemory;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for TelemetryDebug.xaml
    /// </summary>
    public partial class TelemetryDebug : UserControl
    {
        static TelemetryDebug Instance;

        SharedMemory sharedMemory = new SharedMemory();

        public TelemetryDebug()
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
                {
                    value = FieldTypeValue(member, value);

                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
                }
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
                {
                    value = FieldTypeValue(member, value);

                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
                }
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
                {
                    value = FieldTypeValue(member, value);

                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
                }

            }



            ScrollViewer scrollViewer = new ScrollViewer() { Margin = new Thickness(3) };
            scrollViewer.Content = stacker;

            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabGraphicsData.Content = scrollViewer;
            }));
        }

        public static object FieldTypeValue(FieldInfo member, object value)
        {
            if (member.FieldType.Name == typeof(Int32[]).Name)
            {
                Int32[] arr = (Int32[])value;
                value = string.Empty;
                foreach (Int32 v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(Single[]).Name)
            {
                Single[] arr = (Single[])value;
                value = string.Empty;
                foreach (Single v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(StructVector3[]).Name)
            {
                StructVector3[] arr = (StructVector3[])value;
                value = string.Empty;
                foreach (StructVector3 v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(StructVector3).Name)
            {
                value = (StructVector3)value;
            }

            return value;
        }
    }
}
