using System;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using static RaceElement.Data.SetupConverter;
using RaceElement.Data;
using static RaceElement.Data.SetupJson;
using RaceElement.Data.SetupRanges;

namespace RaceElement.Controls
{
    /// <summary>
    /// Interaction logic for SetupEditor.xaml
    /// </summary>
    public partial class SetupEditor : UserControl
    {
        public static SetupEditor Instance { get; private set; }

        private ISetupChanger SetupChanger { get; set; }
        private Root Setup { get; set; }

        public SetupEditor()
        {
            InitializeComponent();

            buttonSave.Click += (o, e) => Save();
            buttonCancel.Click += (o, e) => Close();

            Instance = this;
        }

        public void Open(string file)
        {
            this.Setup = GetSetup(new FileInfo(file));

            Instance.transitionEditPanel.Visibility = Visibility.Visible;

            ISetupChanger changer = ConversionFactory.GetChanger(ConversionFactory.ParseCarName(Setup.CarName));
            if (changer == null)
                Close();


            EnableBrowser(false);
            SetupChanger = changer;
            CreateFields();
        }

        public void Save()
        {
            // Do save stuff here


            Close();
        }

        private void EnableBrowser(bool enable)
        {
            SetupBrowser.Instance.setupsTreeView.IsEnabled = enable;
        }

        public void Close()
        {
            EnableBrowser(true);
            Instance.transitionEditPanel.Visibility = Visibility.Hidden;
        }

        private void CreateFields()
        {
            FieldStackPanel.Children.Clear();

            // Tyre Setup
            int tyreLabelWidth = 110;
            FieldStackPanel.Children.Add(GetTitle("Tyres Setup"));
            FieldStackPanel.Children.Add(GetTyrePressureStacker(tyreLabelWidth));
            FieldStackPanel.Children.Add(GetToeStacker(tyreLabelWidth));
            FieldStackPanel.Children.Add(GetCamberStacker(tyreLabelWidth));
            FieldStackPanel.Children.Add(GetCasterStacker(tyreLabelWidth));


            // Electronics Setup
            int ecuLabelWidth = tyreLabelWidth;
            FieldStackPanel.Children.Add(GetTitle("Electronics Setup"));
            FieldStackPanel.Children.Add(GetTractionControlStacker(ecuLabelWidth));
            FieldStackPanel.Children.Add(GetABSStacker(ecuLabelWidth));
            FieldStackPanel.Children.Add(GetEngineMapStacker(ecuLabelWidth));



            // Mechanical Setup
            int mechLabelWidth = tyreLabelWidth;
            FieldStackPanel.Children.Add(GetTitle("Mechanical Setup"));
            FieldStackPanel.Children.Add(GetWheelRatesStacker(mechLabelWidth));
            FieldStackPanel.Children.Add(GetBumpstopRateStacker(mechLabelWidth));
            FieldStackPanel.Children.Add(GetBumpstopRangeStacker(mechLabelWidth));
            FieldStackPanel.Children.Add(GetAntiRollBarStacker(mechLabelWidth));
            FieldStackPanel.Children.Add(GetDiffPreloadStacker(mechLabelWidth));
            FieldStackPanel.Children.Add(GetBrakePowerStacker(mechLabelWidth));
            FieldStackPanel.Children.Add(GetBrakeBiasStacker(mechLabelWidth));
            FieldStackPanel.Children.Add(GetSteeringRatioStacker(mechLabelWidth));

            // Damper Setup
            int damperLabelWidth = tyreLabelWidth;
            FieldStackPanel.Children.Add(GetTitle("Damper Setup"));
            FieldStackPanel.Children.Add(GetBumpSlowStacker(damperLabelWidth));
            FieldStackPanel.Children.Add(GetBumpFastStacker(damperLabelWidth));
            FieldStackPanel.Children.Add(GetReboundSlowStacker(damperLabelWidth));
            FieldStackPanel.Children.Add(GetReboundFastStacker(damperLabelWidth));

            // Aero Setup
            int aeroLabelWidth = tyreLabelWidth;
            FieldStackPanel.Children.Add(GetTitle("Aero Setup"));
            FieldStackPanel.Children.Add(GetRideHeightStacker(aeroLabelWidth));
            FieldStackPanel.Children.Add(GetAeroSurfaceStacker(aeroLabelWidth));
            FieldStackPanel.Children.Add(GetBrakeDuctStacker(aeroLabelWidth));


        }

