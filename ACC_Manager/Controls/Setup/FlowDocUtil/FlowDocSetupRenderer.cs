using ACCManager.Controls.Setup.FlowDocUtil;
using ACCManager.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;
using static ACCManager.Data.SetupJson;

namespace ACCManager.Controls.Setup
{
    public class FlowDocSetupRenderer
    {
        public void LogSetup(ref FlowDocument flowDocument, string file, bool logTrack = true)
        {
            flowDocument.Blocks.Clear();

            Root setup = GetSetupJsonRoot(file);
            if (setup == null) return;

            CarModels model = ConversionFactory.ParseCarName(setup.CarName);
            if (model == CarModels.None) return;
            ICarSetupConversion carSetup = ConversionFactory.GetConversion(model);

            if (carSetup == null)
                return;

            TyreCompound compound = carSetup.TyresSetup.Compound(setup.BasicSetup.Tyres.TyreCompound);

            // Allignment / Tyre Setup
            double frontLeftPressure = carSetup.TyresSetup.TirePressure(carSetup.DryTyreCompound, Wheel.FrontLeft, setup.BasicSetup.Tyres.TyrePressure);
            double frontRightPressure = carSetup.TyresSetup.TirePressure(carSetup.DryTyreCompound, Wheel.FrontRight, setup.BasicSetup.Tyres.TyrePressure);
            double rearLeftPressure = carSetup.TyresSetup.TirePressure(carSetup.DryTyreCompound, Wheel.RearLeft, setup.BasicSetup.Tyres.TyrePressure);
            double rearRightPressure = carSetup.TyresSetup.TirePressure(carSetup.DryTyreCompound, Wheel.RearRight, setup.BasicSetup.Tyres.TyrePressure);

            double frontLeftCaster = carSetup.TyresSetup.Caster(setup.BasicSetup.Alignment.CasterLF);
            double frontRightCaster = carSetup.TyresSetup.Caster(setup.BasicSetup.Alignment.CasterRF);

            double frontLeftToe = carSetup.TyresSetup.Toe(Wheel.FrontLeft, setup.BasicSetup.Alignment.Toe);
            double frontRightToe = carSetup.TyresSetup.Toe(Wheel.FrontRight, setup.BasicSetup.Alignment.Toe);
            double rearLeftToe = carSetup.TyresSetup.Toe(Wheel.RearLeft, setup.BasicSetup.Alignment.Toe);
            double rearRightToe = carSetup.TyresSetup.Toe(Wheel.RearRight, setup.BasicSetup.Alignment.Toe);

            double camberFrontLeft = carSetup.TyresSetup.Camber(Wheel.FrontLeft, setup.BasicSetup.Alignment.Camber);
            double camberFrontRight = carSetup.TyresSetup.Camber(Wheel.FrontRight, setup.BasicSetup.Alignment.Camber);
            double camberRearLeft = carSetup.TyresSetup.Camber(Wheel.RearLeft, setup.BasicSetup.Alignment.Camber);
            double camberRearRight = carSetup.TyresSetup.Camber(Wheel.RearRight, setup.BasicSetup.Alignment.Camber);


            // Mechnical Setup
            int wheelRateFrontLeft = carSetup.MechanicalSetup.WheelRate(setup.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.FrontLeft);
            int wheelRateFrontRight = carSetup.MechanicalSetup.WheelRate(setup.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.FrontRight);
            int wheelRateRearLeft = carSetup.MechanicalSetup.WheelRate(setup.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.RearLeft);
            int wheelRateRearRight = carSetup.MechanicalSetup.WheelRate(setup.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.RearRight);


            int bumpStopRateFrontLeft = carSetup.MechanicalSetup.BumpstopRate(setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.FrontLeft);
            int bumpStopRateFrontRight = carSetup.MechanicalSetup.BumpstopRate(setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.FrontRight);
            int bumpStopRateRearLeft = carSetup.MechanicalSetup.BumpstopRate(setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.RearLeft);
            int bumpStopRateRearRight = carSetup.MechanicalSetup.BumpstopRate(setup.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.RearRight);

            int bumpStopRangeFrontLeft = carSetup.MechanicalSetup.BumpstopRange(setup.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.FrontLeft);
            int bumpStopRangeFrontRight = carSetup.MechanicalSetup.BumpstopRange(setup.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.FrontRight);
            int bumpStopRangeRearLeft = carSetup.MechanicalSetup.BumpstopRange(setup.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.RearLeft);
            int bumpStopRangeRearRight = carSetup.MechanicalSetup.BumpstopRange(setup.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.RearRight);

            int differentialPreload = carSetup.MechanicalSetup.PreloadDifferential(setup.AdvancedSetup.Drivetrain.Preload);

            int brakePower = carSetup.MechanicalSetup.BrakePower(setup.AdvancedSetup.MechanicalBalance.BrakeTorque);
            double brakeBias = carSetup.MechanicalSetup.BrakeBias(setup.AdvancedSetup.MechanicalBalance.BrakeBias);
            int antiRollBarFront = carSetup.MechanicalSetup.AntiRollBarFront(setup.AdvancedSetup.MechanicalBalance.ARBFront);
            int antiRollBarRear = carSetup.MechanicalSetup.AntiRollBarFront(setup.AdvancedSetup.MechanicalBalance.ARBRear);
            double steeringRatio = carSetup.MechanicalSetup.SteeringRatio(setup.BasicSetup.Alignment.SteerRatio);


            // Dampers
            int bumpSlowFrontLeft = carSetup.DamperSetup.BumpSlow(setup.AdvancedSetup.Dampers.BumpSlow, Wheel.FrontLeft);
            int bumpSlowFrontRight = carSetup.DamperSetup.BumpSlow(setup.AdvancedSetup.Dampers.BumpSlow, Wheel.FrontRight);
            int bumpSlowRearLeft = carSetup.DamperSetup.BumpSlow(setup.AdvancedSetup.Dampers.BumpSlow, Wheel.RearLeft);
            int bumpSlowRearRight = carSetup.DamperSetup.BumpSlow(setup.AdvancedSetup.Dampers.BumpSlow, Wheel.RearRight);

            int bumpFastFrontLeft = carSetup.DamperSetup.BumpFast(setup.AdvancedSetup.Dampers.BumpFast, Wheel.FrontLeft);
            int bumpFastFrontRight = carSetup.DamperSetup.BumpFast(setup.AdvancedSetup.Dampers.BumpFast, Wheel.FrontRight);
            int bumpFastRearLeft = carSetup.DamperSetup.BumpFast(setup.AdvancedSetup.Dampers.BumpFast, Wheel.RearLeft);
            int bumpFastRearRight = carSetup.DamperSetup.BumpFast(setup.AdvancedSetup.Dampers.BumpFast, Wheel.RearRight);

            int reboundSlowFrontLeft = carSetup.DamperSetup.ReboundSlow(setup.AdvancedSetup.Dampers.ReboundSlow, Wheel.FrontLeft);
            int reboundSlowFrontRight = carSetup.DamperSetup.ReboundSlow(setup.AdvancedSetup.Dampers.ReboundSlow, Wheel.FrontRight);
            int reboundSlowRearLeft = carSetup.DamperSetup.ReboundSlow(setup.AdvancedSetup.Dampers.ReboundSlow, Wheel.RearLeft);
            int reboundSlowRearRight = carSetup.DamperSetup.ReboundSlow(setup.AdvancedSetup.Dampers.ReboundSlow, Wheel.RearRight);

            int reboundFastFrontLeft = carSetup.DamperSetup.ReboundFast(setup.AdvancedSetup.Dampers.ReboundFast, Wheel.FrontLeft);
            int reboundFastFrontRight = carSetup.DamperSetup.ReboundFast(setup.AdvancedSetup.Dampers.ReboundFast, Wheel.FrontRight);
            int reboundFastRearLeft = carSetup.DamperSetup.ReboundFast(setup.AdvancedSetup.Dampers.ReboundFast, Wheel.RearLeft);
            int reboundFastRearRight = carSetup.DamperSetup.ReboundFast(setup.AdvancedSetup.Dampers.ReboundFast, Wheel.RearRight);



            // Aero Balance
            int rideHeightFront = carSetup.AeroBalance.RideHeight(setup.AdvancedSetup.AeroBalance.RideHeight, Position.Front);
            int rideHeightRear = carSetup.AeroBalance.RideHeight(setup.AdvancedSetup.AeroBalance.RideHeight, Position.Rear);
            int rearWing = carSetup.AeroBalance.RearWing(setup.AdvancedSetup.AeroBalance.RearWing);
            int splitter = carSetup.AeroBalance.Splitter(setup.AdvancedSetup.AeroBalance.Splitter);
            int brakeDuctsFront = setup.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Front];
            int brakeDuctsRear = setup.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Rear];



