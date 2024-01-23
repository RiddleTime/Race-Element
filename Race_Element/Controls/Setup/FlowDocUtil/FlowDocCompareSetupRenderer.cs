using RaceElement.Controls.Setup.FlowDocUtil;
using RaceElement.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using static RaceElement.Data.ConversionFactory;
using static RaceElement.Data.SetupConverter;

namespace RaceElement.Controls.Setup;

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
        TyreCompound compound1 = AbstractTyresSetup.Compound(setup1.BasicSetup.Tyres.TyreCompound);

        // Allignment / Tyre Setup car 1
        double frontLeftPressure1 = AbstractTyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.FrontLeft, setup1.BasicSetup.Tyres.TyrePressure);
        double frontRightPressure1 = AbstractTyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.FrontRight, setup1.BasicSetup.Tyres.TyrePressure);
        double rearLeftPressure1 = AbstractTyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.RearLeft, setup1.BasicSetup.Tyres.TyrePressure);
        double rearRightPressure1 = AbstractTyresSetup.TirePressure(carSetup1.DryTyreCompound, Wheel.RearRight, setup1.BasicSetup.Tyres.TyrePressure);

        double frontLeftCaster1 = carSetup1.TyresSetup.Caster(setup1.BasicSetup.Alignment.CasterLF);
        double frontRightCaster1 = carSetup1.TyresSetup.Caster(setup1.BasicSetup.Alignment.CasterRF);

        double frontLeftToe1 = carSetup1.TyresSetup.Toe(Wheel.FrontLeft, setup1.BasicSetup.Alignment.Toe);
        double frontRightToe1 = carSetup1.TyresSetup.Toe(Wheel.FrontRight, setup1.BasicSetup.Alignment.Toe);
        double rearLeftToe1 = carSetup1.TyresSetup.Toe(Wheel.RearLeft, setup1.BasicSetup.Alignment.Toe);
        double rearRightToe1 = carSetup1.TyresSetup.Toe(Wheel.RearRight, setup1.BasicSetup.Alignment.Toe);

        double camberFrontLeft1 = carSetup1.TyresSetup.Camber(Wheel.FrontLeft, setup1.BasicSetup.Alignment.Camber);
        double camberFrontRight1 = carSetup1.TyresSetup.Camber(Wheel.FrontRight, setup1.BasicSetup.Alignment.Camber);
        double camberRearLeft1 = carSetup1.TyresSetup.Camber(Wheel.RearLeft, setup1.BasicSetup.Alignment.Camber);
        double camberRearRight1 = carSetup1.TyresSetup.Camber(Wheel.RearRight, setup1.BasicSetup.Alignment.Camber);


        // Mechnical Setup car 1
        int wheelRateFrontLeft1 = carSetup1.MechanicalSetup.WheelRate(setup1.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.FrontLeft);
        int wheelRateFrontRight1 = carSetup1.MechanicalSetup.WheelRate(setup1.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.FrontRight);
        int wheelRateRearLeft1 = carSetup1.MechanicalSetup.WheelRate(setup1.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.RearLeft);
        int wheelRateRearRight1 = carSetup1.MechanicalSetup.WheelRate(setup1.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.RearRight);


        int bumpStopRateFrontLeft1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.FrontLeft);
        int bumpStopRateFrontRight1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.FrontRight);
        int bumpStopRateRearLeft1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.RearLeft);
        int bumpStopRateRearRight1 = carSetup1.MechanicalSetup.BumpstopRate(setup1.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.RearRight);

        int bumpStopRangeFrontLeft1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.FrontLeft);
        int bumpStopRangeFrontRight1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.FrontRight);
        int bumpStopRangeRearLeft1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.RearLeft);
        int bumpStopRangeRearRight1 = carSetup1.MechanicalSetup.BumpstopRange(setup1.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.RearRight);

        int differentialPreload1 = carSetup1.MechanicalSetup.PreloadDifferential(setup1.AdvancedSetup.Drivetrain.Preload);

        int brakePower1 = carSetup1.MechanicalSetup.BrakePower(setup1.AdvancedSetup.MechanicalBalance.BrakeTorque);
        double brakeBias1 = carSetup1.MechanicalSetup.BrakeBias(setup1.AdvancedSetup.MechanicalBalance.BrakeBias);
        int antiRollBarFront1 = carSetup1.MechanicalSetup.AntiRollBarFront(setup1.AdvancedSetup.MechanicalBalance.ARBFront);
        int antiRollBarRear1 = carSetup1.MechanicalSetup.AntiRollBarFront(setup1.AdvancedSetup.MechanicalBalance.ARBRear);
        double steeringRatio1 = carSetup1.MechanicalSetup.SteeringRatio(setup1.BasicSetup.Alignment.SteerRatio);


        // Dampers car 1
        int bumpSlowFrontLeft1 = carSetup1.DamperSetup.BumpSlow(setup1.AdvancedSetup.Dampers.BumpSlow, Wheel.FrontLeft);
        int bumpSlowFrontRight1 = carSetup1.DamperSetup.BumpSlow(setup1.AdvancedSetup.Dampers.BumpSlow, Wheel.FrontRight);
        int bumpSlowRearLeft1 = carSetup1.DamperSetup.BumpSlow(setup1.AdvancedSetup.Dampers.BumpSlow, Wheel.RearLeft);
        int bumpSlowRearRight1 = carSetup1.DamperSetup.BumpSlow(setup1.AdvancedSetup.Dampers.BumpSlow, Wheel.RearRight);

        int bumpFastFrontLeft1 = carSetup1.DamperSetup.BumpFast(setup1.AdvancedSetup.Dampers.BumpFast, Wheel.FrontLeft);
        int bumpFastFrontRight1 = carSetup1.DamperSetup.BumpFast(setup1.AdvancedSetup.Dampers.BumpFast, Wheel.FrontRight);
        int bumpFastRearLeft1 = carSetup1.DamperSetup.BumpFast(setup1.AdvancedSetup.Dampers.BumpFast, Wheel.RearLeft);
        int bumpFastRearRight1 = carSetup1.DamperSetup.BumpFast(setup1.AdvancedSetup.Dampers.BumpFast, Wheel.RearRight);

        int reboundSlowFrontLeft1 = carSetup1.DamperSetup.ReboundSlow(setup1.AdvancedSetup.Dampers.ReboundSlow, Wheel.FrontLeft);
        int reboundSlowFrontRight1 = carSetup1.DamperSetup.ReboundSlow(setup1.AdvancedSetup.Dampers.ReboundSlow, Wheel.FrontRight);
        int reboundSlowRearLeft1 = carSetup1.DamperSetup.ReboundSlow(setup1.AdvancedSetup.Dampers.ReboundSlow, Wheel.RearLeft);
        int reboundSlowRearRight1 = carSetup1.DamperSetup.ReboundSlow(setup1.AdvancedSetup.Dampers.ReboundSlow, Wheel.RearRight);

        int reboundFastFrontLeft1 = carSetup1.DamperSetup.ReboundFast(setup1.AdvancedSetup.Dampers.ReboundFast, Wheel.FrontLeft);
        int reboundFastFrontRight1 = carSetup1.DamperSetup.ReboundFast(setup1.AdvancedSetup.Dampers.ReboundFast, Wheel.FrontRight);
        int reboundFastRearLeft1 = carSetup1.DamperSetup.ReboundFast(setup1.AdvancedSetup.Dampers.ReboundFast, Wheel.RearLeft);
        int reboundFastRearRight1 = carSetup1.DamperSetup.ReboundFast(setup1.AdvancedSetup.Dampers.ReboundFast, Wheel.RearRight);

        // Aero Balance car 1
        int rideHeightFront1 = carSetup1.AeroBalance.RideHeight(setup1.AdvancedSetup.AeroBalance.RideHeight, Position.Front);
        int rideHeightRear1 = carSetup1.AeroBalance.RideHeight(setup1.AdvancedSetup.AeroBalance.RideHeight, Position.Rear);
        int rearWing1 = carSetup1.AeroBalance.RearWing(setup1.AdvancedSetup.AeroBalance.RearWing);
        int splitter1 = carSetup1.AeroBalance.Splitter(setup1.AdvancedSetup.AeroBalance.Splitter);
        int brakeDuctsFront1 = setup1.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Front];
        int brakeDuctsRear1 = setup1.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Rear];

        // Strategy car 1   
        int brakeCompoundFront1 = setup1.BasicSetup.Strategy.FrontBrakePadCompound + 1;
        int brakeCompoundRear1 = setup1.BasicSetup.Strategy.FrontBrakePadCompound + 1;



        ///// Car 2 conversions

        CarModels model2 = ConversionFactory.ParseCarName(DocUtil.GetParseName(setupFile2.FullName));
        if (model2 == CarModels.None) return;
        ICarSetupConversion carSetup2 = ConversionFactory.GetConversion(model2);

        if (carSetup2 == null) return;

        TyreCompound compound2 = AbstractTyresSetup.Compound(setup2.BasicSetup.Tyres.TyreCompound);

        // Allignment / Tyre Setup car 2
        double frontLeftPressure2 = AbstractTyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.FrontLeft, setup2.BasicSetup.Tyres.TyrePressure);
        double frontRightPressure2 = AbstractTyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.FrontRight, setup2.BasicSetup.Tyres.TyrePressure);
        double rearLeftPressure2 = AbstractTyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.RearLeft, setup2.BasicSetup.Tyres.TyrePressure);
        double rearRightPressure2 = AbstractTyresSetup.TirePressure(carSetup2.DryTyreCompound, Wheel.RearRight, setup2.BasicSetup.Tyres.TyrePressure);

        double frontLeftCaster2 = carSetup2.TyresSetup.Caster(setup2.BasicSetup.Alignment.CasterLF);
        double frontRightCaster2 = carSetup2.TyresSetup.Caster(setup2.BasicSetup.Alignment.CasterRF);

        double frontLeftToe2 = carSetup2.TyresSetup.Toe(Wheel.FrontLeft, setup2.BasicSetup.Alignment.Toe);
        double frontRightToe2 = carSetup2.TyresSetup.Toe(Wheel.FrontRight, setup2.BasicSetup.Alignment.Toe);
        double rearLeftToe2 = carSetup2.TyresSetup.Toe(Wheel.RearLeft, setup2.BasicSetup.Alignment.Toe);
        double rearRightToe2 = carSetup2.TyresSetup.Toe(Wheel.RearRight, setup2.BasicSetup.Alignment.Toe);

        double camberFrontLeft2 = carSetup2.TyresSetup.Camber(Wheel.FrontLeft, setup2.BasicSetup.Alignment.Camber);
        double camberFrontRight2 = carSetup2.TyresSetup.Camber(Wheel.FrontRight, setup2.BasicSetup.Alignment.Camber);
        double camberRearLeft2 = carSetup2.TyresSetup.Camber(Wheel.RearLeft, setup2.BasicSetup.Alignment.Camber);
        double camberRearRight2 = carSetup2.TyresSetup.Camber(Wheel.RearRight, setup2.BasicSetup.Alignment.Camber);


        // Mechnical Setup car 2
        int wheelRateFrontLeft2 = carSetup2.MechanicalSetup.WheelRate(setup2.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.FrontLeft);
        int wheelRateFrontRight2 = carSetup2.MechanicalSetup.WheelRate(setup2.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.FrontRight);
        int wheelRateRearLeft2 = carSetup2.MechanicalSetup.WheelRate(setup2.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.RearLeft);
        int wheelRateRearRight2 = carSetup2.MechanicalSetup.WheelRate(setup2.AdvancedSetup.MechanicalBalance.WheelRate, Wheel.RearRight);


        int bumpStopRateFrontLeft2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.FrontLeft);
        int bumpStopRateFrontRight2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.FrontRight);
        int bumpStopRateRearLeft2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.RearLeft);
        int bumpStopRateRearRight2 = carSetup2.MechanicalSetup.BumpstopRate(setup2.AdvancedSetup.MechanicalBalance.BumpStopRateUp, Wheel.RearRight);

        int bumpStopRangeFrontLeft2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.FrontLeft);
        int bumpStopRangeFrontRight2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.FrontRight);
        int bumpStopRangeRearLeft2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.RearLeft);
        int bumpStopRangeRearRight2 = carSetup2.MechanicalSetup.BumpstopRange(setup2.AdvancedSetup.MechanicalBalance.BumpStopWindow, Wheel.RearRight);

        int differentialPreload2 = carSetup2.MechanicalSetup.PreloadDifferential(setup2.AdvancedSetup.Drivetrain.Preload);

        int brakePower2 = carSetup2.MechanicalSetup.BrakePower(setup2.AdvancedSetup.MechanicalBalance.BrakeTorque);
        double brakeBias2 = carSetup2.MechanicalSetup.BrakeBias(setup2.AdvancedSetup.MechanicalBalance.BrakeBias);
        int antiRollBarFront2 = carSetup2.MechanicalSetup.AntiRollBarFront(setup2.AdvancedSetup.MechanicalBalance.ARBFront);
        int antiRollBarRear2 = carSetup2.MechanicalSetup.AntiRollBarFront(setup2.AdvancedSetup.MechanicalBalance.ARBRear);
        double steeringRatio2 = carSetup2.MechanicalSetup.SteeringRatio(setup2.BasicSetup.Alignment.SteerRatio);


        // Dampers car 2
        int bumpSlowFrontLeft2 = carSetup2.DamperSetup.BumpSlow(setup2.AdvancedSetup.Dampers.BumpSlow, Wheel.FrontLeft);
        int bumpSlowFrontRight2 = carSetup2.DamperSetup.BumpSlow(setup2.AdvancedSetup.Dampers.BumpSlow, Wheel.FrontRight);
        int bumpSlowRearLeft2 = carSetup2.DamperSetup.BumpSlow(setup2.AdvancedSetup.Dampers.BumpSlow, Wheel.RearLeft);
        int bumpSlowRearRight2 = carSetup2.DamperSetup.BumpSlow(setup2.AdvancedSetup.Dampers.BumpSlow, Wheel.RearRight);

        int bumpFastFrontLeft2 = carSetup2.DamperSetup.BumpFast(setup2.AdvancedSetup.Dampers.BumpFast, Wheel.FrontLeft);
        int bumpFastFrontRight2 = carSetup2.DamperSetup.BumpFast(setup2.AdvancedSetup.Dampers.BumpFast, Wheel.FrontRight);
        int bumpFastRearLeft2 = carSetup2.DamperSetup.BumpFast(setup2.AdvancedSetup.Dampers.BumpFast, Wheel.RearLeft);
        int bumpFastRearRight2 = carSetup2.DamperSetup.BumpFast(setup2.AdvancedSetup.Dampers.BumpFast, Wheel.RearRight);

        int reboundSlowFrontLeft2 = carSetup2.DamperSetup.ReboundSlow(setup2.AdvancedSetup.Dampers.ReboundSlow, Wheel.FrontLeft);
        int reboundSlowFrontRight2 = carSetup2.DamperSetup.ReboundSlow(setup2.AdvancedSetup.Dampers.ReboundSlow, Wheel.FrontRight);
        int reboundSlowRearLeft2 = carSetup2.DamperSetup.ReboundSlow(setup2.AdvancedSetup.Dampers.ReboundSlow, Wheel.RearLeft);
        int reboundSlowRearRight2 = carSetup2.DamperSetup.ReboundSlow(setup2.AdvancedSetup.Dampers.ReboundSlow, Wheel.RearRight);

        int reboundFastFrontLeft2 = carSetup2.DamperSetup.ReboundFast(setup2.AdvancedSetup.Dampers.ReboundFast, Wheel.FrontLeft);
        int reboundFastFrontRight2 = carSetup2.DamperSetup.ReboundFast(setup2.AdvancedSetup.Dampers.ReboundFast, Wheel.FrontRight);
        int reboundFastRearLeft2 = carSetup2.DamperSetup.ReboundFast(setup2.AdvancedSetup.Dampers.ReboundFast, Wheel.RearLeft);
        int reboundFastRearRight2 = carSetup2.DamperSetup.ReboundFast(setup2.AdvancedSetup.Dampers.ReboundFast, Wheel.RearRight);

        // Aero Balance car 2
        int rideHeightFront2 = carSetup2.AeroBalance.RideHeight(setup2.AdvancedSetup.AeroBalance.RideHeight, Position.Front);
        int rideHeightRear2 = carSetup2.AeroBalance.RideHeight(setup2.AdvancedSetup.AeroBalance.RideHeight, Position.Rear);
        int rearWing2 = carSetup2.AeroBalance.RearWing(setup2.AdvancedSetup.AeroBalance.RearWing);
        int splitter2 = carSetup2.AeroBalance.Splitter(setup2.AdvancedSetup.AeroBalance.Splitter);
        int brakeDuctsFront2 = setup2.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Front];
        int brakeDuctsRear2 = setup2.AdvancedSetup.AeroBalance.BrakeDuct[(int)Position.Rear];

        // Strategy car 2   
        int brakeCompoundFront2 = setup2.BasicSetup.Strategy.FrontBrakePadCompound + 1;
        int brakeCompoundRear2 = setup2.BasicSetup.Strategy.FrontBrakePadCompound + 1;



        const int cells = 13;
        const int headerWidthPercent = 15;
        string[] tyreLocationLabels = ["FL: ", "FR: ", "RL: ", "RR: "];
        string[] frontOrRearLabels = ["Front: ", "Rear: "];


        //// Setup Info Section
        Section setupSection = new();
        TableRowGroup rgSetupInfo = new();
        Paragraph header1 = DocUtil.GetDefaultHeader($"{setupFile1.Name.Replace(".json", "")}");
        header1.TextAlignment = TextAlignment.Right;
        Paragraph header2 = DocUtil.GetDefaultHeader($"{setupFile2.Name.Replace(".json", "")}");
        header2.TextAlignment = TextAlignment.Left;
        TableRow headerRow = DocUtil.GetTableRow(header1, DocUtil.GetDefaultParagraph(), header2, cells);
        rgSetupInfo.Rows.Add(headerRow);

        rgSetupInfo.Rows.Add(DocUtil.GetTableRowCompare($"{DocUtil.GetTrackName(setupFile1.FullName)}", "Track", $"{DocUtil.GetTrackName(setupFile2.FullName)}", cells));
        rgSetupInfo.Rows.Add(DocUtil.GetTableRowCompare($"{ConversionFactory.CarModelToCarName[carSetup1.CarModel]}", "Car", $"{ConversionFactory.CarModelToCarName[carSetup2.CarModel]}", cells));
        rgSetupInfo.Rows.Add(DocUtil.GetTableRowCompare($"{carSetup1.CarClass}", "Class", $"{carSetup2.CarClass}", cells));
        Table setupInfoTable = DocUtil.GetCenteredTable(headerWidthPercent, cells - 1);
        setupInfoTable.RowGroups.Add(rgSetupInfo);
        setupSection.Blocks.Add(setupInfoTable);
        setupSection.BorderBrush = Brushes.White;
        setupSection.BorderThickness = new Thickness(0, 1, 0, 0);
        setupSection.Margin = new Thickness(0, 0, 0, 0);
        flowDocument.Blocks.Add(setupSection);


        //// Tyres Section
        Section tyresSection = new();
        TableRowGroup rgTyres = new();
        Paragraph tyresHeader = DocUtil.GetDefaultHeader("Tyres Setup");
        tyresHeader.FontStyle = FontStyles.Italic;
        rgTyres.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), tyresHeader, DocUtil.GetDefaultParagraph(), cells));
        rgTyres.Rows.Add(DocUtil.GetTableRowCompare($"{compound1}", "Compound", $"{compound2}", cells));
        rgTyres.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [frontLeftPressure1, frontRightPressure1, rearLeftPressure1, rearRightPressure1], "Pressures(psi)", [frontLeftPressure2, frontRightPressure2, rearLeftPressure2, rearRightPressure2], cells, 1));
        rgTyres.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [frontLeftToe1, frontRightToe1, rearLeftToe1, rearRightToe1], "Toe(°)", [frontLeftToe2, frontRightToe2, rearLeftToe2, rearRightToe2], cells, 2));
        rgTyres.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [camberFrontLeft1, camberFrontRight1, camberRearLeft1, camberRearRight1], "Camber(°)", [camberFrontLeft2, camberFrontRight2, camberRearLeft2, camberRearRight2], cells, 1));
        rgTyres.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels.Take(2).ToArray(), [frontLeftCaster1, frontRightCaster1], "Caster(°)", [frontLeftCaster2, frontRightCaster2], cells, 1));
        Table tyresSetupTable = DocUtil.GetCenteredTable(headerWidthPercent, cells - 1);
        tyresSetupTable.RowGroups.Add(rgTyres);
        tyresSection.Blocks.Add(tyresSetupTable);
        tyresSection.BorderBrush = Brushes.White;
        tyresSection.BorderThickness = new Thickness(0, 1, 0, 0);
        flowDocument.Blocks.Add(tyresSection);

        //// Mechanical Grip Section
        Section gripSection = new();
        TableRowGroup rgGrip = new();
        Paragraph mechanicalHeader = DocUtil.GetDefaultHeader("Mechanical Grip");
        mechanicalHeader.FontStyle = FontStyles.Italic;
        rgGrip.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), mechanicalHeader, DocUtil.GetDefaultParagraph(), cells));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [wheelRateFrontLeft1, wheelRateFrontRight1, wheelRateRearLeft1, wheelRateRearRight1], "Wheelrates(Nm)", [wheelRateFrontLeft2, wheelRateFrontRight2, wheelRateRearLeft2, wheelRateRearRight2], cells, 0));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [bumpStopRateFrontLeft1, bumpStopRateFrontRight1, bumpStopRateRearLeft1, bumpStopRateRearRight1], "Bumpstop Rate(Nm)", [bumpStopRateFrontLeft2, bumpStopRateFrontRight2, bumpStopRateRearLeft2, bumpStopRateRearRight2], cells, 0));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [bumpStopRangeFrontLeft1, bumpStopRangeFrontRight1, bumpStopRangeRearLeft1, bumpStopRangeRearRight1], "Bumpstop Range", [bumpStopRangeFrontLeft2, bumpStopRangeFrontRight2, bumpStopRangeRearLeft2, bumpStopRangeRearRight2], cells, 0));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompareWithLabels(frontOrRearLabels, [antiRollBarFront1, antiRollBarRear1], "Anti roll bar", [antiRollBarFront2, antiRollBarRear2], cells, 0));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompare(differentialPreload1, "Diff Preload(Nm)", differentialPreload2, cells));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompare(brakePower1, "Brake Power(%)", brakePower2, cells));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompare(brakeBias1, "Brake Bias(%)", brakeBias2, cells, 1));
        rgGrip.Rows.Add(DocUtil.GetTableRowCompare(steeringRatio1, "Steering Ratio", steeringRatio2, cells));
        Table gripTable = DocUtil.GetCenteredTable(headerWidthPercent, cells - 1);
        gripTable.RowGroups.Add(rgGrip);
        gripSection.Blocks.Add(gripTable);
        gripSection.BorderBrush = Brushes.White;
        gripSection.BorderThickness = new Thickness(0, 1, 0, 0);
        flowDocument.Blocks.Add(gripSection);



        //// Dampers Section
        Section dampersSection = new();
        TableRowGroup rgDampers = new();
        Paragraph dampersHeader = DocUtil.GetDefaultHeader("Dampers");
        dampersHeader.FontStyle = FontStyles.Italic;
        rgDampers.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), dampersHeader, DocUtil.GetDefaultParagraph(), cells));
        rgDampers.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [bumpSlowFrontLeft1, bumpSlowFrontRight1, bumpSlowRearLeft1, bumpSlowRearRight1], "Bump Slow", [bumpSlowFrontLeft2, bumpSlowFrontRight2, bumpSlowRearLeft2, bumpSlowRearRight2], cells));
        rgDampers.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [bumpFastFrontLeft1, bumpFastFrontRight1, bumpFastRearLeft1, bumpFastRearRight1], "Bump Fast", [bumpFastFrontLeft2, bumpFastFrontRight2, bumpFastRearLeft2, bumpFastRearRight2], cells));
        rgDampers.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [reboundSlowFrontLeft1, reboundSlowFrontRight1, reboundSlowRearLeft1, reboundSlowRearRight1], "Rebound Slow", [reboundSlowFrontLeft2, reboundSlowFrontRight2, reboundSlowRearLeft2, reboundSlowRearRight2], cells));
        rgDampers.Rows.Add(DocUtil.GetTableRowCompareWithLabels(tyreLocationLabels, [reboundFastFrontLeft1, reboundFastFrontRight1, reboundFastRearLeft1, reboundFastRearRight1], "Rebound Fast", [reboundFastFrontLeft2, reboundFastFrontRight2, reboundFastRearLeft2, reboundFastRearRight2], cells));
        Table dampersTable = DocUtil.GetCenteredTable(headerWidthPercent, cells - 1);
        dampersTable.RowGroups.Add(rgDampers);
        dampersSection.Blocks.Add(dampersTable);
        dampersSection.BorderBrush = Brushes.White;
        dampersSection.BorderThickness = new Thickness(0, 1, 0, 0);
        flowDocument.Blocks.Add(dampersSection);


        //// Aero
        Section aeroBalanceSection = new();
        TableRowGroup rgAero = new();
        Paragraph aeroHeader = DocUtil.GetDefaultHeader("Aero Balance");
        aeroHeader.FontStyle = FontStyles.Italic;
        rgAero.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), aeroHeader, DocUtil.GetDefaultParagraph(), cells));
        rgAero.Rows.Add(DocUtil.GetTableRowCompareWithLabels(frontOrRearLabels, [rideHeightFront1, rideHeightRear1], "Ride height(mm)", [rideHeightFront2, rideHeightRear2], cells));
        rgAero.Rows.Add(DocUtil.GetTableRowCompare(splitter1, "Splitter", splitter2, cells));
        rgAero.Rows.Add(DocUtil.GetTableRowCompare(rearWing1, "Rear Wing", rearWing2, cells));
        rgAero.Rows.Add(DocUtil.GetTableRowCompareWithLabels(frontOrRearLabels, [brakeDuctsFront1, brakeDuctsRear1], "Brake ducts", [brakeDuctsFront2, brakeDuctsRear2], cells));
        Table aeroTable = DocUtil.GetCenteredTable(headerWidthPercent, cells - 1);
        aeroTable.RowGroups.Add(rgAero);
        aeroBalanceSection.Blocks.Add(aeroTable);
        aeroBalanceSection.BorderBrush = Brushes.White;
        aeroBalanceSection.BorderThickness = new Thickness(0, 1, 0, 0);
        flowDocument.Blocks.Add(aeroBalanceSection);

        //// Electronics
        Section electronicsSection = new();
        TableRowGroup rgElectro = new();
        Paragraph electroHeader = DocUtil.GetDefaultHeader("Electronics");
        electroHeader.FontStyle = FontStyles.Italic;
        rgElectro.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), electroHeader, DocUtil.GetDefaultParagraph(), cells));
        rgElectro.Rows.Add(DocUtil.GetTableRowCompare(setup1.BasicSetup.Electronics.TC1, "TC 1", setup2.BasicSetup.Electronics.TC1, cells));
        rgElectro.Rows.Add(DocUtil.GetTableRowCompare(setup1.BasicSetup.Electronics.TC2, "TC 2", setup2.BasicSetup.Electronics.TC2, cells));
        rgElectro.Rows.Add(DocUtil.GetTableRowCompare(setup1.BasicSetup.Electronics.Abs, "ABS", setup2.BasicSetup.Electronics.Abs, cells));
        rgElectro.Rows.Add(DocUtil.GetTableRowCompare(setup1.BasicSetup.Electronics.ECUMap + 1, "Engine map", setup2.BasicSetup.Electronics.ECUMap + 1, cells));
        Table electroTable = DocUtil.GetCenteredTable(headerWidthPercent, cells - 1);
        electroTable.RowGroups.Add(rgElectro);
        electronicsSection.Blocks.Add(electroTable);
        electronicsSection.BorderBrush = Brushes.White;
        electronicsSection.BorderThickness = new Thickness(0, 1, 0, 0);
        flowDocument.Blocks.Add(electronicsSection);


        //// Strategy
        Section strategySection = new();
        TableRowGroup rgStrategy = new();
        Paragraph strategyHeader = DocUtil.GetDefaultHeader("Strategy");
        strategyHeader.FontStyle = FontStyles.Italic;
        rgStrategy.Rows.Add(DocUtil.GetTableRow(DocUtil.GetDefaultParagraph(), strategyHeader, DocUtil.GetDefaultParagraph(), cells));
        rgStrategy.Rows.Add(DocUtil.GetTableRowCompareWithLabels(frontOrRearLabels, [brakeCompoundFront1, brakeCompoundRear1], "Brake Compound", [brakeCompoundFront2, brakeCompoundRear2], cells));
        Table strategyTable = DocUtil.GetCenteredTable(headerWidthPercent, cells - 1);
        strategyTable.RowGroups.Add(rgStrategy);
        strategySection.Blocks.Add(strategyTable);
        strategySection.BorderBrush = Brushes.White;
        strategySection.BorderThickness = new Thickness(0, 1, 0, 1);
        flowDocument.Blocks.Add(strategySection);
    }

}
