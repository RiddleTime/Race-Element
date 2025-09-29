
using RaceElement.Data.Games.WRC_Generations.UDP;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RaceElement.Data.Common.SimulatorData;

public sealed record GameData
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public bool IsGamePaused { get; internal set; } = false;
    public bool IsInReplay { get; internal set; } = false;

    public bool IsCarSetupScreenVisible { get; internal set; } = false;
    public bool IsRunning { get; internal set; } = false;
}

/// <summary>
/// Extension methods for calculating wheel slip from WRCGenData.
/// </summary>
internal static class SlipCalculator
{
    private const float Epsilon = 0.05f;  // Threshold for standstill (m/s)

    /// <summary>
    /// Calculates simplified longitudinal slip ratios using scalar wheel speeds vs. vehicle speed.
    /// Slip ratio formula: |v_wheel - v_speed| / v_speed (positive-only magnitude).
    /// Returns [FrontLeft, FrontRight, RearLeft, RearRight]. Returns 0 for low-speed cases.
    /// Assumes wheel speeds and Speed in m/s; ignores direction/steering for simplicity.
    /// Guaranteed: All values >= 0 (no sub-zero outputs).
    /// </summary>
    /// <param name="data">The WRCGenData packet.</param>
    /// <returns>Array of four slip ratios (dimensionless, non-negative).</returns>
    public static float[] CalculateLongitudinalSlips(this WRCGenData data)
    {
        float v_speed = data.Speed;
        if (v_speed < Epsilon)
        {
            return [0f, 0f, 0f, 0f];  // Standstill: no slip
        }

        float slip_fl = Math.Abs(data.WheelSpeedFrontLeft * 3.6f - v_speed) / v_speed;
        float slip_fr = Math.Abs(data.WheelSpeedFrontRight * 3.6f - v_speed) / v_speed;
        float slip_rl = Math.Abs(data.WheelSpeedRearLeft * 3.6f - v_speed) / v_speed;
        float slip_rr = Math.Abs(data.WheelSpeedRearRight * 3.6f - v_speed) / v_speed;

        //#if DEBUG
        //        // Safety assertion: Ensure no sub-zero values (for dev/testing)
        //        System.Diagnostics.Debug.Assert(slip_fl >= 0f && slip_fr >= 0f && slip_rl >= 0f && slip_rr >= 0f, "Slip values must be non-negative");
        //#endif

        return new[] { slip_fl, slip_fr, slip_rl, slip_rr };
    }
}