        #region AeroSetupChanger

        private Grid GetAeroSurfaceStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Aero", labelWidth);


            int blockWidth = 50;
            Grid settings = GetGrid(2, blockWidth + 45);
            Grid.SetColumn(settings, 2);


            // Front
            StackPanel stackerFront = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFront.Children.Add(new Label() { Content = "Splitter" });
            ComboBox comboSplitter = new ComboBox() { Width = blockWidth - 11, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboSplitter.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.AeroSetupChanger.Splitter);
            comboSplitter.SelectedIndex = Setup.AdvancedSetup.AeroBalance.Splitter;
            comboSplitter.SelectionChanged += (s, e) => { Setup.AdvancedSetup.AeroBalance.Splitter = comboSplitter.SelectedIndex; };
            stackerFront.Children.Add(comboSplitter);
            Grid.SetColumn(stackerFront, 0);

            // Rear
            StackPanel stackerRear = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRear.Children.Add(new Label() { Content = "Wing" });
            ComboBox comboRear = new ComboBox() { Width = blockWidth + 1, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboRear.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.AeroSetupChanger.RearWing);
            comboRear.SelectedIndex = Setup.AdvancedSetup.AeroBalance.RearWing;
            comboRear.SelectionChanged += (s, e) => { Setup.AdvancedSetup.AeroBalance.RearWing = comboRear.SelectedIndex; };
            stackerRear.Children.Add(comboRear);
            Grid.SetColumn(stackerRear, 1);


            settings.Children.Add(stackerFront);
            settings.Children.Add(stackerRear);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetBrakeDuctStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Brake Ducts", labelWidth);

            int blockWidth = 50;

            Grid settings = GetGrid(2, blockWidth + 45);
            Grid.SetColumn(settings, 1);


