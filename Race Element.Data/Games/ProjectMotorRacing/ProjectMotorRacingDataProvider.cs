using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.ProjectMotorRacing.ProjectMotorRacingUDP;
using System;
using System.Collections.Generic;
using System.Text;

namespace RaceElement.Data.Games.ProjectMotorRacing;
internal sealed class ProjectMotorRacingDataProvider : AbstractSimDataProvider
{
    private readonly DataStore _dataStore = new DataStore();
    private UDPThread _udpThread;

    internal override int PollingRate() => 100;



    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        int playerVehicleId = -1;
        foreach (var item in _dataStore.GetLeaderboard())
            if (item.m_isPlayer)
            {
                playerVehicleId = item.m_vehicleId;
                break;
            }
        if (playerVehicleId == -1) return;
        TimeSpan sinceLastWrite = _dataStore.TimeSinceLastWrite();
        if (sinceLastWrite > TimeSpan.FromSeconds(1))
            gameData.IsGamePaused = true;
        else
            gameData.IsGamePaused = false;

        var telemetry = _dataStore.GetTelemetryForVehicle(playerVehicleId);
        ;// _dataStore.m_raceInfo.m_state == UDPRaceSessionState.Active;
        if (telemetry == null) return;

        // Inputs
        localCar.Inputs.Throttle = telemetry.m_input.m_accelerator;
        localCar.Inputs.Brake = telemetry.m_input.m_brake;
        localCar.Inputs.Clutch = telemetry.m_input.m_clutch;
        localCar.Inputs.Steering = telemetry.m_input.m_steering;
        localCar.Inputs.MaxSteeringAngle = telemetry.m_general.m_steeringWheelAngle;
        localCar.Inputs.Gear = telemetry.m_input.m_gear + 1;

        // Physica
        localCar.Physics.Velocity = telemetry.m_general.m_estLinearSpeed * 3.6f;
        localCar.Physics.Acceleration = new(-telemetry.m_chassis.m_accelerationLS.z / 9.80665f, telemetry.m_chassis.m_accelerationLS.y / 9.80665f, telemetry.m_chassis.m_accelerationLS.x / 9.80665f);
        localCar.Physics.Rotation = new(telemetry.m_chassis.m_quat.x, telemetry.m_chassis.m_quat.y, telemetry.m_chassis.m_quat.z, telemetry.m_chassis.m_quat.w);

        // Engine
        localCar.Engine.IsRunning = telemetry.m_drivetrain.m_engineRunning;
        localCar.Engine.MaxRpm = (int)telemetry.m_drivetrain.m_gears.Last().m_upshiftRPM;
        localCar.Engine.Rpm = (int)telemetry.m_drivetrain.m_engineRPM;

        // Electronics
        localCar.Electronics.TractionControlActivation = telemetry.m_drivetrain.m_tractionControlActive ? 1 : 0;
        localCar.Electronics.AbsActivation = telemetry.m_general.m_absActive ? 1 : 0;

        // Tyres
        localCar.Tyres.SlipRatio = [
            telemetry.m_wheels[0].m_slipRatio,
            telemetry.m_wheels[1].m_slipRatio,
            telemetry.m_wheels[2].m_slipRatio,
            telemetry.m_wheels[3].m_slipRatio,
        ];
        for (int i = 0; i < 4; i++)
            if (localCar.Tyres.SlipRatio[i] < 0)
                localCar.Tyres.SlipRatio[i] *= -1;

        localCar.Tyres.SlipAngle = [
            telemetry.m_wheels[0].m_slipAngle,
            telemetry.m_wheels[1].m_slipAngle,
            telemetry.m_wheels[2].m_slipAngle,
            telemetry.m_wheels[3].m_slipAngle,
        ];


    }

    internal override void Start()
    {
        _udpThread = new(_dataStore, []);
        _udpThread?.StartThread();
    }

    internal override void Stop()
    {
        _udpThread?.Shutdown(this, new());
        _dataStore?.Clear();
    }


    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;
}
