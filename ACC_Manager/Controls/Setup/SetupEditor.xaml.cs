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


            // Mechanical Setup



            // Aero Setup

        }


        private Grid GetTyrePressureStacker()
        {
            Grid grid = GetMainGrid("PSI", 100);

            Grid settings = GetGrid(4, 75);
            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboPressureFL = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboPressureFL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureFL.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontLeft];
            comboPressureFL.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontLeft] = comboPressureFL.SelectedIndex; };
            stackerFL.Children.Add(comboPressureFL);
            Grid.SetColumn(stackerFL, 0);


            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboPressureFR = new ComboBox();
            comboPressureFR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureFR.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontRight];
            comboPressureFR.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.FrontRight] = comboPressureFR.SelectedIndex; };
            stackerFR.Children.Add(comboPressureFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboPressureRL = new ComboBox();
            comboPressureRL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureRL.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearLeft];
            comboPressureRL.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearLeft] = comboPressureRL.SelectedIndex; };
            stackerRL.Children.Add(comboPressureRL);
            Grid.SetColumn(stackerRL, 2);


            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboPressureRR = new ComboBox();
            comboPressureRR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureRR.SelectedIndex = Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearRight];
            comboPressureRR.SelectionChanged += (s, e) => { Setup.basicSetup.tyres.tyrePressure[(int)Wheel.RearRight] = comboPressureRR.SelectedIndex; };
            stackerRR.Children.Add(comboPressureRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetCamberStacker()
        {
            Grid grid = GetMainGrid("Camber", 100);

            Grid settings = GetGrid(4, 75);
            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboToeFL = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboToeFL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberFront);
            comboToeFL.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.FrontLeft];
            comboToeFL.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.FrontLeft] = comboToeFL.SelectedIndex; };
            stackerFL.Children.Add(comboToeFL);
            Grid.SetColumn(stackerFL, 0);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboToeFR = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboToeFR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberFront);
            comboToeFR.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.FrontRight];
            comboToeFR.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.FrontRight] = comboToeFR.SelectedIndex; };
            stackerFR.Children.Add(comboToeFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboToeRL = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboToeRL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberRear);
            comboToeRL.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.RearLeft];
            comboToeRL.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.RearLeft] = comboToeRL.SelectedIndex; };
            stackerRL.Children.Add(comboToeRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboToeRR = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboToeRR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.CamberRear);
            comboToeRR.SelectedIndex = Setup.basicSetup.alignment.camber[(int)Wheel.RearRight];
            comboToeRR.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.RearRight] = comboToeRR.SelectedIndex; };
            stackerRR.Children.Add(comboToeRR);
            Grid.SetColumn(stackerRR, 3);

            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetCasterStacker()
        {
            // Caster inputs 
            Grid grid = GetMainGrid("Caster", 100);

            Grid settings = GetGrid(2, 75);
            Grid.SetColumn(settings, 1);

            // LF
            StackPanel stackerCasterLF = new StackPanel() { Orientation = Orientation.Horizontal };
            stackerCasterLF.Children.Add(new Label() { Content = "FL" });
            ComboBox comboCasterLF = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboCasterLF.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.Caster);
            comboCasterLF.SelectedIndex = Setup.basicSetup.alignment.casterLF;
            comboCasterLF.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.casterLF = comboCasterLF.SelectedIndex; };
            stackerCasterLF.Children.Add(comboCasterLF);
            Grid.SetColumn(stackerCasterLF, 0);

            // RF
            StackPanel stackerCasterRF = new StackPanel() { Orientation = Orientation.Horizontal };
            stackerCasterRF.Children.Add(new Label() { Content = "FR" });
            ComboBox comboCasterRF = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboCasterRF.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.Caster);
            comboCasterRF.SelectedIndex = Setup.basicSetup.alignment.casterRF;
            comboCasterRF.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.casterRF = comboCasterRF.SelectedIndex; };
            stackerCasterRF.Children.Add(comboCasterRF);
            Grid.SetColumn(stackerCasterRF, 1);

            settings.Children.Add(stackerCasterLF);
            settings.Children.Add(stackerCasterRF);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetToeStacker()
        {
            Grid grid = GetMainGrid("Toe", 100);

            Grid settings = GetGrid(4, 75);
            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboCasterFL = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboCasterFL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeFront);
            comboCasterFL.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.FrontLeft];
            comboCasterFL.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.FrontLeft] = comboCasterFL.SelectedIndex; };
            stackerFL.Children.Add(comboCasterFL);
            Grid.SetColumn(stackerFL, 0);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboCasterFR = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboCasterFR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeFront);
            comboCasterFR.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.FrontRight];
            comboCasterFR.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.FrontRight] = comboCasterFR.SelectedIndex; };
            stackerFR.Children.Add(comboCasterFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboCasterRL = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboCasterRL.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeRear);
            comboCasterRL.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.RearLeft];
            comboCasterRL.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.RearLeft] = comboCasterRL.SelectedIndex; };
            stackerRL.Children.Add(comboCasterRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboCasterRR = new ComboBox() { HorizontalAlignment = HorizontalAlignment.Right };
            comboCasterRR.ItemsSource = GetDoubleRangeCollection(SetupChanger.TyreSetupChanger.ToeRear);
            comboCasterRR.SelectedIndex = Setup.basicSetup.alignment.toe[(int)Wheel.RearRight];
            comboCasterRR.SelectionChanged += (s, e) => { Setup.basicSetup.alignment.camber[(int)Wheel.RearRight] = comboCasterRR.SelectedIndex; };
            stackerRR.Children.Add(comboCasterRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetMainGrid(string label, int labelWidth)
        {
            Grid grid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                ColumnDefinitions = {
                    new ColumnDefinition() {  Width = new GridLength(labelWidth) },
                    new ColumnDefinition()
                }
            };
            grid.Children.Add(new Label() { Content = label });

            return grid;
        }

        private Grid GetGrid(int columnCount, int columnWidth)
        {
            Grid customGrid = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            for (int i = 0; i < columnCount; i++)
            {
                customGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(columnWidth) });
            }

            return customGrid;
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

        private int[] GetIntegerRangeCollection(SetupIntRange intRange)
        {
            if (intRange.LUT != null)
            {
                return intRange.LUT;
            }

            List<int> collection = new List<int>();

            for (int i = intRange.Min; i < intRange.Max + intRange.Increment; i += intRange.Increment)
            {
                collection.Add(i);
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
