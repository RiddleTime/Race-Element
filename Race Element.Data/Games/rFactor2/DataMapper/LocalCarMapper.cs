using RaceElement.Data.Common.SimulatorData.LocalCar;
using static RaceElement.Data.Games.rFactor2.SharedMemory.SharedMemoryStructs;

namespace RaceElement.Data.Games.rFactor2.DataMapper;

internal static class LocalCarMapper
{
    public static void MapLocalCar(ref LocalCarData localCar, RF2VehicleTelemetry vehicleTelemetry, RF2VehicleScoring vehicleScoring)
    {
        localCar.Inputs.Throttle = (float)vehicleTelemetry.mUnfilteredThrottle;
        localCar.Inputs.Brake = (float)vehicleTelemetry.mUnfilteredBrake;
        localCar.Inputs.Clutch = (float)vehicleTelemetry.mUnfilteredClutch;
        localCar.Inputs.Steering = (float)vehicleTelemetry.mUnfilteredSteering;
        localCar.Inputs.MaxSteeringAngle = vehicleTelemetry.mVisualSteeringWheelRange;

        if (vehicleTelemetry.mFilteredClutch > 0 || vehicleTelemetry.mSpeedLimiter > 0)
        {
            localCar.Electronics.TractionControlActivation = 0;
            localCar.Electronics.AbsActivation = 0;
        }
        else
        {
            localCar.Electronics.TractionControlActivation = vehicleTelemetry.mFilteredThrottle < vehicleTelemetry.mUnfilteredThrottle ? 1 : 0;
            localCar.Electronics.AbsActivation = vehicleTelemetry.mFilteredBrake < vehicleTelemetry.mUnfilteredBrake ? 1 : 0;
        }

        localCar.Engine.IsIgnitionOn = vehicleTelemetry.mIgnitionStarter == 1;
        localCar.Engine.MaxRpm = (int)vehicleTelemetry.mEngineMaxRPM;
        localCar.Engine.Rpm = (int)vehicleTelemetry.mEngineRPM;
        localCar.Engine.IsRunning = localCar.Engine.Rpm > 0;

        localCar.Timing.LapTimeDeltaBestMS = (int)vehicleTelemetry.mDeltaTime;
        localCar.Timing.CurrentLaptimeMS = (int)vehicleScoring.mTimeIntoLap * 1000;
        localCar.Timing.LapTimeBestMs = (int)vehicleScoring.mBestLapTime * 1000;

        var speedMetersPerSecond = Math.Sqrt((vehicleTelemetry.mLocalVel.x * vehicleTelemetry.mLocalVel.x)
            + (vehicleTelemetry.mLocalVel.y * vehicleTelemetry.mLocalVel.y)
            + (vehicleTelemetry.mLocalVel.z * vehicleTelemetry.mLocalVel.z));
        localCar.Physics.Velocity = (float)(speedMetersPerSecond * 3.6f);
        localCar.Physics.Acceleration = new((float)vehicleTelemetry.mLocalAccel.x / 9.80665f, (float)vehicleTelemetry.mLocalAccel.y / 9.80665f, (float)-vehicleTelemetry.mLocalAccel.z / 9.80665f);

        localCar.Tyres.SlipRatio = [
            CalculateWheelSlipRatio(vehicleTelemetry.mWheels[0]),
            CalculateWheelSlipRatio(vehicleTelemetry.mWheels[1]),
            CalculateWheelSlipRatio(vehicleTelemetry.mWheels[2]),
            CalculateWheelSlipRatio(vehicleTelemetry.mWheels[3]),
        ];
    }

    private static float CalculateWheelSlipRatio(RF2Wheel wheel)
    {
        // Convert tire radius from centimeters to meters
        double tireRadius = wheel.mStaticUndeflectedRadius / 100.0;

        // Calculate wheel tangential velocity (Vw = ω * r)
        double wheelVelocity = wheel.mRotation * tireRadius;

        // Ground velocity (Vg)
        double groundVelocity = wheel.mLongitudinalGroundVel;

        // Avoid division by zero using a small epsilon
        double epsilon = 0.1;
        double denominator = Math.Max(Math.Abs(groundVelocity), epsilon);

        // Calculate slip ratio: (Vw - Vg) / max(|Vg|, ε)
        float slipRatio = (float)((wheelVelocity - groundVelocity) / denominator);
        if (slipRatio < 0) slipRatio *= -1;
        return slipRatio * 10f;
    }
}
