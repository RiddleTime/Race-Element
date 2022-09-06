using ACCManager.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
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
using static ACCManager.ACCSharedMemory;

namespace ACCManager.Controls
{
    /// <summary>
    /// Interaction logic for TelemetryDebug.xaml
    /// </summary>
    public partial class TelemetryDebug : UserControl
    {
        static TelemetryDebug Instance;

        public TelemetryDebug()
        {
            InitializeComponent();
            Instance = this;

            updateDataButton.Click += UpdateDataButton_Click;
            this.MouseRightButtonUp += UpdateDataButton_Click;
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

            SPageFileStatic pageStatic = ACCSharedMemory.Instance.ReadStaticPageFile(true);
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);

                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = ReflectionUtil.FieldTypeValue(member, value);

                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
                }
            }

            ScrollViewer scrollViewer = new ScrollViewer()
            {
                Margin = new Thickness(3),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            scrollViewer.Content = stacker;


            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabStaticData.Content = scrollViewer;
            }));
        }


        private unsafe void UpdatePhysicsData()
        {
            StackPanel stacker = new StackPanel() { Orientation = Orientation.Vertical };

            SPageFilePhysics pageStatic = ACCSharedMemory.Instance.ReadPhysicsPageFile(true);
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = ReflectionUtil.FieldTypeValue(member, value);

                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
                }
            }


            ScrollViewer scrollViewer = new ScrollViewer()
            {
                Margin = new Thickness(3),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            scrollViewer.Content = stacker;
            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabPhysicsData.Content = scrollViewer;
            }));
        }

        private unsafe void UpdateGraphicsData()
        {
            StackPanel stacker = new StackPanel() { Orientation = Orientation.Vertical };

            SPageFileGraphic pageStatic = ACCSharedMemory.Instance.ReadGraphicsPageFile(true);
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);
                bool isObsolete = false;
                foreach (CustomAttributeData cad in member.CustomAttributes)
                {
                    if (cad.AttributeType == typeof(ObsoleteAttribute)) { isObsolete = true; break; }
                }
                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = ReflectionUtil.FieldTypeValue(member, value);

                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
                }

            }

            ScrollViewer scrollViewer = new ScrollViewer()
            {
                Margin = new Thickness(3),
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            };
            scrollViewer.Content = stacker;

            Instance.Dispatcher.BeginInvoke(new Action(() =>
            {
                tabGraphicsData.Content = scrollViewer;
            }));
        }
    }
}
