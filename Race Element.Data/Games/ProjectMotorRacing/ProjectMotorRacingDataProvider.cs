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

    private int _selectedVehicleId = -1;

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

        var telemetry = _dataStore.GetTelemetryForVehicle(playerVehicleId);

        // Inputs
        localCar.Inputs.Throttle = telemetry.m_input.m_accelerator;
        localCar.Inputs.Brake = telemetry.m_input.m_brake;
        localCar.Inputs.Clutch = telemetry.m_input.m_clutch;
        localCar.Inputs.Steering = telemetry.m_input.m_steering;
        localCar.Inputs.MaxSteeringAngle = telemetry.m_general.m_steeringWheelAngle;

        // Physica
        localCar.Physics.Velocity = telemetry.m_general.m_estRollingSpeed * 3.6f;

        // Tyres
        localCar.Tyres.SlipRatio = [
            telemetry.m_wheels[0].m_slipRatio,
            telemetry.m_wheels[1].m_slipRatio,
            telemetry.m_wheels[2].m_slipRatio,
            telemetry.m_wheels[3].m_slipRatio,
        ];
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
        _udpThread.StartThread();
    }

    internal override void Stop()
    {
        _udpThread.Shutdown(this, new());
        _dataStore.Clear();
    }


    public override List<string> GetCarClasses() => [];

    public override bool HasTelemetry() => false;
}
