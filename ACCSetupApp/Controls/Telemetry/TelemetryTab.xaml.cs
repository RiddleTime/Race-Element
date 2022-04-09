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


                if (member.FieldType.IsNested)
                {
                    object[] attr = member.GetCustomAttributes(typeof(FixedBufferAttribute), false);
                    if (attr.Length > 0)
                    {
                        FixedBufferAttribute bufattr = (FixedBufferAttribute)attr[0];

                        //Console.WriteLine("Member '{0}' is a fixed buffer with {1} elements of {2}", member.Name, bufattr.Length, bufattr.ElementType);
                        if (bufattr.ElementType == typeof(char))
                        {
                            switch (member.Name)
                            {
                                case "smVersion": value = new string(pageStatic.smVersion); break;
                                case "acVersion": value = new string(pageStatic.acVersion); break;
                                case "carModel": value = new string(pageStatic.carModel); break;
                                case "track": value = new string(pageStatic.track); break;
                                case "playerName": value = new string(pageStatic.playerName); break;
                                case "playerSurname": value = new string(pageStatic.playerSurname); break;
                                case "playerNick": value = new string(pageStatic.playerNick); break;
                                case "trackConfiguration": value = new string(pageStatic.trackConfiguration); break;
                                case "carSkin": value = new string(pageStatic.carSkin); break;
                            }
                        }

                        if (bufattr.ElementType == typeof(Single))
                        {
                            switch (member.Name)
                            {
                                case "suspensionMaxTravel": value = $"FL:{pageStatic.suspensionMaxTravel[0]}, FR:{pageStatic.suspensionMaxTravel[1]}, RL:{pageStatic.suspensionMaxTravel[2]}, RR:{pageStatic.suspensionMaxTravel[3]}"; break;
                                case "tyreRadius": value = $"FL:{pageStatic.tyreRadius[0]}, FR:{pageStatic.tyreRadius[1]}, RL:{pageStatic.tyreRadius[2]}, RR:{pageStatic.tyreRadius[3]}"; break;
                            }
                        }
                    }


                    stacker.Children.Add(new TextBlock() { Text = $"{member.Name}: {value}" });
                }

                //SPageFileStatic

                ScrollViewer scrollViewer = new ScrollViewer() { Margin = new Thickness(3) };
                scrollViewer.Content = stacker;


                Instance.Dispatcher.BeginInvoke(new Action(() =>
                {
                    tabStaticData.Content = scrollViewer;
                }));
            }
        }

        private unsafe void UpdatePhysicsData()
        {
            StackPanel stacker = new StackPanel() { Orientation = Orientation.Vertical };

            SPageFilePhysics pageStatic = sharedMemory.ReadPhysicsPageFile();
            FieldInfo[] members = pageStatic.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var value = member.GetValue(pageStatic);


                if (member.FieldType.IsNested)
                {
                    object[] attr = member.GetCustomAttributes(typeof(FixedBufferAttribute), false);
                    if (attr.Length > 0)
                    {
                        FixedBufferAttribute bufattr = (FixedBufferAttribute)attr[0];
                        //Console.WriteLine("Member '{0}' is a fixed buffer with {1} elements of {2}", member.Name, bufattr.Length, bufattr.ElementType);

                        if (bufattr.ElementType == typeof(Single))
                        {
                            switch (member.Name)
                            {
                                // 3 coords
                                case "velocity": value = $"X:{pageStatic.velocity[0]}, Y:{pageStatic.velocity[1]}, Z:{pageStatic.velocity[2]}"; break;
                                case "accG": value = $"X:{pageStatic.accG[0]}, Y:{pageStatic.accG[1]}, Z:{pageStatic.accG[2]}"; break;
                                case "localVelocity": value = $"X:{pageStatic.localVelocity[0]}, Y:{pageStatic.localVelocity[1]}, Z:{pageStatic.localVelocity[2]}"; break;
                                case "localAngularVel": value = $"X:{pageStatic.localAngularVel[0]}, Y:{pageStatic.localAngularVel[1]}, Z:{pageStatic.localAngularVel[2]}"; break;


                                // tire side info
                                case "wheelSlip": value = $"FL:{pageStatic.wheelSlip[0]}, FR:{pageStatic.wheelSlip[1]}, RL:{pageStatic.wheelSlip[2]}, RR:{pageStatic.wheelSlip[3]}"; break;
                                case "wheelLoad": value = $"FL:{pageStatic.wheelLoad[0]}, FR:{pageStatic.wheelLoad[1]}, RL:{pageStatic.wheelLoad[2]}, RR:{pageStatic.wheelLoad[3]}"; break;
                                case "wheelsPressure": value = $"FL:{pageStatic.wheelsPressure[0]}, FR:{pageStatic.wheelsPressure[1]}, RL:{pageStatic.wheelsPressure[2]}, RR:{pageStatic.wheelsPressure[3]}"; break;
                                case "wheelAngularSpeed": value = $"FL:{pageStatic.wheelAngularSpeed[0]}, FR:{pageStatic.wheelAngularSpeed[1]}, RL:{pageStatic.wheelAngularSpeed[2]}, RR:{pageStatic.wheelAngularSpeed[3]}"; break;
                                case "tyreWear": value = $"FL:{pageStatic.tyreWear[0]}, FR:{pageStatic.tyreWear[1]}, RL:{pageStatic.tyreWear[2]}, RR:{pageStatic.tyreWear[3]}"; break;
                                case "tyreDirtyLevel": value = $"FL:{pageStatic.tyreDirtyLevel[0]}, FR:{pageStatic.tyreDirtyLevel[1]}, RL:{pageStatic.tyreDirtyLevel[2]}, RR:{pageStatic.tyreDirtyLevel[3]}"; break;
                                case "tyreCoreTemperature": value = $"FL:{pageStatic.tyreCoreTemperature[0]}, FR:{pageStatic.tyreCoreTemperature[1]}, RL:{pageStatic.tyreCoreTemperature[2]}, RR:{pageStatic.tyreCoreTemperature[3]}"; break;
                                case "camberRAD": value = $"FL:{pageStatic.camberRAD[0]}, FR:{pageStatic.camberRAD[1]}, RL:{pageStatic.camberRAD[2]}, RR:{pageStatic.camberRAD[3]}"; break;
                                case "suspensionTravel": value = $"FL:{pageStatic.suspensionTravel[0]}, FR:{pageStatic.suspensionTravel[1]}, RL:{pageStatic.suspensionTravel[2]}, RR:{pageStatic.suspensionTravel[3]}"; break;
                                case "brakeTemp": value = $"FL:{pageStatic.brakeTemp[0]}, FR:{pageStatic.brakeTemp[1]}, RL:{pageStatic.brakeTemp[2]}, RR:{pageStatic.brakeTemp[3]}"; break;
                                case "tyreTempI": value = $"FL:{pageStatic.tyreTempI[0]}, FR:{pageStatic.tyreTempI[1]}, RL:{pageStatic.tyreTempI[2]}, RR:{pageStatic.tyreTempI[3]}"; break;
                                case "tyreTempM": value = $"FL:{pageStatic.tyreTempM[0]}, FR:{pageStatic.tyreTempM[1]}, RL:{pageStatic.tyreTempM[2]}, RR:{pageStatic.tyreTempM[3]}"; break;
                                case "tyreTempO": value = $"FL:{pageStatic.tyreTempO[0]}, FR:{pageStatic.tyreTempO[1]}, RL:{pageStatic.tyreTempO[2]}, RR:{pageStatic.tyreTempO[3]}"; break;
                                case "mz": value = $"FL:{pageStatic.mz[0]}, FR:{pageStatic.mz[1]}, RL:{pageStatic.mz[2]}, RR:{pageStatic.mz[3]}"; break;
                                case "fx": value = $"FL:{pageStatic.fx[0]}, FR:{pageStatic.fx[1]}, RL:{pageStatic.fx[2]}, RR:{pageStatic.fx[3]}"; break;
                                case "fy": value = $"FL:{pageStatic.fy[0]}, FR:{pageStatic.fy[1]}, RL:{pageStatic.fy[2]}, RR:{pageStatic.fy[3]}"; break;
                                case "slipRatio": value = $"FL:{pageStatic.slipRatio[0]}, FR:{pageStatic.slipRatio[1]}, RL:{pageStatic.slipRatio[2]}, RR:{pageStatic.slipRatio[3]}"; break;
                                case "slipAngle": value = $"FL:{pageStatic.slipAngle[0]}, FR:{pageStatic.slipAngle[1]}, RL:{pageStatic.slipAngle[2]}, RR:{pageStatic.slipAngle[3]}"; break;
                                case "suspensionDamage": value = $"FL:{pageStatic.suspensionDamage[0]}, FR:{pageStatic.suspensionDamage[1]}, RL:{pageStatic.suspensionDamage[2]}, RR:{pageStatic.suspensionDamage[3]}"; break;
                                case "tyreTemp": value = $"FL:{pageStatic.tyreTemp[0]}, FR:{pageStatic.tyreTemp[1]}, RL:{pageStatic.tyreTemp[2]}, RR:{pageStatic.tyreTemp[3]}"; break;

                                // 5 element data
                                case "carDamage": value = $"Front:{pageStatic.slipAngle[0]}, Rear:{pageStatic.slipAngle[1]}, Left:{pageStatic.slipAngle[2]}, Right:{pageStatic.slipAngle[3]}, Centre: { pageStatic.slipAngle[4]}"; break;


                                // 2 element data
                                case "rideHeight": value = $"Front:{pageStatic.slipAngle[0]}, Rear:{pageStatic.slipAngle[1]}"; break;

                            }
                        }

                        if (bufattr.ElementType == typeof(Int32))
                        {
                            switch (member.Name)
                            {
                                //case "carID": value = pageStatic.carID->ToString(); break;

                            }
                        }

                    }
                }


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

                if (member.FieldType.IsNested)
                {
                    object[] attr = member.GetCustomAttributes(typeof(FixedBufferAttribute), false);
                    if (attr.Length > 0)
                    {
                        FixedBufferAttribute bufattr = (FixedBufferAttribute)attr[0];


                        if (bufattr.ElementType == typeof(char))
                        {
                            switch (member.Name)
                            {
                                case "currentTime": value = new string(pageStatic.currentTime); break;
                                case "lastTime": value = new string(pageStatic.lastTime); break;
                                case "bestTime": value = new string(pageStatic.bestTime); break;
                                case "split": value = new string(pageStatic.split); break;
                                case "tyreCompound": value = new string(pageStatic.tyreCompound); break;
                            }
                        }

                        if (bufattr.ElementType == typeof(Int32))
                        {
                            switch (member.Name)
                            {
                                case "carID": value = pageStatic.carID->ToString(); break;
                            }
                        }

                    }
                }

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