            const int cells = 8;
            const int headerWidthPercent = 20;
            string[] tyreLocationLabels = new string[] { "FL: ", "FR: ", "RL: ", "RR: " };
            string[] frontOrRearLabels = new string[] { "Front: ", "Rear: " };


            //// Setup Info Section
            Table setupInfoTable = DocUtil.GetLeftAllignedTable(headerWidthPercent, cells);
            TableRowGroup rowGroupSetupInfo = new TableRowGroup();
            if (logTrack)
                rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRowLeft("Track", $"{DocUtil.GetTrackName(file)}", cells));
            rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRowLeft("Car", $"{CarModelToCarName[carSetup.CarModel]}", cells));
            rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRowLeft("Class", $"{carSetup.CarClass}", cells));
            setupInfoTable.RowGroups.Add(rowGroupSetupInfo);
            Section setupSection = new Section
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(0, 1, 0, 1),
                Margin = new Thickness(0, 0, 0, 0)
            };
            setupSection.Blocks.Add(setupInfoTable);
            flowDocument.Blocks.Add(setupSection);


            //// Tyres Section
            Section tyresSection = new Section();
            TableRowGroup rgTyres = new TableRowGroup();
            rgTyres.Rows.Add(DocUtil.GetTableRowLeftCenterTitle("Tyres Setup", (int)(cells * 0.85)));
            rgTyres.Rows.Add(DocUtil.GetTableRowLeft("Compound", $"{compound}", cells));
            rgTyres.Rows.Add(DocUtil.GetTableRowLeft("Pressures(psi)", tyreLocationLabels, new double[] { frontLeftPressure, frontRightPressure, rearLeftPressure, rearRightPressure }, cells, 1));
            rgTyres.Rows.Add(DocUtil.GetTableRowLeft("Toe(°)", tyreLocationLabels, new double[] { frontLeftToe, frontRightToe, rearLeftToe, rearRightToe }, cells, 2));
            rgTyres.Rows.Add(DocUtil.GetTableRowLeft("Camber(°)", tyreLocationLabels, new double[] { camberFrontLeft, camberFrontRight, camberRearLeft, camberRearRight }, cells, 1));
            rgTyres.Rows.Add(DocUtil.GetTableRowLeft("Caster(°)", tyreLocationLabels.Take(2).ToArray(), new double[] { frontLeftCaster, frontRightCaster }, cells, 1));
            Table tyresSetupTable = DocUtil.GetLeftAllignedTable(headerWidthPercent, cells);
            tyresSetupTable.RowGroups.Add(rgTyres);
            tyresSection.Blocks.Add(tyresSetupTable);
            tyresSection.BorderBrush = Brushes.White;
            tyresSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(tyresSection);

            //// Mechanical Grip Section
            Section gripSection = new Section();
            TableRowGroup rgGrip = new TableRowGroup();
            rgGrip.Rows.Add(DocUtil.GetTableRowLeftCenterTitle("Mechanical Grip", (int)(cells * 0.85)));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Wheelrates(Nm)", tyreLocationLabels, new double[] { wheelRateFrontLeft, wheelRateFrontRight, wheelRateRearLeft, wheelRateRearRight }, cells, 0));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Bumpstop Rate(Nm)", tyreLocationLabels, new double[] { bumpStopRateFrontLeft, bumpStopRateFrontRight, bumpStopRateRearLeft, bumpStopRateRearRight }, cells, 0));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Bumpstop Range", tyreLocationLabels, new double[] { bumpStopRangeFrontLeft, bumpStopRangeFrontRight, bumpStopRangeRearLeft, bumpStopRangeRearRight }, cells, 0));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Anti roll bar", frontOrRearLabels, new double[] { antiRollBarFront, antiRollBarRear }, cells, 0));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Diff Preload(Nm)", differentialPreload, cells));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Brake Power(%)", brakePower, cells));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Brake Bias(%)", brakeBias, cells, 1));
            rgGrip.Rows.Add(DocUtil.GetTableRowLeft("Steering Ratio", steeringRatio, cells));
            Table gripTable = DocUtil.GetLeftAllignedTable(headerWidthPercent, cells); ;
            gripTable.RowGroups.Add(rgGrip);
            gripSection.Blocks.Add(gripTable);
            gripSection.BorderBrush = Brushes.White;
            gripSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(gripSection);



            //// Dampers Section
            Section dampersSection = new Section();
            TableRowGroup rgDampers = new TableRowGroup();
            rgDampers.Rows.Add(DocUtil.GetTableRowLeftCenterTitle("Dampers", (int)(cells * 0.85)));
            rgDampers.Rows.Add(DocUtil.GetTableRowLeft("Bump Slow", tyreLocationLabels, new double[] { bumpSlowFrontLeft, bumpSlowFrontRight, bumpSlowRearLeft, bumpSlowRearRight }, cells));
            rgDampers.Rows.Add(DocUtil.GetTableRowLeft("Bump Fast", tyreLocationLabels, new double[] { bumpFastFrontLeft, bumpFastFrontRight, bumpFastRearLeft, bumpFastRearRight }, cells));
            rgDampers.Rows.Add(DocUtil.GetTableRowLeft("Rebound Slow", tyreLocationLabels, new double[] { reboundSlowFrontLeft, reboundSlowFrontRight, reboundSlowRearLeft, reboundSlowRearRight }, cells));
            rgDampers.Rows.Add(DocUtil.GetTableRowLeft("Rebound Fast", tyreLocationLabels, new double[] { reboundFastFrontLeft, reboundFastFrontRight, reboundFastRearLeft, reboundFastRearRight }, cells));
            Table dampersTable = DocUtil.GetLeftAllignedTable(headerWidthPercent, cells);
            dampersTable.RowGroups.Add(rgDampers);
            dampersSection.Blocks.Add(dampersTable);
            dampersSection.BorderBrush = Brushes.White;
            dampersSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(dampersSection);


            //// Aero
            Section aeroBalanceSection = new Section();
            TableRowGroup rgAero = new TableRowGroup();
            rgAero.Rows.Add(DocUtil.GetTableRowLeftCenterTitle("Aero Balance", (int)(cells * 0.85)));
            rgAero.Rows.Add(DocUtil.GetTableRowLeft("Ride height(mm)", frontOrRearLabels, new double[] { rideHeightFront, rideHeightRear }, cells));
            rgAero.Rows.Add(DocUtil.GetTableRowLeft("Splitter", splitter, cells));
            rgAero.Rows.Add(DocUtil.GetTableRowLeft("Rear Wing", rearWing, cells));
            rgAero.Rows.Add(DocUtil.GetTableRowLeft("Brake ducts", frontOrRearLabels, new double[] { brakeDuctsFront, brakeDuctsRear }, cells));
            Table aeroTable = DocUtil.GetLeftAllignedTable(headerWidthPercent, cells);
            aeroTable.RowGroups.Add(rgAero);
            aeroBalanceSection.Blocks.Add(aeroTable);
            aeroBalanceSection.BorderBrush = Brushes.White;
            aeroBalanceSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(aeroBalanceSection);

            //// Electronics
            Section electronicsSection = new Section();
            TableRowGroup rgElectro = new TableRowGroup();
            rgElectro.Rows.Add(DocUtil.GetTableRowLeftCenterTitle("Electronics", (int)(cells * 0.85)));
            rgElectro.Rows.Add(DocUtil.GetTableRowLeft("TC 1", setup.BasicSetup.Electronics.TC1, cells));
            rgElectro.Rows.Add(DocUtil.GetTableRowLeft("TC ", setup.BasicSetup.Electronics.TC2, cells));
            rgElectro.Rows.Add(DocUtil.GetTableRowLeft("ABS", setup.BasicSetup.Electronics.Abs, cells));
            rgElectro.Rows.Add(DocUtil.GetTableRowLeft("Engine map", setup.BasicSetup.Electronics.ECUMap + 1, cells));
            Table electroTable = DocUtil.GetLeftAllignedTable(headerWidthPercent, cells);
            electroTable.RowGroups.Add(rgElectro);
            electronicsSection.Blocks.Add(electroTable);
            electronicsSection.BorderBrush = Brushes.White;
            electronicsSection.BorderThickness = new Thickness(0, 1, 0, 1);
            flowDocument.Blocks.Add(electronicsSection);
        }
    }
}