            // Front
            StackPanel stackerFront = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFront.Children.Add(new Label() { Content = "Front" });
            ComboBox comboFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.AeroSetupChanger.BrakeDucts);
            comboFL.SelectedIndex = Setup.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Front];
            comboFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Front] = comboFL.SelectedIndex; };
            stackerFront.Children.Add(comboFL);
            Grid.SetColumn(stackerFront, 0);

            // Rear
            StackPanel stackerRear = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRear.Children.Add(new Label() { Content = "Rear" });
            ComboBox comboFR = new ComboBox() { Width = blockWidth + 4, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.AeroSetupChanger.BrakeDucts);
            comboFR.SelectedIndex = Setup.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Rear];
            comboFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Rear] = comboFR.SelectedIndex; };
            stackerRear.Children.Add(comboFR);
            Grid.SetColumn(stackerRear, 1);


            settings.Children.Add(stackerFront);
            settings.Children.Add(stackerRear);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetRideHeightStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Ride Height", labelWidth);

            int blockWidth = 50;

            Grid settings = GetGrid(2, blockWidth + 45);
            Grid.SetColumn(settings, 1);


            // Front
            StackPanel stackerFront = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFront.Children.Add(new Label() { Content = "Front" });
            ComboBox comboFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.AeroSetupChanger.RideHeightFront);
            comboFL.SelectedIndex = Setup.AdvancedSetup.AeroBalance.RideHeight[0];
            comboFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.AeroBalance.RideHeight[0] = comboFL.SelectedIndex; };
            stackerFront.Children.Add(comboFL);
            Grid.SetColumn(stackerFront, 0);

            // Rear
            StackPanel stackerRear = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRear.Children.Add(new Label() { Content = "Rear" });
            ComboBox comboFR = new ComboBox() { Width = blockWidth + 4, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.AeroSetupChanger.RideHeightRear);
            comboFR.SelectedIndex = Setup.AdvancedSetup.AeroBalance.RideHeight[2];
            comboFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.AeroBalance.RideHeight[2] = comboFR.SelectedIndex; };
            stackerRear.Children.Add(comboFR);
            Grid.SetColumn(stackerRear, 1);


            settings.Children.Add(stackerFront);
            settings.Children.Add(stackerRear);

            grid.Children.Add(settings);

            return grid;
        }

        #endregion

        #region DamperSetupChanger

        private Grid GetReboundFastStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Rebound Fast", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);

            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboPressureFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundFast);
            comboPressureFL.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.FrontLeft];
            comboPressureFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.FrontLeft] = comboPressureFL.SelectedIndex; };
            stackerFL.Children.Add(comboPressureFL);
            Grid.SetColumn(stackerFL, 0);


            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboPressureFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundFast);
            comboPressureFR.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.FrontRight];
            comboPressureFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.FrontRight] = comboPressureFR.SelectedIndex; };
            stackerFR.Children.Add(comboPressureFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboPressureRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundFast);
            comboPressureRL.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.RearLeft];
            comboPressureRL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.RearLeft] = comboPressureRL.SelectedIndex; };
            stackerRL.Children.Add(comboPressureRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboPressureRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundFast);
            comboPressureRR.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.RearRight];
            comboPressureRR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundFast[(int)Wheel.RearRight] = comboPressureRR.SelectedIndex; };
            stackerRR.Children.Add(comboPressureRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetReboundSlowStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Rebound Slow", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);

            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboPressureFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundSlow);
            comboPressureFL.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.FrontLeft];
            comboPressureFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.FrontLeft] = comboPressureFL.SelectedIndex; };
            stackerFL.Children.Add(comboPressureFL);
            Grid.SetColumn(stackerFL, 0);


            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboPressureFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundSlow);
            comboPressureFR.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.FrontRight];
            comboPressureFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.FrontRight] = comboPressureFR.SelectedIndex; };
            stackerFR.Children.Add(comboPressureFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboPressureRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundSlow);
            comboPressureRL.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.RearLeft];
            comboPressureRL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.RearLeft] = comboPressureRL.SelectedIndex; };
            stackerRL.Children.Add(comboPressureRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboPressureRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.ReboundSlow);
            comboPressureRR.SelectedIndex = Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.RearRight];
            comboPressureRR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.ReboundSlow[(int)Wheel.RearRight] = comboPressureRR.SelectedIndex; };
            stackerRR.Children.Add(comboPressureRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetBumpSlowStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Bump Slow", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);

            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboPressureFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpSlow);
            comboPressureFL.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.FrontLeft];
            comboPressureFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.FrontLeft] = comboPressureFL.SelectedIndex; };
            stackerFL.Children.Add(comboPressureFL);
            Grid.SetColumn(stackerFL, 0);


            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboPressureFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpSlow);
            comboPressureFR.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.FrontRight];
            comboPressureFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.FrontRight] = comboPressureFR.SelectedIndex; };
            stackerFR.Children.Add(comboPressureFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboPressureRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpSlow);
            comboPressureRL.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.RearLeft];
            comboPressureRL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.RearLeft] = comboPressureRL.SelectedIndex; };
            stackerRL.Children.Add(comboPressureRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboPressureRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpSlow);
            comboPressureRR.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.RearRight];
            comboPressureRR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpSlow[(int)Wheel.RearRight] = comboPressureRR.SelectedIndex; };
            stackerRR.Children.Add(comboPressureRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetBumpFastStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Bump Fast", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);

            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboPressureFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpFast);
            comboPressureFL.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.FrontLeft];
            comboPressureFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.FrontLeft] = comboPressureFL.SelectedIndex; };
            stackerFL.Children.Add(comboPressureFL);
            Grid.SetColumn(stackerFL, 0);


            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboPressureFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpFast);
            comboPressureFR.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.FrontRight];
            comboPressureFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.FrontRight] = comboPressureFR.SelectedIndex; };
            stackerFR.Children.Add(comboPressureFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboPressureRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpFast);
            comboPressureRL.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.RearLeft];
            comboPressureRL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.RearLeft] = comboPressureRL.SelectedIndex; };
            stackerRL.Children.Add(comboPressureRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboPressureRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.DamperSetupChanger.BumpFast);
            comboPressureRR.SelectedIndex = Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.RearRight];
            comboPressureRR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Dampers.BumpFast[(int)Wheel.RearRight] = comboPressureRR.SelectedIndex; };
            stackerRR.Children.Add(comboPressureRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }


        #endregion

        #region ElectronicsSetupChanger


        private Grid GetEngineMapStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Engine Map", labelWidth);

            Grid settings = GetGrid(1, 95);
            Grid.SetColumn(settings, 1);


            StackPanel stackerEcuMap = new StackPanel { Orientation = Orientation.Horizontal };
            ComboBox comboEcuMap = new ComboBox() { Width = 88, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboEcuMap.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.ElectronicsSetupChanger.EcuMap);
            comboEcuMap.SelectedIndex = Setup.BasicSetup.Electronics.ECUMap;
            comboEcuMap.SelectionChanged += (s, e) => { Setup.BasicSetup.Electronics.ECUMap = comboEcuMap.SelectedIndex; };
            stackerEcuMap.Children.Add(comboEcuMap);
            Grid.SetColumn(stackerEcuMap, 0);


            settings.Children.Add(stackerEcuMap);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetABSStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("ABS", labelWidth);

            Grid settings = GetGrid(1, 95);
            Grid.SetColumn(settings, 1);


            StackPanel stackerABS = new StackPanel { Orientation = Orientation.Horizontal };
            ComboBox comboABS = new ComboBox() { Width = 88, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboABS.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.ElectronicsSetupChanger.ABS);
            comboABS.SelectedIndex = Setup.BasicSetup.Electronics.Abs;
            comboABS.SelectionChanged += (s, e) => { Setup.BasicSetup.Electronics.Abs = comboABS.SelectedIndex; };
            stackerABS.Children.Add(comboABS);
            Grid.SetColumn(stackerABS, 0);


            settings.Children.Add(stackerABS);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetTractionControlStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Traction Control", labelWidth);

            Grid settings = GetGrid(2, 95);
            Grid.SetColumn(settings, 2);


            StackPanel stackerTC = new StackPanel { Orientation = Orientation.Horizontal };
            stackerTC.Children.Add(new Label() { Content = "TC1", ToolTip = "Traction Control" });
            ComboBox comboTC = new ComboBox() { Width = 56, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboTC.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.ElectronicsSetupChanger.TractionControl);
            comboTC.SelectedIndex = Setup.BasicSetup.Electronics.TC1;
            comboTC.SelectionChanged += (s, e) => { Setup.BasicSetup.Electronics.TC1 = comboTC.SelectedIndex; };
            stackerTC.Children.Add(comboTC);
            Grid.SetColumn(stackerTC, 0);

            StackPanel stackerTC2 = new StackPanel { Orientation = Orientation.Horizontal };
            stackerTC2.Children.Add(new Label() { Content = "TC2", ToolTip = "Traction Control Cut" });
            ComboBox comboTC2 = new ComboBox() { Width = 57, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboTC2.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.ElectronicsSetupChanger.TractionControlCut);
            comboTC2.SelectedIndex = Setup.BasicSetup.Electronics.TC2;
            comboTC2.SelectionChanged += (s, e) => { Setup.BasicSetup.Electronics.TC2 = comboTC2.SelectedIndex; };
            stackerTC2.Children.Add(comboTC2);
            Grid.SetColumn(stackerTC2, 1);


            settings.Children.Add(stackerTC);
            settings.Children.Add(stackerTC2);

            grid.Children.Add(settings);

            return grid;
        }

        #endregion

        #region MechanicalSetupChanger
        private Grid GetSteeringRatioStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Steering Ratio", labelWidth);

            Grid settings = GetGrid(1, 90);
            Grid.SetColumn(settings, 1);


            StackPanel stackerBrakeBias = new StackPanel { Orientation = Orientation.Horizontal };
            ComboBox comboBrakeBias = new ComboBox() { Width = 88, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboBrakeBias.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.SteeringRatio);
            comboBrakeBias.SelectedIndex = Setup.BasicSetup.Alignment.SteerRatio;
            comboBrakeBias.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.SteerRatio = comboBrakeBias.SelectedIndex; };
            stackerBrakeBias.Children.Add(comboBrakeBias);
            Grid.SetColumn(stackerBrakeBias, 0);


            settings.Children.Add(stackerBrakeBias);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetBrakeBiasStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Brake Bias", labelWidth);

            Grid settings = GetGrid(1, 90);
            Grid.SetColumn(settings, 1);


            StackPanel stackerBrakeBias = new StackPanel { Orientation = Orientation.Horizontal };
            ComboBox comboBrakeBias = new ComboBox() { Width = 88, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboBrakeBias.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BrakeBias);
            comboBrakeBias.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BrakeBias;
            comboBrakeBias.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BrakeBias = comboBrakeBias.SelectedIndex; };
            stackerBrakeBias.Children.Add(comboBrakeBias);
            Grid.SetColumn(stackerBrakeBias, 0);


            settings.Children.Add(stackerBrakeBias);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetBrakePowerStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Brake Power", labelWidth);

            Grid settings = GetGrid(1, 90);
            Grid.SetColumn(settings, 1);


            StackPanel stackerBrakePower = new StackPanel { Orientation = Orientation.Horizontal };
            ComboBox comboBrakePower = new ComboBox() { Width = 88, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboBrakePower.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BrakePower);
            comboBrakePower.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BrakeTorque;
            comboBrakePower.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BrakeTorque = comboBrakePower.SelectedIndex; };
            stackerBrakePower.Children.Add(comboBrakePower);
            Grid.SetColumn(stackerBrakePower, 0);


            settings.Children.Add(stackerBrakePower);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetDiffPreloadStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Diff Preload", labelWidth);

            Grid settings = GetGrid(1, 90);
            Grid.SetColumn(settings, 1);


            StackPanel stackerPreload = new StackPanel { Orientation = Orientation.Horizontal };
            ComboBox comboPreload = new ComboBox() { Width = 88, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPreload.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.PreloadDifferential);
            comboPreload.SelectedIndex = Setup.AdvancedSetup.Drivetrain.Preload;
            comboPreload.SelectionChanged += (s, e) => { Setup.AdvancedSetup.Drivetrain.Preload = comboPreload.SelectedIndex; };
            stackerPreload.Children.Add(comboPreload);
            Grid.SetColumn(stackerPreload, 0);


            settings.Children.Add(stackerPreload);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetAntiRollBarStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Anti roll bar", labelWidth);

            int blockWidth = 50;

            Grid settings = GetGrid(2, blockWidth + 45);
            Grid.SetColumn(settings, 1);


            // FL
            StackPanel stackerFront = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFront.Children.Add(new Label() { Content = "Front" });
            ComboBox comboFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.AntiRollBarFront);
            comboFL.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.ARBFront;
            comboFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.ARBFront = comboFL.SelectedIndex; };
            stackerFront.Children.Add(comboFL);
            Grid.SetColumn(stackerFront, 0);

            // FR
            StackPanel stackerRear = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRear.Children.Add(new Label() { Content = "Rear" });
            ComboBox comboFR = new ComboBox() { Width = blockWidth + 4, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.AntiRollBarRear);
            comboFR.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.ARBRear;
            comboFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.ARBRear = comboFR.SelectedIndex; };
            stackerRear.Children.Add(comboFR);
            Grid.SetColumn(stackerRear, 1);


            settings.Children.Add(stackerFront);
            settings.Children.Add(stackerRear);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetBumpstopRangeStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Bumpstop range", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);
            Grid.SetColumn(settings, 1);


            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRangeFronts);
            comboFL.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.FrontLeft];
            comboFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.FrontLeft] = comboFL.SelectedIndex; };
            stackerFL.Children.Add(comboFL);
            Grid.SetColumn(stackerFL, 0);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRangeFronts);
            comboFR.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.FrontRight];
            comboFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.FrontRight] = comboFR.SelectedIndex; };
            stackerFR.Children.Add(comboFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboRL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRangeRears);
            comboRL.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.RearLeft];
            comboRL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.RearLeft] = comboRL.SelectedIndex; };
            stackerRL.Children.Add(comboRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboRR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRangeRears);
            comboRR.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.RearRight];
            comboRR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopWindow[(int)Wheel.RearRight] = comboRR.SelectedIndex; };
            stackerRR.Children.Add(comboRR);
            Grid.SetColumn(stackerRR, 3);

            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetBumpstopRateStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Bumpstop rate", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);
            Grid.SetColumn(settings, 1);


            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRate);
            comboFL.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.FrontLeft];
            comboFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.FrontLeft] = comboFL.SelectedIndex; };
            stackerFL.Children.Add(comboFL);
            Grid.SetColumn(stackerFL, 0);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRate);
            comboFR.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.FrontRight];
            comboFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.FrontRight] = comboFR.SelectedIndex; };
            stackerFR.Children.Add(comboFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboRL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRate);
            comboRL.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.RearLeft];
            comboRL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.RearLeft] = comboRL.SelectedIndex; };
            stackerRL.Children.Add(comboRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboRR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.BumpstopRate);
            comboRR.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.RearRight];
            comboRR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp[(int)Wheel.RearRight] = comboRR.SelectedIndex; };
            stackerRR.Children.Add(comboRR);
            Grid.SetColumn(stackerRR, 3);

            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetWheelRatesStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Wheelrate", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);
            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.WheelRateFronts);
            comboFL.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.FrontLeft];
            comboFL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.FrontLeft] = comboFL.SelectedIndex; };
            stackerFL.Children.Add(comboFL);
            Grid.SetColumn(stackerFL, 0);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboFR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.WheelRateFronts);
            comboFR.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.FrontRight];
            comboFR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.FrontRight] = comboFR.SelectedIndex; };
            stackerFR.Children.Add(comboFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboRL.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.WheelRateRears);
            comboRL.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.RearLeft];
            comboRL.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.RearLeft] = comboRL.SelectedIndex; };
            stackerRL.Children.Add(comboRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboRR.ItemsSource = SetupIntRange.GetOptionsCollection(SetupChanger.MechanicalSetupChanger.WheelRateRears);
            comboRR.SelectedIndex = Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.RearRight];
            comboRR.SelectionChanged += (s, e) => { Setup.AdvancedSetup.MechanicalBalance.WheelRate[(int)Wheel.RearRight] = comboRR.SelectedIndex; };
            stackerRR.Children.Add(comboRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        #endregion

        #region TyreSetupChanger
        private Grid GetTyrePressureStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("PSI", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);

            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboPressureFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFL.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureFL.SelectedIndex = Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.FrontLeft];
            comboPressureFL.SelectionChanged += (s, e) => { Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.FrontLeft] = comboPressureFL.SelectedIndex; };
            stackerFL.Children.Add(comboPressureFL);
            Grid.SetColumn(stackerFL, 0);


            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboPressureFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureFR.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureFR.SelectedIndex = Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.FrontRight];
            comboPressureFR.SelectionChanged += (s, e) => { Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.FrontRight] = comboPressureFR.SelectedIndex; };
            stackerFR.Children.Add(comboPressureFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboPressureRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRL.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureRL.SelectedIndex = Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.RearLeft];
            comboPressureRL.SelectionChanged += (s, e) => { Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.RearLeft] = comboPressureRL.SelectedIndex; };
            stackerRL.Children.Add(comboPressureRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboPressureRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboPressureRR.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.TyrePressures);
            comboPressureRR.SelectedIndex = Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.RearRight];
            comboPressureRR.SelectionChanged += (s, e) => { Setup.BasicSetup.Tyres.TyrePressure[(int)Wheel.RearRight] = comboPressureRR.SelectedIndex; };
            stackerRR.Children.Add(comboPressureRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetCamberStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Camber", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);
            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboToeFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboToeFL.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.CamberFront);
            comboToeFL.SelectedIndex = Setup.BasicSetup.Alignment.Camber[(int)Wheel.FrontLeft];
            comboToeFL.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.FrontLeft] = comboToeFL.SelectedIndex; };
            stackerFL.Children.Add(comboToeFL);
            Grid.SetColumn(stackerFL, 0);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboToeFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboToeFR.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.CamberFront);
            comboToeFR.SelectedIndex = Setup.BasicSetup.Alignment.Camber[(int)Wheel.FrontRight];
            comboToeFR.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.FrontRight] = comboToeFR.SelectedIndex; };
            stackerFR.Children.Add(comboToeFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboToeRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboToeRL.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.CamberRear);
            comboToeRL.SelectedIndex = Setup.BasicSetup.Alignment.Camber[(int)Wheel.RearLeft];
            comboToeRL.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.RearLeft] = comboToeRL.SelectedIndex; };
            stackerRL.Children.Add(comboToeRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboToeRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboToeRR.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.CamberRear);
            comboToeRR.SelectedIndex = Setup.BasicSetup.Alignment.Camber[(int)Wheel.RearRight];
            comboToeRR.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.RearRight] = comboToeRR.SelectedIndex; };
            stackerRR.Children.Add(comboToeRR);
            Grid.SetColumn(stackerRR, 3);

            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetCasterStacker(int labelWidth)
        {
            // Caster inputs 
            Grid grid = GetMainGrid("Caster", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(2, blockWidth + 30);
            Grid.SetColumn(settings, 1);

            // LF
            StackPanel stackerCasterLF = new StackPanel() { Orientation = Orientation.Horizontal };
            stackerCasterLF.Children.Add(new Label() { Content = "FL" });
            ComboBox comboCasterLF = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboCasterLF.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.Caster);
            comboCasterLF.SelectedIndex = Setup.BasicSetup.Alignment.CasterLF;
            comboCasterLF.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.CasterLF = comboCasterLF.SelectedIndex; };
            stackerCasterLF.Children.Add(comboCasterLF);
            Grid.SetColumn(stackerCasterLF, 0);

            // RF
            StackPanel stackerCasterRF = new StackPanel() { Orientation = Orientation.Horizontal };
            stackerCasterRF.Children.Add(new Label() { Content = "FR" });
            ComboBox comboCasterRF = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboCasterRF.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.Caster);
            comboCasterRF.SelectedIndex = Setup.BasicSetup.Alignment.CasterRF;
            comboCasterRF.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.CasterRF = comboCasterRF.SelectedIndex; };
            stackerCasterRF.Children.Add(comboCasterRF);
            Grid.SetColumn(stackerCasterRF, 1);

            settings.Children.Add(stackerCasterLF);
            settings.Children.Add(stackerCasterRF);

            grid.Children.Add(settings);

            return grid;
        }

        private Grid GetToeStacker(int labelWidth)
        {
            Grid grid = GetMainGrid("Toe", labelWidth);

            int blockWidth = 65;

            Grid settings = GetGrid(4, blockWidth + 30);
            Grid.SetColumn(settings, 1);

            // FL
            StackPanel stackerFL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFL.Children.Add(new Label() { Content = "FL" });
            ComboBox comboCasterFL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboCasterFL.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.ToeFront);
            comboCasterFL.SelectedIndex = Setup.BasicSetup.Alignment.Toe[(int)Wheel.FrontLeft];
            comboCasterFL.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.FrontLeft] = comboCasterFL.SelectedIndex; };
            stackerFL.Children.Add(comboCasterFL);
            Grid.SetColumn(stackerFL, 0);

            // FR
            StackPanel stackerFR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerFR.Children.Add(new Label() { Content = "FR" });
            ComboBox comboCasterFR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboCasterFR.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.ToeFront);
            comboCasterFR.SelectedIndex = Setup.BasicSetup.Alignment.Toe[(int)Wheel.FrontRight];
            comboCasterFR.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.FrontRight] = comboCasterFR.SelectedIndex; };
            stackerFR.Children.Add(comboCasterFR);
            Grid.SetColumn(stackerFR, 1);

            // RL
            StackPanel stackerRL = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRL.Children.Add(new Label() { Content = "RL" });
            ComboBox comboCasterRL = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right }; ;
            comboCasterRL.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.ToeRear);
            comboCasterRL.SelectedIndex = Setup.BasicSetup.Alignment.Toe[(int)Wheel.RearLeft];
            comboCasterRL.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.RearLeft] = comboCasterRL.SelectedIndex; };
            stackerRL.Children.Add(comboCasterRL);
            Grid.SetColumn(stackerRL, 2);

            // RR
            StackPanel stackerRR = new StackPanel { Orientation = Orientation.Horizontal };
            stackerRR.Children.Add(new Label() { Content = "RR" });
            ComboBox comboCasterRR = new ComboBox() { Width = blockWidth, HorizontalContentAlignment = HorizontalAlignment.Right };
            comboCasterRR.ItemsSource = SetupDoubleRange.GetOptionsCollection(SetupChanger.TyreSetupChanger.ToeRear);
            comboCasterRR.SelectedIndex = Setup.BasicSetup.Alignment.Toe[(int)Wheel.RearRight];
            comboCasterRR.SelectionChanged += (s, e) => { Setup.BasicSetup.Alignment.Camber[(int)Wheel.RearRight] = comboCasterRR.SelectedIndex; };
            stackerRR.Children.Add(comboCasterRR);
            Grid.SetColumn(stackerRR, 3);


            settings.Children.Add(stackerFL);
            settings.Children.Add(stackerFR);
            settings.Children.Add(stackerRL);
            settings.Children.Add(stackerRR);

            grid.Children.Add(settings);

            return grid;
        }
        #endregion

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

        private TextBlock GetTitle(string label)
        {
            return new TextBlock()
            {
                Text = label,
                HorizontalAlignment = HorizontalAlignment.Center,
                Style = Resources["MaterialDesignHeadline6TextBlock"] as Style,
                TextDecorations = { TextDecorations.Underline },
                Margin = new Thickness(0, 2, 0, 0),
                FontSize = 16
            };
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
