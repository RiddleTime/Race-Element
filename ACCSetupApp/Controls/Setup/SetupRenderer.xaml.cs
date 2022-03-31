using Newtonsoft.Json;
using SetupParser.Cars.GT3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using static SetupParser.SetupConverter;
using static SetupParser.SetupJson;

namespace ACCSetupApp.Controls
{
    /// <summary>
    /// Interaction logic for SetupRenderer.xaml
    /// 
    /// </summary>
    public partial class SetupRenderer : UserControl
    {
        public SetupRenderer()
        {
            InitializeComponent();


            openFile.Click += OpenFile_Click;

        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".json";
            dlg.Filter = "ACC Setup files|*.json";
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                flowDocument.Blocks.Clear();

                // Open document 
                string filename = dlg.FileName;
                LogSetup(filename);
            }
        }

        private void LogSetup(string file)
        {
            FileInfo jsonFile = new FileInfo(file);
            if (!jsonFile.Exists)
                return;

            string jsonString = string.Empty;
            try
            {
                using (FileStream fileStream = jsonFile.OpenRead())
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

            ICarSetupConversion carSetup = new Porsche911II();

            TyreCompound compound = carSetup.TyresSetup.Compound(setup.basicSetup.tyres.tyreCompound);

            // Allignment / Tyre Setup
            double frontLeftPressure = carSetup.TyresSetup.TirePressure(carSetup.CarClass, Wheel.FrontLeft, setup.basicSetup.tyres.tyrePressure);
            double frontRightPressure = carSetup.TyresSetup.TirePressure(carSetup.CarClass, Wheel.FrontRight, setup.basicSetup.tyres.tyrePressure);
            double rearLeftPressure = carSetup.TyresSetup.TirePressure(carSetup.CarClass, Wheel.RearLeft, setup.basicSetup.tyres.tyrePressure);
            double rearRightPressure = carSetup.TyresSetup.TirePressure(carSetup.CarClass, Wheel.RearRight, setup.basicSetup.tyres.tyrePressure);

            double frontLeftCaster = carSetup.TyresSetup.Caster(setup.basicSetup.alignment.casterLF);
            double frontRightCaster = carSetup.TyresSetup.Caster(setup.basicSetup.alignment.casterRF);

            double frontLeftToe = carSetup.TyresSetup.Toe(Wheel.FrontLeft, setup.basicSetup.alignment.toe);
            double frontRightToe = carSetup.TyresSetup.Toe(Wheel.FrontRight, setup.basicSetup.alignment.toe);
            double rearLeftToe = carSetup.TyresSetup.Toe(Wheel.RearLeft, setup.basicSetup.alignment.toe);
            double rearRightToe = carSetup.TyresSetup.Toe(Wheel.RearRight, setup.basicSetup.alignment.toe);

            double camberFrontLeft = carSetup.TyresSetup.Camber(Wheel.FrontLeft, setup.basicSetup.alignment.camber);
            double camberFrontRight = carSetup.TyresSetup.Camber(Wheel.FrontRight, setup.basicSetup.alignment.camber);
            double camberRearLeft = carSetup.TyresSetup.Camber(Wheel.RearLeft, setup.basicSetup.alignment.camber);
            double camberRearRight = carSetup.TyresSetup.Camber(Wheel.RearRight, setup.basicSetup.alignment.camber);


            // Mechnical Setup
            int wheelRateFrontLeft = carSetup.MechanicalSetup.WheelRate(setup.advancedSetup.mechanicalBalance.wheelRate, Wheel.FrontLeft);
            int wheelRateFrontRight = carSetup.MechanicalSetup.WheelRate(setup.advancedSetup.mechanicalBalance.wheelRate, Wheel.FrontRight);
            int wheelRateRearLeft = carSetup.MechanicalSetup.WheelRate(setup.advancedSetup.mechanicalBalance.wheelRate, Wheel.RearLeft);
            int wheelRateRearRight = carSetup.MechanicalSetup.WheelRate(setup.advancedSetup.mechanicalBalance.wheelRate, Wheel.RearRight);


            int bumpStopRateFrontLeft = carSetup.MechanicalSetup.BumpstopRate(setup.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.FrontLeft);
            int bumpStopRateFrontRight = carSetup.MechanicalSetup.BumpstopRate(setup.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.FrontRight);
            int bumpStopRateRearLeft = carSetup.MechanicalSetup.BumpstopRate(setup.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.RearLeft);
            int bumpStopRateRearRight = carSetup.MechanicalSetup.BumpstopRate(setup.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.RearRight);

            int bumpStopRangeFrontLeft = carSetup.MechanicalSetup.BumpstopRange(setup.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.FrontLeft);
            int bumpStopRangeFrontRight = carSetup.MechanicalSetup.BumpstopRange(setup.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.FrontRight);
            int bumpStopRangeRearLeft = carSetup.MechanicalSetup.BumpstopRange(setup.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.RearLeft);
            int bumpStopRangeRearRight = carSetup.MechanicalSetup.BumpstopRange(setup.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.RearRight);

            int differentialPreload = carSetup.MechanicalSetup.PreloadDifferential(setup.advancedSetup.drivetrain.preload);

            int brakePower = carSetup.MechanicalSetup.BrakePower(setup.advancedSetup.mechanicalBalance.brakeTorque);
            double brakeBias = carSetup.MechanicalSetup.BrakeBias(setup.advancedSetup.mechanicalBalance.brakeBias);
            int antiRollBarFront = carSetup.MechanicalSetup.AntiRollBarFront(setup.advancedSetup.mechanicalBalance.aRBFront);
            int antiRollBarRear = carSetup.MechanicalSetup.AntiRollBarFront(setup.advancedSetup.mechanicalBalance.aRBRear);
            double steeringRatio = carSetup.MechanicalSetup.SteeringRatio(setup.basicSetup.alignment.steerRatio);


            // Aero Balance
            int rideHeightFront = carSetup.AeroBalance.RideHeight(setup.advancedSetup.aeroBalance.rideHeight, Position.Front);
            int rideHeightRear = carSetup.AeroBalance.RideHeight(setup.advancedSetup.aeroBalance.rideHeight, Position.Rear);



            // Setup Info
            Section setupSection = new Section();
            setupSection.Blocks.Add(GetDefaultHeader("Setup Info"));

            Table setupInfoTable = GetTable(30, 70);
            TableRowGroup rowGroupSetupInfo = new TableRowGroup();
            rowGroupSetupInfo.Rows.Add(GetTableRow("Setup", $"{jsonFile.Name}"));
            rowGroupSetupInfo.Rows.Add(GetTableRow("Track", $"{GetTrackName(jsonFile.FullName)}"));
            rowGroupSetupInfo.Rows.Add(GetTableRow("Car", $"{carSetup.CarName}"));
            rowGroupSetupInfo.Rows.Add(GetTableRow("Class", $"{carSetup.CarClass}"));

            setupInfoTable.RowGroups.Add(rowGroupSetupInfo);
            setupSection.Blocks.Add(setupInfoTable);
            setupSection.BorderBrush = Brushes.White;
            setupSection.BorderThickness = new Thickness(1, 1, 1, 1);
            setupSection.Margin = new Thickness(0, 0, 0, 0);
            flowDocument.Blocks.Add(setupSection);



            // Tyres setup
            Section tiresSection = new Section();
            tiresSection.Blocks.Add(GetDefaultHeader("Tyres Setup"));

            Table tiresTable = GetTable(30, 70);
            TableRowGroup rowGroupTires = new TableRowGroup();
            rowGroupTires.Rows.Add(GetTableRow("Compound", $"{compound}"));
            rowGroupTires.Rows.Add(GetTableRow("PSI", $"FL: {frontLeftPressure}, FR: {frontRightPressure}, RL: {rearLeftPressure}, RR: {rearRightPressure}"));
            rowGroupTires.Rows.Add(GetTableRow("Caster", $"FL: {frontLeftCaster}, FR: {frontRightCaster}"));
            rowGroupTires.Rows.Add(GetTableRow("Toe", $"FL: {frontLeftToe}, FR: {frontRightToe}, RL: {rearLeftToe}, RR: {rearRightToe}"));
            rowGroupTires.Rows.Add(GetTableRow("Camber", $"FL: {camberFrontLeft}, FR: {camberFrontRight}, RL: {camberRearLeft}, RR: {camberRearRight}"));
            tiresTable.RowGroups.Add(rowGroupTires);
            tiresSection.Blocks.Add(tiresTable);
            tiresSection.BorderBrush = Brushes.White;
            tiresSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(tiresSection);


            // Mechanical grip
            Section mechanicalGripSection = new Section();
            mechanicalGripSection.Blocks.Add(GetDefaultHeader("Mechanical Grip"));

            Table gripTable = GetTable(30, 70);
            TableRowGroup rowGroupGrip = new TableRowGroup();
            rowGroupGrip.Rows.Add(GetTableRow("Wheelrates(Nm)", $"FL: {wheelRateFrontLeft}, FR: {wheelRateFrontRight}, RL: {wheelRateRearLeft}, RR: {wheelRateRearRight}"));
            rowGroupGrip.Rows.Add(GetTableRow("Bumpstop rate(Nm)", $"FL: {bumpStopRateFrontLeft}, FR: {bumpStopRateFrontRight}, RL: {bumpStopRateRearLeft}, RR: {bumpStopRateRearRight}"));
            rowGroupGrip.Rows.Add(GetTableRow("Bumstop range", $"FL: {bumpStopRangeFrontLeft}, FR: {bumpStopRangeFrontRight}, RL: {bumpStopRangeRearLeft}, RR: {bumpStopRangeRearRight}"));
            rowGroupGrip.Rows.Add(GetTableRow("Diff preload(Nm)", $"{differentialPreload}"));
            rowGroupGrip.Rows.Add(GetTableRow("Brake Power", $"{brakePower}%"));
            rowGroupGrip.Rows.Add(GetTableRow("Brake bias", $"{brakeBias}%"));
            rowGroupGrip.Rows.Add(GetTableRow("Anti roll bar", $"Front: {antiRollBarFront}, Rear: {antiRollBarRear}"));
            rowGroupGrip.Rows.Add(GetTableRow("Steering Ratio", $"{steeringRatio}"));

            gripTable.RowGroups.Add(rowGroupGrip);
            mechanicalGripSection.Blocks.Add(gripTable);
            mechanicalGripSection.BorderBrush = Brushes.White;
            mechanicalGripSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(mechanicalGripSection);


            // Aero
            Section aeroBalanceSection = new Section();
            aeroBalanceSection.Blocks.Add(GetDefaultHeader("Aero Balance"));
            Table aeroTable = GetTable(30, 70);
            TableRowGroup aeroTableRowGroup = new TableRowGroup();
            aeroTableRowGroup.Rows.Add(GetTableRow("Ride height(mm)", $"Front: {rideHeightFront}, Rear: {rideHeightRear}"));

            aeroTable.RowGroups.Add(aeroTableRowGroup);
            aeroBalanceSection.Blocks.Add(aeroTable);
            aeroBalanceSection.BorderBrush = Brushes.White;
            aeroBalanceSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(aeroBalanceSection);
        }

        private Table GetTable(int headerWidth, int valueWidth)
        {
            Table table = new Table();
            TableColumn columnTitle = new TableColumn();
            columnTitle.Width = new GridLength(headerWidth, GridUnitType.Star);
            table.Columns.Add(columnTitle);

            TableColumn columnValues = new TableColumn();
            columnValues.Width = new GridLength(valueWidth, GridUnitType.Star);
            table.Columns.Add(columnValues);

            table.Margin = new Thickness(0);

            return table;
        }

        private TableRow GetTableRow(string title, string value)
        {
            TableRow row = new TableRow();
            row.Cells.Add(new TableCell(GetDefaultParagraph(title)));
            row.Cells.Add(new TableCell(GetDefaultParagraph(value)));
            return row;
        }

        private Paragraph GetDefaultHeader()
        {
            Paragraph content = new Paragraph();
            content.FontSize = 17;
            content.FontWeight = FontWeights.Medium;
            content.Foreground = Brushes.White;
            content.TextAlignment = TextAlignment.Center;
            content.Margin = new Thickness(0, 0, 0, 2);
            return content;
        }

        private Paragraph GetDefaultHeader(double fontSize)
        {
            Paragraph content = GetDefaultHeader();
            content.FontSize = fontSize;
            return content;
        }

        private Paragraph GetDefaultHeader(string inlineText)
        {
            Paragraph content = GetDefaultHeader();
            content.Inlines.Add(inlineText);
            return content;
        }

        private Paragraph GetDefaultHeader(double fontSize, string inlineText)
        {
            Paragraph content = GetDefaultHeader();
            content.FontSize = fontSize;
            content.Inlines.Add(inlineText);
            return content;
        }

        private Paragraph GetDefaultParagraph()
        {
            Paragraph content = new Paragraph();
            content.FontSize = 12;
            content.FontWeight = FontWeights.Medium;
            content.Foreground = Brushes.White;
            content.Margin = new Thickness(0, 0, 0, 0);
            return content;
        }

        private Paragraph GetDefaultParagraph(double fontSize)
        {
            Paragraph content = GetDefaultParagraph();
            content.FontSize = fontSize;
            return content;
        }

        private Paragraph GetDefaultParagraph(string inlineText)
        {
            Paragraph content = GetDefaultParagraph();
            content.Inlines.Add(inlineText);
            return content;
        }

        private Paragraph GetDefaultParagraph(double fontSize, string inlineText)
        {
            Paragraph content = GetDefaultParagraph(fontSize);
            content.Inlines.Add(inlineText);
            return content;
        }

        private List GetDefaultList()
        {
            List list = new List();
            list.FontSize = 13;
            list.Foreground = Brushes.White;

            return list;
        }

        private string GetTrackName(string fileName)
        {
            string[] dashSplit = fileName.Split('\\');
            string trackName = dashSplit[dashSplit.Length - 2];
            trackName = Regex.Replace(trackName, "^[a-z]", m => m.Value.ToUpper());
            trackName = trackName.Replace("_", " ");
            return trackName;
        }

        /// <summary>
        /// Strips the file name from a windows directory path
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>removed the filename from the path is what it returns</returns>
        public static string StripFileName(string fileName)
        {
            string[] dashSplit = fileName.Split('\\');
            string result = String.Empty;

            for (int i = 0; i < dashSplit.Length - 1; i++)
            {
                result += dashSplit[i] + '\\';
            }

            return result;
        }
    }
}
