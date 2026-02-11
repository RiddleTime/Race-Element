using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.DirtRally2.UDP;

/// <summary>
/// https://github.com/BuiltClever/DirtRally2TelemetryData/blob/main/ExtractingTheDataInCode/Telemetry.cs
/// Except this is a sequential layout, looks nicer and should be easier to maintain.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1, Size = 264)]
public struct DirtRally2Data
{
    // Total Time [seconds] (Starts during the count down is rally stage and rally cross!!!)
    // Rally: Time Since start of Race, Starts at very beginning including for count down!!!
    // Rally cross: Time Since start of Race, Starts at very beginning including line up and count down!!! would need to calculate actual total race time manually (eg laptime > 0 get time to offset)
    public Single TotalTime;

    // LapTime [seconds]
    // Rally: Total Time (starts after go, continues counting across stages)
    // Rally cross: Time since start of Lap (resets after each lap)
    public Single LapTime;

    // Distance [meters]
    // Rally: Distance travelled since start of Race (continues across stages)
    // Rally cross: Distance travelled since start of Lap (resets after each lap)
    public Single Distance;

    // PercentComplete [0 to 1]
    // Rally: Percent travelled through the race
    // Rally cross: Percent travelled through the Lap (resets after each Lap)
    public Single PercentComplete; // this is the percentage of how far through the race you are (0 to 1)

    // ---------------------------------------------------------------------
    // World space positions [meters]
    // this is the car position on the map relative to the map
    public Single X; // (Horizontal left - right)
    public Single Y; // (vertical up - down)
    public Single Z; // (depth forward - backward)
                     // ---------------------------------------------------------------------

    // Speed - Velocity (Speed [meters per second]) * 3.6 for kilometres per hour, * 2.2369 = miles per hour
    public Single Speed;

    // ---------------------------------------------------------------------
    // Velocity in world space [meters per second]
    // the speed at which the car is moving along each axis
    public Single VelX; // (Horizontal left - right)
    public Single VelY; // (vertical up - down)
    public Single VelZ; // (depth forward - backward)
                      // ---------------------------------------------------------------------

    // ---------------------------------------------------------------------
    // World space right direction [decimal 0 to 1] (normalised)
    // Represents the car's rightward direction relative to the world space positions X,Y,Z
    // helps with determine lateral movement and orientation.
    public Single Xr;
    public Single Yr;
    public Single Zr;
    // ---------------------------------------------------------------------

    // ---------------------------------------------------------------------
    // World space forward direction [decimal 0 to 1] (normalised)
    // Represents the car's forward direction relative to the world space positions X,Y,Z
    // helps with speed, heading, and yaw-related calculations.
    public Single PitchVecX;
    public Single PitchVecY;
    public Single PitchVecZ;
    // ---------------------------------------------------------------------

    // ---------------------------------------------------------------------
    // Suspension position for each wheel [Scale 0 to 1000]
    // ---------------------------------------------------------------------
    public Single Susp_pos_bl; // Position of Suspension Rear Left
    public Single Susp_pos_br; // Position of Suspension Rear Right
    public Single Susp_pos_fl;// Position of Suspension Front Left
    public Single Susp_pos_fr; // Position of Suspension Front Right
                               // ---------------------------------------------------------------------

    // ---------------------------------------------------------------------
    // Velocity of Suspension [meters per second]
    // ---------------------------------------------------------------------
    public Single Susp_vel_bl; // Velocity of Suspension Rear Left
    public Single Susp_vel_br;// Velocity of Suspension Rear Right
    public Single Susp_vel_fl;// Velocity of Suspension Front Left
    public Single Susp_vel_fr;// Velocity of Suspension Front Right
                              // ---------------------------------------------------------------------


    // ---------------------------------------------------------------------
    // Velocity of Wheels [meters per second]
    // ---------------------------------------------------------------------
    public Single WheelSpeedRL; // Velocity of Wheel Rear Left
    public Single WheelSpeedRR; // Velocity of Wheel Rear Right
    public Single WheelSpeedFL; // Velocity of Wheel Front Left
    public Single WheelSpeedFR; // Velocity of Wheel Front Right
                                  // ---------------------------------------------------------------------

    // Throttle Position [0 (off) to 1 (full)]
    public Single Throttle;

    // Steering wheel position [-1 (full left) to 1 (full right)]
    public Single Steer;

    // Brake Position [0 (off) to 1 (full)]
    public Single Brake;

    // Clutch Position [0 (off) to 1 (full)]
    public Single Clutch;

    // Gear [0 = Neutral, 10 = Reverse, others are the gear]
    public Single Gear;

    // ---------------------------------------------------------------------
    // G-Forces - where 1g is equivalent to Earth's gravitational acceleration (9.81 m/s²)
    public Single GforceLat; // G-Force Lateral [G-forces (g)]
    public Single GforceLong; // G-Force Longitudinal [G-forces (g)]
                              // ---------------------------------------------------------------------

