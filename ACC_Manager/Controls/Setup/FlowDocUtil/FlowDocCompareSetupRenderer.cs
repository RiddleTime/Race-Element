using ACCManager.Controls.Setup.FlowDocUtil;
using ACCManager.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static ACCManager.Data.ConversionFactory;
using static ACCManager.Data.SetupConverter;

namespace ACCManager.Controls.Setup
{
    public class FlowDocCompareSetupRenderer
    {
        public FlowDocCompareSetupRenderer()
        {

        }

        public void LogComparison(ref FlowDocument flowDocument, FileInfo setupFile1, FileInfo setupFile2)
        {
            flowDocument.Blocks.Clear();

            SetupJson.Root setup1 = GetSetupJsonRoot(setupFile1);
            SetupJson.Root setup2 = GetSetupJsonRoot(setupFile2);

            if (setup1 == null || setup2 == null)
            {
                return;
            }


            //// Car 1 conversions
            CarModels model1 = ConversionFactory.ParseCarName(DocUtil.GetParseName(setupFile1.FullName));
            if (model1 == CarModels.None) return;
            ICarSetupConversion carSetup1 = ConversionFactory.GetConversion(model1);
            if (carSetup1 == null) return;
            TyreCompound compound1 = carSetup1.TyresSetup.Compound(setup1.basicSetup.tyres.tyreCompound);

            // Allignment / Tyre Setup car 1
            double frontLeftPressure1 = carSetup1.TyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.FrontLeft, setup1.basicSetup.tyres.tyrePressure);
            double frontRightPressure1 = carSetup1.TyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.FrontRight, setup1.basicSetup.tyres.tyrePressure);
            double rearLeftPressure1 = carSetup1.TyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.RearLeft, setup1.basicSetup.tyres.tyrePressure);
            double rearRightPressure1 = carSetup1.TyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.RearRight, setup1.basicSetup.tyres.tyrePressure);

            double frontLeftCaster1 = carSetup1.TyresSetup.Caster(setup1.basicSetup.alignment.casterLF);
            double frontRightCaster1 = carSetup1.TyresSetup.Caster(setup1.basicSetup.alignment.casterRF);

            double frontLeftToe1 = carSetup1.TyresSetup.Toe(Wheel.FrontLeft, setup1.basicSetup.alignment.toe);
            double frontRightToe1 = carSetup1.TyresSetup.Toe(Wheel.FrontRight, setup1.basicSetup.alignment.toe);
            double rearLeftToe1 = carSetup1.TyresSetup.Toe(Wheel.RearLeft, setup1.basicSetup.alignment.toe);
            double rearRightToe1 = carSetup1.TyresSetup.Toe(Wheel.RearRight, setup1.basicSetup.alignment.toe);

            double camberFrontLeft1 = carSetup1.TyresSetup.Camber(Wheel.FrontLeft, setup1.basicSetup.alignment.camber);
            double camberFrontRight1 = carSetup1.TyresSetup.Camber(Wheel.FrontRight, setup1.basicSetup.alignment.camber);
            double camberRearLeft1 = carSetup1.TyresSetup.Camber(Wheel.RearLeft, setup1.basicSetup.alignment.camber);
            double camberRearRight1 = carSetup1.TyresSetup.Camber(Wheel.RearRight, setup1.basicSetup.alignment.camber);


            // Mechnical Setup car 1
            int wheelRateFrontLeft1 = carSetup1.MechanicalSetup.WheelRate(setup1.advancedSetup.mechanicalBalance.wheelRate, Wheel.FrontLeft);
            int wheelRateFrontRight1 = carSetup1.MechanicalSetup.WheelRate(setup1.advancedSetup.mechanicalBalance.wheelRate, Wheel.FrontRight);
            int wheelRateRearLeft1 = carSetup1.MechanicalSetup.WheelRate(setup1.advancedSetup.mechanicalBalance.wheelRate, Wheel.RearLeft);
            int wheelRateRearRight1 = carSetup1.MechanicalSetup.WheelRate(setup1.advancedSetup.mechanicalBalance.wheelRate, Wheel.RearRight);


            int bumpStopRateFrontLeft1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.FrontLeft);
            int bumpStopRateFrontRight1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.FrontRight);
            int bumpStopRateRearLeft1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.RearLeft);
            int bumpStopRateRearRight1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.RearRight);

            int bumpStopRangeFrontLeft1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.FrontLeft);
            int bumpStopRangeFrontRight1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.FrontRight);
            int bumpStopRangeRearLeft1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.RearLeft);
            int bumpStopRangeRearRight1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.RearRight);

            int differentialPreload1 = carSetup1.MechanicalSetup.PreloadDifferential(setup1.advancedSetup.drivetrain.preload);

            int brakePower1 = carSetup1.MechanicalSetup.BrakePower(setup1.advancedSetup.mechanicalBalance.brakeTorque);
            double brakeBias1 = carSetup1.MechanicalSetup.BrakeBias(setup1.advancedSetup.mechanicalBalance.brakeBias);
            int antiRollBarFront1 = carSetup1.MechanicalSetup.AntiRollBarFront(setup1.advancedSetup.mechanicalBalance.aRBFront);
            int antiRollBarRear1 = carSetup1.MechanicalSetup.AntiRollBarFront(setup1.advancedSetup.mechanicalBalance.aRBRear);
            double steeringRatio1 = carSetup1.MechanicalSetup.SteeringRatio(setup1.basicSetup.alignment.steerRatio);


            // Dampers car 1
            int bumpSlowFrontLeft1 = carSetup1.DamperSetup.BumpSlow(setup1.advancedSetup.dampers.bumpSlow, Wheel.FrontLeft);
            int bumpSlowFrontRight1 = carSetup1.DamperSetup.BumpSlow(setup1.advancedSetup.dampers.bumpSlow, Wheel.FrontRight);
            int bumpSlowRearLeft1 = carSetup1.DamperSetup.BumpSlow(setup1.advancedSetup.dampers.bumpSlow, Wheel.RearLeft);
            int bumpSlowRearRight1 = carSetup1.DamperSetup.BumpSlow(setup1.advancedSetup.dampers.bumpSlow, Wheel.RearRight);

            int bumpFastFrontLeft1 = carSetup1.DamperSetup.BumpFast(setup1.advancedSetup.dampers.bumpFast, Wheel.FrontLeft);
            int bumpFastFrontRight1 = carSetup1.DamperSetup.BumpFast(setup1.advancedSetup.dampers.bumpFast, Wheel.FrontRight);
            int bumpFastRearLeft1 = carSetup1.DamperSetup.BumpFast(setup1.advancedSetup.dampers.bumpFast, Wheel.RearLeft);
            int bumpFastRearRight1 = carSetup1.DamperSetup.BumpFast(setup1.advancedSetup.dampers.bumpFast, Wheel.RearRight);

            int reboundSlowFrontLeft1 = carSetup1.DamperSetup.ReboundSlow(setup1.advancedSetup.dampers.reboundSlow, Wheel.FrontLeft);
            int reboundSlowFrontRight1 = carSetup1.DamperSetup.ReboundSlow(setup1.advancedSetup.dampers.reboundSlow, Wheel.FrontRight);
            int reboundSlowRearLeft1 = carSetup1.DamperSetup.ReboundSlow(setup1.advancedSetup.dampers.reboundSlow, Wheel.RearLeft);
            int reboundSlowRearRight1 = carSetup1.DamperSetup.ReboundSlow(setup1.advancedSetup.dampers.reboundSlow, Wheel.RearRight);

            int reboundFastFrontLeft1 = carSetup1.DamperSetup.ReboundFast(setup1.advancedSetup.dampers.reboundFast, Wheel.FrontLeft);
            int reboundFastFrontRight1 = carSetup1.DamperSetup.ReboundFast(setup1.advancedSetup.dampers.reboundFast, Wheel.FrontRight);
            int reboundFastRearLeft1 = carSetup1.DamperSetup.ReboundFast(setup1.advancedSetup.dampers.reboundFast, Wheel.RearLeft);
            int reboundFastRearRight1 = carSetup1.DamperSetup.ReboundFast(setup1.advancedSetup.dampers.reboundFast, Wheel.RearRight);

            // Aero Balance car 1
            int rideHeightFront1 = carSetup1.AeroBalance.RideHeight(setup1.advancedSetup.aeroBalance.rideHeight, Position.Front);
            int rideHeightRear1 = carSetup1.AeroBalance.RideHeight(setup1.advancedSetup.aeroBalance.rideHeight, Position.Rear);
            int rearWing1 = carSetup1.AeroBalance.RearWing(setup1.advancedSetup.aeroBalance.rearWing);
            int splitter1 = carSetup1.AeroBalance.Splitter(setup1.advancedSetup.aeroBalance.splitter);
            int brakeDuctsFront1 = setup1.advancedSetup.aeroBalance.brakeDuct[(int)Position.Front];
            int brakeDuctsRear1 = setup1.advancedSetup.aeroBalance.brakeDuct[(int)Position.Rear];



            ///// Car 2 conversions

            CarModels model2 = ConversionFactory.ParseCarName(DocUtil.GetParseName(setupFile2.FullName));
            if (model2 == CarModels.None) return;
            ICarSetupConversion carSetup2 = ConversionFactory.GetConversion(model2);

            if (carSetup2 == null) return;

            TyreCompound compound2 = carSetup2.TyresSetup.Compound(setup2.basicSetup.tyres.tyreCompound);

            // Allignment / Tyre Setup car 2
            double frontLeftPressure2 = carSetup2.TyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.FrontLeft, setup2.basicSetup.tyres.tyrePressure);
            double frontRightPressure2 = carSetup2.TyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.FrontRight, setup2.basicSetup.tyres.tyrePressure);
            double rearLeftPressure2 = carSetup2.TyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.RearLeft, setup2.basicSetup.tyres.tyrePressure);
            double rearRightPressure2 = carSetup2.TyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.RearRight, setup2.basicSetup.tyres.tyrePressure);

            double frontLeftCaster2 = carSetup2.TyresSetup.Caster(setup2.basicSetup.alignment.casterLF);
            double frontRightCaster2 = carSetup2.TyresSetup.Caster(setup2.basicSetup.alignment.casterRF);

            double frontLeftToe2 = carSetup2.TyresSetup.Toe(Wheel.FrontLeft, setup2.basicSetup.alignment.toe);
            double frontRightToe2 = carSetup2.TyresSetup.Toe(Wheel.FrontRight, setup2.basicSetup.alignment.toe);
            double rearLeftToe2 = carSetup2.TyresSetup.Toe(Wheel.RearLeft, setup2.basicSetup.alignment.toe);
            double rearRightToe2 = carSetup2.TyresSetup.Toe(Wheel.RearRight, setup2.basicSetup.alignment.toe);

            double camberFrontLeft2 = carSetup2.TyresSetup.Camber(Wheel.FrontLeft, setup2.basicSetup.alignment.camber);
            double camberFrontRight2 = carSetup2.TyresSetup.Camber(Wheel.FrontRight, setup2.basicSetup.alignment.camber);
            double camberRearLeft2 = carSetup2.TyresSetup.Camber(Wheel.RearLeft, setup2.basicSetup.alignment.camber);
            double camberRearRight2 = carSetup2.TyresSetup.Camber(Wheel.RearRight, setup2.basicSetup.alignment.camber);


            // Mechnical Setup car 2
            int wheelRateFrontLeft2 = carSetup2.MechanicalSetup.WheelRate(setup2.advancedSetup.mechanicalBalance.wheelRate, Wheel.FrontLeft);
            int wheelRateFrontRight2 = carSetup2.MechanicalSetup.WheelRate(setup2.advancedSetup.mechanicalBalance.wheelRate, Wheel.FrontRight);
            int wheelRateRearLeft2 = carSetup2.MechanicalSetup.WheelRate(setup2.advancedSetup.mechanicalBalance.wheelRate, Wheel.RearLeft);
            int wheelRateRearRight2 = carSetup2.MechanicalSetup.WheelRate(setup2.advancedSetup.mechanicalBalance.wheelRate, Wheel.RearRight);


            int bumpStopRateFrontLeft2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.FrontLeft);
            int bumpStopRateFrontRight2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.FrontRight);
            int bumpStopRateRearLeft2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.RearLeft);
            int bumpStopRateRearRight2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.advancedSetup.mechanicalBalance.bumpStopRateUp, Wheel.RearRight);

            int bumpStopRangeFrontLeft2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.FrontLeft);
            int bumpStopRangeFrontRight2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.FrontRight);
            int bumpStopRangeRearLeft2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.RearLeft);
            int bumpStopRangeRearRight2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.advancedSetup.mechanicalBalance.bumpStopWindow, Wheel.RearRight);

            int differentialPreload2 = carSetup2.MechanicalSetup.PreloadDifferential(setup2.advancedSetup.drivetrain.preload);

            int brakePower2 = carSetup2.MechanicalSetup.BrakePower(setup2.advancedSetup.mechanicalBalance.brakeTorque);
            double brakeBias2 = carSetup2.MechanicalSetup.BrakeBias(setup2.advancedSetup.mechanicalBalance.brakeBias);
            int antiRollBarFront2 = carSetup2.MechanicalSetup.AntiRollBarFront(setup2.advancedSetup.mechanicalBalance.aRBFront);
            int antiRollBarRear2 = carSetup2.MechanicalSetup.AntiRollBarFront(setup2.advancedSetup.mechanicalBalance.aRBRear);
            double steeringRatio2 = carSetup2.MechanicalSetup.SteeringRatio(setup2.basicSetup.alignment.steerRatio);


            // Dampers car 2
            int bumpSlowFrontLeft2 = carSetup2.DamperSetup.BumpSlow(setup2.advancedSetup.dampers.bumpSlow, Wheel.FrontLeft);
            int bumpSlowFrontRight2 = carSetup2.DamperSetup.BumpSlow(setup2.advancedSetup.dampers.bumpSlow, Wheel.FrontRight);
            int bumpSlowRearLeft2 = carSetup2.DamperSetup.BumpSlow(setup2.advancedSetup.dampers.bumpSlow, Wheel.RearLeft);
            int bumpSlowRearRight2 = carSetup2.DamperSetup.BumpSlow(setup2.advancedSetup.dampers.bumpSlow, Wheel.RearRight);

            int bumpFastFrontLeft2 = carSetup2.DamperSetup.BumpFast(setup2.advancedSetup.dampers.bumpFast, Wheel.FrontLeft);
            int bumpFastFrontRight2 = carSetup2.DamperSetup.BumpFast(setup2.advancedSetup.dampers.bumpFast, Wheel.FrontRight);
            int bumpFastRearLeft2 = carSetup2.DamperSetup.BumpFast(setup2.advancedSetup.dampers.bumpFast, Wheel.RearLeft);
            int bumpFastRearRight2 = carSetup2.DamperSetup.BumpFast(setup2.advancedSetup.dampers.bumpFast, Wheel.RearRight);

            int reboundSlowFrontLeft2 = carSetup2.DamperSetup.ReboundSlow(setup2.advancedSetup.dampers.reboundSlow, Wheel.FrontLeft);
            int reboundSlowFrontRight2 = carSetup2.DamperSetup.ReboundSlow(setup2.advancedSetup.dampers.reboundSlow, Wheel.FrontRight);
            int reboundSlowRearLeft2 = carSetup2.DamperSetup.ReboundSlow(setup2.advancedSetup.dampers.reboundSlow, Wheel.RearLeft);
            int reboundSlowRearRight2 = carSetup2.DamperSetup.ReboundSlow(setup2.advancedSetup.dampers.reboundSlow, Wheel.RearRight);

            int reboundFastFrontLeft2 = carSetup2.DamperSetup.ReboundFast(setup2.advancedSetup.dampers.reboundFast, Wheel.FrontLeft);
            int reboundFastFrontRight2 = carSetup2.DamperSetup.ReboundFast(setup2.advancedSetup.dampers.reboundFast, Wheel.FrontRight);
            int reboundFastRearLeft2 = carSetup2.DamperSetup.ReboundFast(setup2.advancedSetup.dampers.reboundFast, Wheel.RearLeft);
            int reboundFastRearRight2 = carSetup2.DamperSetup.ReboundFast(setup2.advancedSetup.dampers.reboundFast, Wheel.RearRight);

            // Aero Balance car 2
            int rideHeightFront2 = carSetup2.AeroBalance.RideHeight(setup2.advancedSetup.aeroBalance.rideHeight, Position.Front);
            int rideHeightRear2 = carSetup2.AeroBalance.RideHeight(setup2.advancedSetup.aeroBalance.rideHeight, Position.Rear);
            int rearWing2 = carSetup2.AeroBalance.RearWing(setup2.advancedSetup.aeroBalance.rearWing);
            int splitter2 = carSetup2.AeroBalance.Splitter(setup2.advancedSetup.aeroBalance.splitter);
            int brakeDuctsFront2 = setup2.advancedSetup.aeroBalance.brakeDuct[(int)Position.Front];
            int brakeDuctsRear2 = setup2.advancedSetup.aeroBalance.brakeDuct[(int)Position.Rear];


            //// Setup Info Section
            Section setupSection = new Section();
            TableRowGroup rgSetupInfo = new TableRowGroup();
            Paragraph header1 = DocUtil.GetDefaultHeader($"{setupFile1.Name.Replace(".json", "")}");
            header1.TextAlignment = TextAlignment.Right;
            Paragraph header2 = DocUtil.GetDefaultHeader($"{setupFile2.Name.Replace(".json", "")}");
            header2.TextAlignment = TextAlignment.Left;
            rgSetupInfo.Rows.Add(DocUtil.GetTableRow(header1, DocUtil.GetDefaultParagraph(), header2));
            rgSetupInfo.Rows.Add(DocUtil.GetTableRowCompare($"{DocUtil.GetTrackName(setupFile1.FullName)}", "Track", $"{DocUtil.GetTrackName(setupFile2.FullName)}"));
            rgSetupInfo.Rows.Add(DocUtil.GetTableRowCompare($"{ConversionFactory.CarModelToCarName[carSetup1.CarModel]}", "Car", $"{ConversionFactory.CarModelToCarName[carSetup2.CarModel]}"));
            rgSetupInfo.Rows.Add(DocUtil.GetTableRowCompare($"{carSetup1.CarClass}", "Class", $"{carSetup2.CarClass}"));
            Table setupInfoTable = DocUtil.GetTable(35, 15, 35);
            setupInfoTable.RowGroups.Add(rgSetupInfo);
            setupSection.Blocks.Add(setupInfoTable);
            setupSection.BorderBrush = Brushes.White;
            setupSection.BorderThickness = new Thickness(0, 1, 0, 1);
            setupSection.Margin = new Thickness(0, 0, 0, 0);
            flowDocument.Blocks.Add(setupSection);


            //// Tyres Section
            Section tyresSection = new Section();
            TableRowGroup rgTyres = new TableRowGroup();
            rgTyres.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), DocUtil.GetDefaultHeader("Tyres Setup"), DocUtil.GetDefaultParagraph()));
            rgTyres.Rows.Add(DocUtil.GetTableRowCompare($"{compound1}", "Compound", $"{compound2}"));
            rgTyres.Rows.Add(DocUtil.GetTableRowCompare($"FL: {frontLeftPressure1}, FR: {frontRightPressure1}, RL: {rearLeftPressure1}, RR: {rearRightPressure1}", "Pressures(psi)", $"FL: {frontLeftPressure2}, FR: {frontRightPressure2}, RL: {rearLeftPressure2}, RR: {rearRightPressure2}"));
            rgTyres.Rows.Add(DocUtil.GetTableRowCompare($"FL: {frontLeftToe1}, FR: {frontRightToe1}, RL: {rearLeftToe1}, RR: {rearRightToe1}", "Toe(°)", $"FL: {frontLeftToe2}, FR: {frontRightToe2}, RL: {rearLeftToe2}, RR: {rearRightToe2}"));
            rgTyres.Rows.Add(DocUtil.GetTableRowCompare($"FL: {camberFrontLeft1}, FR: {camberFrontRight1}, RL: {camberRearLeft1}, RR: {camberRearRight1}", "Camber(°)", $"FL: {camberFrontLeft2}, FR: {camberFrontRight2}, RL: {camberRearLeft2}, RR: {camberRearRight2}"));
            rgTyres.Rows.Add(DocUtil.GetTableRowCompare($"FL: {frontLeftCaster1}, FR: {frontRightCaster1}", "Caster(°)", $"FL: {frontLeftCaster2}, FR: {frontRightCaster2}"));
            Table tyresSetupTable = DocUtil.GetTable(35, 15, 35);
            tyresSetupTable.RowGroups.Add(rgTyres);
            tyresSection.Blocks.Add(tyresSetupTable);
            tyresSection.BorderBrush = Brushes.White;
            tyresSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(tyresSection);

            //// Mechanical Grip Section
            Section gripSection = new Section();
            TableRowGroup rgGrip = new TableRowGroup();
            rgGrip.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), DocUtil.GetDefaultHeader("Mechanical Grip"), DocUtil.GetDefaultParagraph()));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"FL: {wheelRateFrontLeft1}, FR: {wheelRateFrontRight1}, RL: {wheelRateRearLeft1}, RR: {wheelRateRearRight1}", "Wheelrates(Nm)", $"FL: {wheelRateFrontLeft2}, FR: {wheelRateFrontRight2}, RL: {wheelRateRearLeft2}, RR: {wheelRateRearRight2}"));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"FL: {bumpStopRateFrontLeft1}, FR: {bumpStopRateFrontRight1}, RL: {bumpStopRateRearLeft1}, RR: {bumpStopRateRearRight1}", "Bumpstop Rate(Nm)", $"FL: {bumpStopRateFrontLeft2}, FR: {bumpStopRateFrontRight2}, RL: {bumpStopRateRearLeft2}, RR: {bumpStopRateRearRight2}"));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"FL: {bumpStopRangeFrontLeft1}, FR: {bumpStopRangeFrontRight1}, RL: {bumpStopRangeRearLeft1}, RR: {bumpStopRangeRearRight1}", "Bumstop Range", $"FL: {bumpStopRangeFrontLeft2}, FR: {bumpStopRangeFrontRight2}, RL: {bumpStopRangeRearLeft2}, RR: {bumpStopRangeRearRight2}"));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"Front: {antiRollBarFront1}, Rear: {antiRollBarRear1}", "Anti roll bar", $"Front: {antiRollBarFront2}, Rear: {antiRollBarRear2}"));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"{differentialPreload1}", "Diff Preload(Nm)", $"{differentialPreload2}"));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"{brakePower1}%", "Brake Power", $"{brakePower2}%"));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"{brakeBias1}%", "Brake Bias", $"{brakeBias2}%"));
            rgGrip.Rows.Add(DocUtil.GetTableRowCompare($"{steeringRatio1}", "Steering Ratio", $"{steeringRatio2}"));
            Table gripTable = DocUtil.GetTable(35, 15, 35);
            gripTable.RowGroups.Add(rgGrip);
            gripSection.Blocks.Add(gripTable);
            gripSection.BorderBrush = Brushes.White;
            gripSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(gripSection);


            //// Dampers Section
            Section dampersSection = new Section();
            TableRowGroup rgDampers = new TableRowGroup();
            rgDampers.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), DocUtil.GetDefaultHeader("Dampers"), DocUtil.GetDefaultParagraph()));
            rgDampers.Rows.Add(DocUtil.GetTableRowCompare($"FL: {bumpSlowFrontLeft1}, FR: {bumpSlowFrontRight1}, RL: {bumpSlowRearLeft1}, RR: {bumpSlowRearRight1}", "Bump Slow", $"FL: {bumpSlowFrontLeft2}, FR: {bumpSlowFrontRight2}, RL: {bumpSlowRearLeft2}, RR: {bumpSlowRearRight2}"));
            rgDampers.Rows.Add(DocUtil.GetTableRowCompare($"FL: {bumpFastFrontLeft1}, FR: {bumpFastFrontRight1}, RL: {bumpFastRearLeft1}, RR: {bumpFastRearRight1}", "Bump Fast", $"FL: {bumpFastFrontLeft2}, FR: {bumpFastFrontRight2}, RL: {bumpFastRearLeft2}, RR: {bumpFastRearRight2}"));
            rgDampers.Rows.Add(DocUtil.GetTableRowCompare($"FL: {reboundSlowFrontLeft1}, FR: {reboundSlowFrontRight1}, RL: {reboundSlowRearLeft1}, RR: {reboundSlowRearRight1}", "Rebound Slow", $"FL: {reboundSlowFrontLeft2}, FR: {reboundSlowFrontRight2}, RL: {reboundSlowRearLeft2}, RR: {reboundSlowRearRight2}"));
            rgDampers.Rows.Add(DocUtil.GetTableRowCompare($"FL: {reboundFastFrontLeft1}, FR: {reboundFastFrontRight1}, RL: {reboundFastRearLeft1}, RR: {reboundFastRearRight1}", "Rebound Fast", $"FL: {reboundFastFrontLeft2}, FR: {reboundFastFrontRight2}, RL: {reboundFastRearLeft2}, RR: {reboundFastRearRight2}"));
            Table dampersTable = DocUtil.GetTable(35, 15, 35);
            dampersTable.RowGroups.Add(rgDampers);
            dampersSection.Blocks.Add(dampersTable);
            dampersSection.BorderBrush = Brushes.White;
            dampersSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(dampersSection);


            //// Aero
            Section aeroBalanceSection = new Section();
            TableRowGroup rgAero = new TableRowGroup();
            rgAero.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), DocUtil.GetDefaultHeader("Aero Balance"), DocUtil.GetDefaultParagraph()));
            rgAero.Rows.Add(DocUtil.GetTableRowCompare($"Front: {rideHeightFront1}, Rear: {rideHeightRear1}", "Ride height(mm)", $"Front: {rideHeightFront2}, Rear: {rideHeightRear2}"));
            rgAero.Rows.Add(DocUtil.GetTableRowCompare($"{splitter1}", "Splitter", $"{splitter2}"));
            rgAero.Rows.Add(DocUtil.GetTableRowCompare($"{rearWing1}", "Rear Wing", $"{rearWing2}"));
            rgAero.Rows.Add(DocUtil.GetTableRowCompare($"Front: {brakeDuctsFront1}, Rear: {brakeDuctsRear1}", "Brake ducts", $"Front: {brakeDuctsFront2}, Rear: {brakeDuctsRear2}"));
            Table aeroTable = DocUtil.GetTable(35, 15, 35);
            aeroTable.RowGroups.Add(rgAero);
            aeroBalanceSection.Blocks.Add(aeroTable);
            aeroBalanceSection.BorderBrush = Brushes.White;
            aeroBalanceSection.BorderThickness = new Thickness(0, 1, 0, 0);
            flowDocument.Blocks.Add(aeroBalanceSection);

            //// Electronics
            Section electronicsSection = new Section();
            TableRowGroup rgElectro = new TableRowGroup();
            rgElectro.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), DocUtil.GetDefaultHeader("Electronics"), DocUtil.GetDefaultParagraph()));
            rgElectro.Rows.Add(DocUtil.GetTableRowCompare($"{setup1.basicSetup.electronics.tC1}", "TC 1", $"{setup2.basicSetup.electronics.tC1}"));
            rgElectro.Rows.Add(DocUtil.GetTableRowCompare($"{setup1.basicSetup.electronics.tC2}", "TC 2", $"{setup2.basicSetup.electronics.tC2}"));
            rgElectro.Rows.Add(DocUtil.GetTableRowCompare($"{setup1.basicSetup.electronics.abs}", "ABS", $"{setup2.basicSetup.electronics.abs}"));
            rgElectro.Rows.Add(DocUtil.GetTableRowCompare($"{setup1.basicSetup.electronics.eCUMap + 1}", "Engine map", $"{setup2.basicSetup.electronics.eCUMap + 1}"));
            Table electroTable = DocUtil.GetTable(35, 15, 35);
            electroTable.RowGroups.Add(rgElectro);
            electronicsSection.Blocks.Add(electroTable);
            electronicsSection.BorderBrush = Brushes.White;
            electronicsSection.BorderThickness = new Thickness(0, 1, 0, 1);
            flowDocument.Blocks.Add(electronicsSection);
        }

    }
}
