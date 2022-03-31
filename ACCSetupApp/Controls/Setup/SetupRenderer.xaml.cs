using Newtonsoft.Json;
using SetupParser.Cars.GT3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();



            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".json";
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                LogSetup(filename);
            }


            //string b = @"C:\Users\Reinier\Documents\Assetto Corsa Competizione\Setups\porsche_991ii_gt3_r\misano\S3.json";
            //ThreadPool.QueueUserWorkItem(new WaitCallback(y => LogSetup(b))); 
        }

        private void LogSetup(string file)
        {
            //string b = @"C:\Users\Reinier\Documents\Assetto Corsa Competizione\Setups\porsche_991ii_gt3_r\misano\S3.json";
            //file = b;

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



           

            Paragraph para = GetDefaultParagraph();


            Debug.WriteLine($"\n\n@@@@ Setup Converted @@@@");
            para.Inlines.Add($"\n -- File -- \n\t {file}");
            para.Inlines.Add($"\n -- Car -- \n\t {carSetup.CarName}");

            para.Inlines.Add($"\n---- Tyres Setup -----");
            para.Inlines.Add($"\n -- Compound -- \n\t {compound}");
            para.Inlines.Add($"\n -- PSI -- \n\t FL: {frontLeftPressure}, FR: {frontRightPressure}, RL: {rearLeftPressure}, RR: {rearRightPressure}");
            para.Inlines.Add($"\n -- Caster -- \n\t FL: {frontLeftCaster}, FR: {frontRightCaster}");
            para.Inlines.Add($"\n -- Toe -- \n\t FL: {frontLeftToe}, FR: {frontRightToe}, RL: {rearLeftToe}, RR: {rearRightToe}");
            para.Inlines.Add($"\n -- Camber -- \n\t FL: {camberFrontLeft}, FR: {camberFrontRight}, RL: {camberRearLeft}, RR: {camberRearRight}");

            para.Inlines.Add($"\n---- Mechanical Grip ----");
            para.Inlines.Add($"\n -- WheelRates (Nm) -- \n\t FL: {wheelRateFrontLeft}, FR: {wheelRateFrontRight}, RL: {wheelRateRearLeft}, RR: {wheelRateRearRight}");
            para.Inlines.Add($"\n -- Bumpstop Rate (Nm) -- \n\t FL: {bumpStopRateFrontLeft}, FR: {bumpStopRateFrontRight}, RL: {bumpStopRateRearLeft}, RR: {bumpStopRateRearRight}");
            para.Inlines.Add($"\n -- Bumpstop range -- \n\t FL: {bumpStopRangeFrontLeft}, FR: {bumpStopRangeFrontRight}, RL: {bumpStopRangeRearLeft}, RR: {bumpStopRangeRearRight}");
            para.Inlines.Add($"\n -- Differential Preload (Nm) -- \n\t {differentialPreload}");
            para.Inlines.Add($"\n -- Brake Power (%) -- \n\t {brakePower}%");
            para.Inlines.Add($"\n -- Brake Bias (%) -- \n\t {brakeBias}%");
            para.Inlines.Add($"\n -- Anti Roll Bar-- \n\t Front: {antiRollBarFront}, Rear: {antiRollBarRear}");
            para.Inlines.Add($"\n -- Steering Ratio -- \n\t {steeringRatio}");

            para.Inlines.Add($"\n---- Aero Balance ----");
            para.Inlines.Add($"\n -- Ride Height (mm)-- \n\t Front: {rideHeightFront}, Rear: {rideHeightRear}");

            para.BorderBrush = Brushes.White;
            para.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(para);
        }

        private Paragraph GetDefaultParagraph()
        {

            Paragraph content = new Paragraph();
            content.FontSize = 13;
            content.FontWeight = FontWeights.Medium;
            content.Foreground = Brushes.White;
            return content;
        }

        private List GetDefaultList()
        {
            List list = new List();
            list.FontSize = 13;
            list.Foreground = Brushes.White;

            return list;
        }
    }
}
