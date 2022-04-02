using ACCSetupApp.Controls.Setup.FlowDocUtil;
using ACCSetupApp.SetupParser;
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
using static ACCSetupApp.SetupParser.SetupConverter;
using static SetupParser.SetupJson;

namespace ACCSetupApp.Controls.Setup
{
    public class FlowDocSetupRenderer
    {
        public void LogSetup(ref FlowDocument flowDocument, string file)
        {
            flowDocument.Blocks.Clear();

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

            ICarSetupConversion carSetup = new ConversionFactory().GetConversion(DocUtil.GetParseName(file));

            if (carSetup == null)
                return;

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


            // Dampers
            int bumpSlowFrontLeft = setup.advancedSetup.dampers.bumpSlow[(int)Wheel.FrontLeft];
            int bumpSlowFrontRight = setup.advancedSetup.dampers.bumpSlow[(int)Wheel.FrontRight];
            int bumpSlowRearLeft = setup.advancedSetup.dampers.bumpSlow[(int)Wheel.RearLeft];
            int bumpSlowRearRight = setup.advancedSetup.dampers.bumpSlow[(int)Wheel.RearRight];

            int bumpFastFrontLeft = setup.advancedSetup.dampers.bumpFast[(int)Wheel.FrontLeft];
            int bumpFastFrontRight = setup.advancedSetup.dampers.bumpFast[(int)Wheel.FrontRight];
            int bumpFastRearLeft = setup.advancedSetup.dampers.bumpFast[(int)Wheel.RearLeft];
            int bumpFastRearRight = setup.advancedSetup.dampers.bumpFast[(int)Wheel.RearRight];

            int reboundSlowFrontLeft = setup.advancedSetup.dampers.reboundSlow[(int)Wheel.FrontLeft];
            int reboundSlowFrontRight = setup.advancedSetup.dampers.reboundSlow[(int)Wheel.FrontRight];
            int reboundSlowRearLeft = setup.advancedSetup.dampers.reboundSlow[(int)Wheel.RearLeft];
            int reboundSlowRearRight = setup.advancedSetup.dampers.reboundSlow[(int)Wheel.RearRight];

            int reboundFastFrontLeft = setup.advancedSetup.dampers.reboundFast[(int)Wheel.FrontLeft];
            int reboundFastFrontRight = setup.advancedSetup.dampers.reboundFast[(int)Wheel.FrontRight];
            int reboundFastRearLeft = setup.advancedSetup.dampers.reboundFast[(int)Wheel.RearLeft];
            int reboundFastRearRight = setup.advancedSetup.dampers.reboundFast[(int)Wheel.RearRight];



            // Aero Balance
            int rideHeightFront = carSetup.AeroBalance.RideHeight(setup.advancedSetup.aeroBalance.rideHeight, Position.Front);
            int rideHeightRear = carSetup.AeroBalance.RideHeight(setup.advancedSetup.aeroBalance.rideHeight, Position.Rear);
            int rearWing = carSetup.AeroBalance.RearWing(setup.advancedSetup.aeroBalance.rearWing);
            int splitter = carSetup.AeroBalance.Splitter(setup.advancedSetup.aeroBalance.splitter);
            int brakeDuctsFront = setup.advancedSetup.aeroBalance.brakeDuct[(int)Position.Front];
            int brakeDuctsRear = setup.advancedSetup.aeroBalance.brakeDuct[(int)Position.Rear];



            Section setupTitle = new Section();
            setupTitle.Blocks.Add(DocUtil.GetDefaultHeader(20, $"{jsonFile.Name.Replace(".json", "")}"));

            setupTitle.BorderBrush = Brushes.White;
            setupTitle.BorderThickness = new Thickness(1, 1, 1, 1);
            setupTitle.Margin = new Thickness(0, 0, 0, 0);

            flowDocument.Blocks.Add(setupTitle);

            // Setup Info
            Section setupSection = new Section();
            //setupSection.Blocks.Add(GetDefaultHeader("Setup Info"));

            Table setupInfoTable = DocUtil.GetTable(30, 70);
            TableRowGroup rowGroupSetupInfo = new TableRowGroup();
            rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRow("Track", $"{DocUtil.GetTrackName(jsonFile.FullName)}"));
            rowGroupSetupInfo.Rows.Add(DocUtil.GetTableRow("Car", $"{carSetup.CarName}"));
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
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Brake ducts", $"Front: {brakeDuctsFront}, Rear: {brakeDuctsRear}"));
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Ride height(mm)", $"Front: {rideHeightFront}, Rear: {rideHeightRear}"));
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Splitter", $"{splitter}"));
            aeroTableRowGroup.Rows.Add(DocUtil.GetTableRow("Rear Wing", $"{rearWing}"));


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
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("TC 1", $"{setup.basicSetup.electronics.tC1}"));
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("TC 2", $"{setup.basicSetup.electronics.tC2}"));
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("ABS", $"{setup.basicSetup.electronics.abs}"));
            electronicsRowGroup.Rows.Add(DocUtil.GetTableRow("Engine map", $"{setup.basicSetup.electronics.eCUMap + 1}"));

            electronicsTable.RowGroups.Add(electronicsRowGroup);
            electronicsSection.Blocks.Add(electronicsTable);
            electronicsSection.BorderBrush = Brushes.White;
            electronicsSection.BorderThickness = new Thickness(1, 1, 1, 1);
            flowDocument.Blocks.Add(electronicsSection);
        }


    }
}