    // Lap_RX: [number]
    // Rally: [Not used always 0]
    // Rally cross: The current Lap number (0 based)
    public Single Lap_RX;

    // Engine Rate [0-1000] (divide by 100 to get RPMs [0 – 10])
    public Single EngineRate;

    // [NOT USED always 1] SLI Pro support (Sim Racing LED displays specifically the SLI-Pro hardware)
    public Single Sli_pro_native_support;

    // Current Position [number]
    // Rally: Normally 1, if behind ghost car = 2
    // Rally cross: current position in the race
    public Single CarPosition_RX;

    // [NOT USED always 0] kers energy left (Kinetic Energy Recovery System (KERS), which is used in motorsports and some electric vehicles to recover energy during braking)
    public Single Kers_level;

    // [NOT USED always 0] kers maximum energy (the maximum amount of energy that the Kinetic Energy Recovery System (KERS) can store or deploy at any given time.)
    public Single Kers_max_level;

    // [NOT USED always 0] (Drag Reduction System) 0 off, 1 On (When activated, the rear wing flap opens, reducing drag and increasing straight-line speed.)
    public Single Drs;

    // [NOT USED always 0] Traction control 0 (off) - 2 (high) 
    public Single Traction_control;

    // [NOT USED always 0] Anti Lock Brakes 0 (off) - 1 (on)
    public Single Anti_lock_brakes;

    // [NOT USED always 0] current fuel mass 
    public Single Fuel_in_tank;

    // [NOT USED always 0] fuel capacity 
    public Single Fuel_capacity;

    // [NOT USED always 0] in the pits(0 = none, 1 = pitting, 2 = in pit area)
    public Single In_pits;

    // Sector [number]
    // Rally: current stage in the track (0 based)
    // Rally cross: current sector in the Lap (0 based)
    public Single Sector; // Stage in the rally (0 based) 

    // Sector1_time [seconds]
    // NOTE: removed as can't get all sector times, better off calculating based on "Sector" changes and current "LapTime" time
    // Rally: 0 at start of race, stage 1 time once stage 1 finished (NOTE: no stage 3,4,5,- times)
    // Rally cross: 0 at start of race, sector 1 time once sector 1 finished (NOTE: no sector 3,4,5,- times)
    public Single Sector1_time;

    // Sector2_time [seconds]
    // NOTE: removed as can't get all sector times, better off calculating based on "Sector" changes and current "LapTime" time
    // Rally: 0 at start of race, stage 21 time once stage 2 finished (NOTE: no stage 3,4,5,- times)
    // Rally cross: 0 at start of race, sector 2 time once sector 2finished (NOTE: no sector 3,4,5,- times)
    public Single Sector2_time;

    // ---------------------------------------------------------------------
    // Brake Temp [Degrees Celsius]
    // ---------------------------------------------------------------------
    public Single Brakes_temp_bl; // Brake Temperature Rear Left in C
    public Single Brakes_temp_br; // Brake Temperature Rear Right in C 
    public Single Brakes_temp_fl; // Brake Temperature Front Left in C
    public Single Brakes_temp_fr; // Brake Temperature Front Right in C 
                                  // ---------------------------------------------------------------------

    // [NOT USED always 0] track size meters
    public Single Track_size;

    // [NOT USED always 0] last lap time 
    public Single Last_lap_time;

    // [NOT USED always 0] cars max RPM, at which point the rev limiter will kick in 
    public Single MaxRpm; //

    // [NOT USED always 0] cars idle RPM 
    public Single Rpm;

    // CurrentLap_RX [number]
    // Rally: [NOT USED always 0]
    // Rally cross: current Lap number (0 based)
    // NOTE: removed as is the same as Lap_RX
    public Single CurrentLap_RX;

    // TotalLaps_RX [number]
    // Rally: [NOT USED always 1]
    // Rally cross: Total number of laps in the race
    public Single TotalLaps_RX;

    // Track ID [decimal] ID of the current track.
    public Single TrackID;

    // LastLapTimeRX [number]
    // Rally: [NOT USED always 0]
    // Rally cross: the time of your last lap, 0 when on first lap
    public Single LastLapTimeRX;

    //Vehicle ID of the current car[decimal] (used with CarID and maxGears to identify car)
    public Single VehicleID;

    // Car ID of the current car [decimal] (used with Vehicle ID and MaxGears to identify the car)
    // (NOTE: this is actually idle RPMs (most times but not always) ??? can match some ambiguous cars by idle RPMs read in game not this)
    public Single CarID;

    // Max Gear of car [number] (4 speed, 5 speed, 6 speed, etc)
    public Single MaxGearNumber;

}