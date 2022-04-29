using ACCSetupApp.SetupParser.SetupRanges;
using ACCSetupApp.SetupParser.Cars.GT3;
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
using static ACCSetupApp.SetupParser.SetupConverter;
using MaterialDesignThemes.Wpf;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using static SetupParser.SetupJson;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for SetupEditor.xaml
    /// </summary>
    public partial class SetupEditor : UserControl
    {
        private static SetupEditor _instance;
        public static SetupEditor Instance
        {
            get
            {
                return _instance;
            }
        }

        private ISetupChanger SetupChanger { get; set; }
        private Root Setup { get; set; }

        public SetupEditor()
        {
            InitializeComponent();
            _instance = this;
        }

        public void Open(string file)
        {
            this.Setup = GetSetup(new FileInfo(file));

            Instance.transitionEditPanel.Visibility = Visibility.Visible;
            SetupChanger = new Porsche911IIGT3R();
            CreateFields();
        }

        public void Save()
        {

        }

        public void Close()
        {

        }

        private void CreateFields()
        {
            FieldStackPanel.Children.Clear();

            // Tyre Setup
            FieldStackPanel.Children.Add(GetTyrePressureStacker());
            FieldStackPanel.Children.Add(GetToeStacker());
            FieldStackPanel.Children.Add(GetCamberStacker());
            FieldStackPanel.Children.Add(GetCasterStacker());


        }


        private StackPanel GetTyrePressureStacker()
        {
            StackPanel pressureStacker = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            pressureStacker.Children.Add(new Label() { Content = "PSI " });

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboPressureFL = new ComboBox();
            comboPressureFL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureFL.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontLeft];
            comboPressureFL.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontLeft] = (int)comboPressureFL.SelectedIndex; };
            stackerFL.Children.Add(comboPressureFL);
            pressureStacker.Children.Add(stackerFL);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboPressureFR = new ComboBox();
            comboPressureFR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureFR.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontRight];
            comboPressureFR.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontRight] = (int)comboPressureFR.SelectedIndex; };
            stackerFR.Children.Add(comboPressureFR);
            pressureStacker.Children.Add(stackerFR);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboPressureRL = new ComboBox();
            comboPressureRL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureRL.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearLeft];
            comboPressureRL.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearLeft] = (int)comboPressureRL.SelectedIndex; };
            stackerRL.Children.Add(comboPressureRL);
            pressureStacker.Children.Add(stackerRL);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboPressureRR = new ComboBox();
            comboPressureRR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureRR.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearRight];
            comboPressureRR.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearRight] = (int)comboPressureRR.SelectedIndex; };
            stackerRR.Children.Add(comboPressureRR);
            pressureStacker.Children.Add(stackerRR);


            return pressureStacker;
        }

        private StackPanel GetCamberStacker()
        {
            // Camber inputs 
            StackPanel camberStacker = new StackPanel() { Orientation = Orientation.Horizontal };
            camberStacker.Children.Add(new Label() { Content = "Camber " });

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboToeFL = new ComboBox();
            comboToeFL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberFront);
            comboToeFL.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.FrontLeft];
            comboToeFL.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.FrontLeft] = (int)comboToeFL.SelectedIndex; };
            stackerFL.Children.Add(comboToeFL);
            camberStacker.Children.Add(stackerFL);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboToeFR = new ComboBox();
            comboToeFR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberFront);
            comboToeFR.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.FrontRight];
            comboToeFR.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.FrontRight] = (int)comboToeFR.SelectedIndex; };
            stackerFR.Children.Add(comboToeFR);
            camberStacker.Children.Add(stackerFR);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboToeRL = new ComboBox();
            comboToeRL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberRear);
            comboToeRL.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.RearLeft];
            comboToeRL.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.RearLeft] = (int)comboToeRL.SelectedIndex; };
            stackerRL.Children.Add(comboToeRL);
            camberStacker.Children.Add(stackerRL);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboToeRR = new ComboBox();
            comboToeRR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberRear);
            comboToeRR.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.RearRight];
            comboToeRR.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.RearRight] = (int)comboToeRR.SelectedIndex; };
            stackerRR.Children.Add(comboToeRR);
            camberStacker.Children.Add(stackerRR);

            return camberStacker;
        }

        private StackPanel GetCasterStacker()
        {
            // Caster inputs 
            StackPanel casterStacker = new StackPanel() { Orientation = Orientation.Horizontal };
            casterStacker.Children.Add(new Label() { Content = "Caster " });
            var casterRange = SetupChanger.TyreSetupChanger.Caster;

            // LF
            StackPanel stackerCasterLF = new StackPanel() { Orientation = Orientation.Horizontal };
            stackerCasterLF.Children.Add(new Label() { Content = "FL" });
            ComboBox comboCasterLF = new ComboBox();
            comboCasterLF.ItemsSource = GetDoubleRangeCollection(casterRange);
            comboCasterLF.SelectedIndex = Setup.basicSetup.alignment.casterLF;
            stackerCasterLF.Children.Add(comboCasterLF);
            casterStacker.Children.Add(stackerCasterLF);

            // RF
            StackPanel stackerCasterRF = new StackPanel() { Orientation = Orientation.Horizontal };
            stackerCasterRF.Children.Add(new Label() { Content = "FR" });
            ComboBox comboCasterRF = new ComboBox();
            comboCasterRF.ItemsSource = GetDoubleRangeCollection(casterRange);
            comboCasterRF.SelectedIndex = Setup.basicSetup.alignment.casterRF;
            stackerCasterRF.Children.Add(comboCasterRF);
            casterStacker.Children.Add(stackerCasterRF);

            return casterStacker;
        }

        private StackPanel GetToeStacker()
        {
            StackPanel toeStacker = new StackPanel() { Orientation = Orientation.Horizontal };
            toeStacker.Children.Add(new Label() { Content = "Toe" });

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboToeFL = new ComboBox();
            comboToeFL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeFront);
            comboToeFL.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.FrontLeft];
            stackerFL.Children.Add(comboToeFL);
            toeStacker.Children.Add(stackerFL);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboToeFR = new ComboBox();
            comboToeFR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeFront);
            comboToeFR.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.FrontRight];
            stackerFR.Children.Add(comboToeFR);
            toeStacker.Children.Add(stackerFR);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboToeRL = new ComboBox();
            comboToeRL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeRear);
            comboToeRL.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.RearLeft];
            stackerRL.Children.Add(comboToeRL);
            toeStacker.Children.Add(stackerRL);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboToeRR = new ComboBox();
            comboToeRR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeRear);
            comboToeRR.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.RearRight];
            stackerRR.Children.Add(comboToeRR);
            toeStacker.Children.Add(stackerRR);

            return toeStacker;
        }

        private double[] GetDoubleRangeCollection(SetupDoubleRange doubleRange)
        {
            if (doubleRange.LUT != null)
            {
                return doubleRange.LUT;
            }

            List<double> collection = new List<double>();

            for (double i = doubleRange.Min; i < doubleRange.Max + doubleRange.Increment; i += doubleRange.Increment)
            {
                collection.Add(Math.Round(i, 2));
            }

            return collection.ToArray();
        }

        public Root GetSetup(FileInfo file)
        {
            if (!file.Exists)
                return null;

            string jsonString = string.Empty;
            try
            {
                using (FileStream fileStream = file.OpenRead())
                {
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        jsonString = reader.ReadToEnd();
                        reader.Close();
                        fileStream.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

            Root setup = JsonConvert.DeserializeObject<Root>(jsonString);
            return setup;
        }
    }
}
