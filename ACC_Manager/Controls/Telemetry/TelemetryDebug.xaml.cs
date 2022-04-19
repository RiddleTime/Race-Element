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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ACCSetupApp.SharedMemory;
using UserControl = System.Windows.Controls.UserControl;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for TelemetryDebug.xaml
    /// </summary>
    public partial class TelemetryDebug : UserControl
    {
        static TelemetryDebug Instance;

        private bool drawOnGame = false;

        SharedMemory sharedMemory = new SharedMemory();

        public TelemetryDebug()
        {
            InitializeComponent();
            Instance = this;

            updateDataButton.Click += UpdateDataButton_Click;
            checkBoxDrawOnGame.Checked += CheckBoxDrawOnGame_Checked;
            checkBoxDrawOnGame.Unchecked += CheckBoxDrawOnGame_Unchecked;

            UpdateStaticData();
            UpdatePhysicsData();
            UpdateGraphicsData();

        }

        private void CheckBoxDrawOnGame_Unchecked(object sender, RoutedEventArgs e)
        {
            drawOnGame = false;
        }

        private void CheckBoxDrawOnGame_Checked(object sender, RoutedEventArgs e)
        {

            new Thread(x =>
            {
                drawOnGame = true;
                Form overlay = null;

                overlay = new Form()
                {
                    WindowState = FormWindowState.Maximized,
                    TopLevel = true,
                    TransparencyKey = System.Drawing.Color.Black,
                    AllowTransparency = true,
                    ShowInTaskbar = false,
                    Capture = false,
                    TopMost = true,
                    FormBorderStyle = FormBorderStyle.None,
                    ShowIcon = false,
                };
                overlay.Show();
                typeof(Form).InvokeMember("DoubleBuffered", BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, overlay, new object[] { true });

                BufferedGraphicsContext ctx = new BufferedGraphicsContext();


                while (drawOnGame)
                {
                    Thread.Sleep(1000 / 60);

                    BufferedGraphics bg = ctx.Allocate(Graphics.FromHwnd(overlay.Handle), new System.Drawing.Rectangle(0, 0, overlay.Width, overlay.Height));
                    Bitmap curBitmap = new Bitmap(overlay.Width, overlay.Height);

                    bg.Graphics.Clear(System.Drawing.Color.Transparent);

                    // draw here
                    DrawData(bg.Graphics);


                    // render double buffer...
                    bg.Render();
                    bg.Dispose();
                }

                if (!drawOnGame)
                {
                    Graphics g = Graphics.FromHwnd(overlay.Handle);
                    g.Dispose();
                    overlay.Dispose();
                }
            }).Start();

        }

        private void DrawData(Graphics g)
        {
            SolidBrush b = new SolidBrush(System.Drawing.Color.White);

            SPageFilePhysics pageStatic = sharedMemory.ReadPhysicsPageFile();
            FieldInfo[] members = pageStatic.GetType().GetFields();
            float y = 0;
            float emSize = 16;
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
                    value = FieldTypeValue(member, value);

                    g.DrawString($"{member.Name}: {value}", new Font("Arial", emSize), b, new PointF(0, y += emSize + 3));
                }
            }
        }

        private void UpdateDataButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStaticData();
            UpdatePhysicsData();
            UpdateGraphicsData();
        }

        private unsafe void UpdateStaticData()
        {
            StackPanel stacker = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };

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

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = FieldTypeValue(member, value);

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
            StackPanel stacker = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };

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

                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = FieldTypeValue(member, value);

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
            StackPanel stacker = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical };

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
                if (!isObsolete && !member.Name.Equals("Buffer") && !member.Name.Equals("Size"))
                {
                    value = FieldTypeValue(member, value);

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

        public static object FieldTypeValue(FieldInfo member, object value)
        {

            if (member.FieldType.Name == typeof(byte[]).Name)
            {
                byte[] arr = (byte[])value;
                value = string.Empty;
                foreach (byte v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }


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
