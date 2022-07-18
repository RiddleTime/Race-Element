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



            Section setupTitle = new Section();
            setupTitle.Blocks.Add(DocUtil.GetDefaultHeader(20, $"{new FileInfo(file).Name.Replace(".json", "")}"));

            setupTitle.BorderBrush = Brushes.White;
            setupTitle.BorderThickness = new Thickness(1, 1, 1, 1);
            setupTitle.Margin = new Thickness(0, 0, 0, 0);

            flowDocument.Blocks.Add(setupTitle);

            // Setup Info
            Section setupSection = new Section();
            //setupSection.Blocks.Add(GetDefaultHeader("Setup Info"));

            Table setupInfoTable = DocUtil.GetTable(30, 70);
            TableRowGroup rowGroupSetupInfo = new TableRowGroup();
            if (logTrack)
                rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRow("Track", $"{DocUtil.GetTrackName(file)}"));
            rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRow("Car", $"{CarModelToCarName[carSetup.CarModel]}"));
            rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRow("Class", $"{carSetup.CarClass}"));

            setupInfoTable.RowGroups.Add(rowGroupSetupInfo);
            setupSection.Blocks.Add(setupInfoTable);
            setupSection.BorderBrush = Brushes.White;
            setupSection.BorderThickness = new Thickness(1, 1, 1, 1);
            setupSection.Margin = new Thickness(0, 0, 0, 0);
            flowDocument.Blocks.Add(setupSection);



            // Tyres setup
            Section tiresSection = new Section();
            tiresSection.Blocks.Add(DocUtil.GetDefaultHeader("Tyres Setup"));

            Table tiresTable = DocUtil.GetTable(30, 70);
            TableRowGroup rowGroupTires = new TableRowGroup();
            rowGroupTires.Rows.Add(DocUtil.GetTableRow("Compound", $"{compound}"));
            rowGroupTires.Rows.Add(DocUtil.GetTableRow("Pressures(psi)", $"FL: {frontLeftPressure}, FR: {frontRightPressure}, RL: {rearLeftPressure}, RR: {rearRightPressure}"));
            rowGroupTires.Rows.Add(DocUtil.GetTableRow("Toe(°)", $"FL: {frontLeftToe}, FR: {frontRightToe}, RL: {rearLeftToe}, RR: {rearRightToe}"));
            rowGroupTires.Rows.Add(DocUtil.GetTableRow("Camber(°)", $"FL: {camberFrontLeft}, FR: {camberFrontRight}, RL: {camberRearLeft}, RR: {camberRearRight}"));
            rowGroupTires.Rows.Add(DocUtil.GetTableRow("Caster(°)", $"FL: {frontLeftCaster}, FR: {frontRightCaster}"));
            tiresTable.RowGroups.Add(rowGroupTires);
            tiresSection.Blocks.Add(tiresTable);
            tiresSection.BorderBrush = Brushes.White;
            tiresSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(tiresSection);


            // Mechanical grip
            Section mechanicalGripSection = new Section();
            mechanicalGripSection.Blocks.Add(DocUtil.GetDefaultHeader("Mechanical Grip"));

            Table gripTable = DocUtil.GetTable(30, 70);
            TableRowGroup rowGroupGrip = new TableRowGroup();
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Wheelrates(Nm)", $"FL: {wheelRateFrontLeft}, FR: {wheelRateFrontRight}, RL: {wheelRateRearLeft}, RR: {wheelRateRearRight}"));
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Bumpstop rate(Nm)", $"FL: {bumpStopRateFrontLeft}, FR: {bumpStopRateFrontRight}, RL: {bumpStopRateRearLeft}, RR: {bumpStopRateRearRight}"));
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Bumstop range", $"FL: {bumpStopRangeFrontLeft}, FR: {bumpStopRangeFrontRight}, RL: {bumpStopRangeRearLeft}, RR: {bumpStopRangeRearRight}"));
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Anti roll bar", $"Front: {antiRollBarFront}, Rear: {antiRollBarRear}"));
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Diff preload(Nm)", $"{differentialPreload}"));
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Brake power", $"{brakePower}%"));
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Brake bias", $"{brakeBias}%"));
            rowGroupGrip.Rows.Add(DocUtil.GetTableRow("Steering Ratio", $"{steeringRatio}"));

            gripTable.RowGroups.Add(rowGroupGrip);
            mechanicalGripSection.Blocks.Add(gripTable);
            mechanicalGripSection.BorderBrush = Brushes.White;
            mechanicalGripSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(mechanicalGripSection);


            // Dampers
            Section dampersSection = new Section();
            dampersSection.Blocks.Add(DocUtil.GetDefaultHeader("Dampers"));

            Table dampersTable = DocUtil.GetTable(30, 70);
            TableRowGroup rowGroupDampers = new TableRowGroup();
            rowGroupDampers.Rows.Add(DocUtil.GetTableRow("Bump Slow", $"FL: {bumpSlowFrontLeft}, FR: {bumpSlowFrontRight}, RL: {bumpSlowRearLeft}, RR: {bumpSlowRearRight}"));
            rowGroupDampers.Rows.Add(DocUtil.GetTableRow("Bump Fast", $"FL: {bumpFastFrontLeft}, FR: {bumpFastFrontRight}, RL: {bumpFastRearLeft}, RR: {bumpFastRearRight}"));
            rowGroupDampers.Rows.Add(DocUtil.GetTableRow("Rebound Slow", $"FL: {reboundSlowFrontLeft}, FR: {reboundSlowFrontRight}, RL: {reboundSlowRearLeft}, RR: {reboundSlowRearRight}"));
            rowGroupDampers.Rows.Add(DocUtil.GetTableRow("Rebound Fast", $"FL: {reboundFastFrontLeft}, FR: {reboundFastFrontRight}, RL: {reboundFastRearLeft}, RR: {reboundFastRearRight}"));

            dampersTable.RowGroups.Add(rowGroupDampers);
            dampersSection.Blocks.Add(dampersTable);
            dampersSection.BorderBrush = Brushes.White;
            dampersSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(dampersSection);


            // Aero
            Section aeroBalanceSection = new Section();
            aeroBalanceSection.Blocks.Add(DocUtil.GetDefaultHeader("Aero Balance"));
            Table aeroTable = DocUtil.GetTable(30, 70);
            TableRowGroup aeroTableRowGroup = new TableRowGroup();
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Ride height(mm)", $"Front: {rideHeightFront}, Rear: {rideHeightRear}"));
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Splitter", $"{splitter}"));
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Rear Wing", $"{rearWing}"));
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Brake ducts", $"Front: {brakeDuctsFront}, Rear: {brakeDuctsRear}"));


            aeroTable.RowGroups.Add(aeroTableRowGroup);
            aeroBalanceSection.Blocks.Add(aeroTable);
            aeroBalanceSection.BorderBrush = Brushes.White;
            aeroBalanceSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(aeroBalanceSection);


            // Electronics
            Section electronicsSection = new Section();
            electronicsSection.Blocks.Add(DocUtil.GetDefaultHeader("Electronics"));
            Table electronicsTable = DocUtil.GetTable(30, 70);
            TableRowGroup electronicsRowGroup = new TableRowGroup();
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("TC 1", $"{setup.BasicSetup.Electronics.TC1}"));
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("TC 2", $"{setup.BasicSetup.Electronics.TC2}"));
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("ABS", $"{setup.BasicSetup.Electronics.Abs}"));
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("Engine map", $"{setup.BasicSetup.Electronics.ECUMap + 1}"));

            electronicsTable.RowGroups.Add(electronicsRowGroup);
            electronicsSection.Blocks.Add(electronicsTable);
            electronicsSection.BorderBrush = Brushes.White;
            electronicsSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(electronicsSection);
        }


    }
}
