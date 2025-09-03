using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.Forza.ForzaUDP;

static class ForzaMotorsportsData
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SledData
    {
        /// <summary>
        /// Used as bool, higher than zero is true.
        /// </summary>
        public float IsRaceOn; 
        public uint TimestampMs;
        public float EngineMaxRpm;
        public float EngineIdleRpm;
        public float CurrentEngineRpm;
        public float AccelerationX;
        public float AccelerationY;
        public float AccelerationZ;
        public float VelocityX;
        public float VelocityY;
        public float VelocityZ;
        public float AngularVelocityX;
        public float AngularVelocityY;
        public float AngularVelocityZ;
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float NormSuspensionTravelFl;
        public float NormSuspensionTravelFr;
        public float NormSuspensionTravelRl;
        public float NormSuspensionTravelRr;
        public float TireSlipRatioFl;
        public float TireSlipRatioFr;
        public float TireSlipRatioRl;
        public float TireSlipRatioRr;
        public float WheelRotationSpeedFl;
        public float WheelRotationSpeedFr;
        public float WheelRotationSpeedRl;
        public float WheelRotationSpeedRr;
        public float WheelOnRumbleStripFl;
        public float WheelOnRumbleStripFr;
        public float WheelOnRumbleStripRl;
        public float WheelOnRumbleStripRr;
        public float WheelInPuddleFl;
        public float WheelInPuddleFr;
        public float WheelInPuddleRl;
        public float WheelInPuddleRr;
        public float SurfaceRumbleFl;
        public float SurfaceRumbleFr;
        public float SurfaceRumbleRl;
        public float SurfaceRumbleRr;
        public float TireSlipAngleFl;
        public float TireSlipAngleFr;
        public float TireSlipAngleRl;
        public float TireSlipAngleRr;
        public float TireCombinedSlipFl;
        public float TireCombinedSlipFr;
        public float TireCombinedSlipRl;
        public float TireCombinedSlipRr;
        public float SuspensionTravelMetersFl;
        public float SuspensionTravelMetersFr;
        public float SuspensionTravelMetersRl;
        public float SuspensionTravelMetersRr;
        public byte CarOrdinal;
        public byte CarClass;
        public byte CarPerformanceIndex;
        public byte DriveTrain;
        public byte NumCylinders;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DashData
    {
        public float PositionX;
        public float PositionY;
        public float PositionZ;
        public float Speed;
        public float Power;
        public float Torque;
        public float TireTempFl;
        public float TireTempFr;
        public float TireTempRl;
        public float TireTempRr;
        public float Boost;
        public float Fuel;
        public float Distance;
        public float BestLapTime;
        public float LastLapTime;
        public float CurrentLapTime;
        public float CurrentRaceTime;
        public ushort Lap;
        public byte RacePosition;
        public byte Accelerator;
        public byte Brake;
        public byte Clutch;
        public byte Handbrake;
        public byte Gear;
        public sbyte Steer;
        public byte NormalDrivingLine;
        public byte NormalAiBrakeDifference;
    }

    private const int SLED_PACKET_LENGTH = 232; // FM7
    private const int DASH_PACKET_LENGTH = 311; // FM7
    private const int FH4_PACKET_LENGTH = 324; // FH4
    private const int FM8_PACKET_LENGTH = 331; // FM8

    public static bool IsSledFormat(byte[] packet) => packet.Length == SLED_PACKET_LENGTH;
    public static bool IsDashFormat(byte[] packet) => packet.Length == DASH_PACKET_LENGTH;

    /// <summary>
    /// FH4 format is used for FH4 and FH5
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    public static bool IsFH4Format(byte[] packet) => packet.Length == FH4_PACKET_LENGTH;
    public static bool IsFM8Format(byte[] packet) => packet.Length == FM8_PACKET_LENGTH;

    public static SledData GetSledData(byte[] bytes)
    {
        if (bytes.Length < SLED_PACKET_LENGTH)
            throw new ArgumentException("Not enough bytes for SledData");

        return MemoryMarshal.Cast<byte, SledData>(bytes)[0];
    }

    public static DashData GetDashData(byte[] bytes)
    {
        if (bytes.Length < DASH_PACKET_LENGTH)
            throw new ArgumentException("Not enough bytes for DashData");

        return MemoryMarshal.Cast<byte, DashData>(bytes.AsSpan(SLED_PACKET_LENGTH))[0];
    }
}