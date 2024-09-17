using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.AssettoCorsa.DataMapper;
using RaceElement.Data.Games.AssettoCorsa.DataMapper.LocalCar;
using RaceElement.Data.Games.AssettoCorsa.SharedMemory;
using System.Drawing;
using System.Numerics;
using System.Text;
using static RaceElement.Data.Games.AssettoCorsa.SharedMemory.AcSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsa;

internal sealed class AssettoCorsa1DataProvider : AbstractSimDataProvider
{
    static int lastPhysicsPacketId = -1;

    // AC1 seems to have only one class. Or at least no race class info in the telemetry.
    static string dummyCarClass = "Race";
    List<string> classes = [dummyCarClass];

    public AssettoCorsa1DataProvider()
    {
    }
    internal override int PollingRate() => 100;

    private static string GameName { get => Game.AssettoCorsa1.ToShortName(); }

    public override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        var physicsPage = AcSharedMemory.ReadPhysicsPageFile();
        if (lastPhysicsPacketId == physicsPage.PacketId) // no need to remap the physics page if packet is the same
            return;

        var graphicsPage = AcSharedMemory.ReadGraphicsPageFile();
        var staticPage = AcSharedMemory.ReadStaticPageFile();
        var crewChiefPage = AcSharedMemory.ReadCrewChiefPageFile();

        MapLocalCar(ref graphicsPage, ref physicsPage, ref localCar);
        LocalCarMapper.WithStaticPage(staticPage, localCar);

        SessionDataMapper.WithGraphicsPage(graphicsPage, sessionData);
        SessionDataMapper.WithPhysicsPage(physicsPage, sessionData);
        SessionDataMapper.WithStaticPage(staticPage, sessionData);

        GameDataMapper.WithStaticPage(staticPage, gameData);
        gameData.Name = GameName;

        SessionData.Instance.PlayerCarIndex = graphicsPage.PlayerCarID;
        SimDataProvider.LocalCar.CarModel.CarClass = dummyCarClass;

        // TODO: AC1 does not provide delta info like ACC and iRacing. We need to try to calculate it.
        // But that would mean we need some way of telling if the car is at 66% of track and has 12345ms from 12000ms expected at
        // that precentage -> delta is 345ms.
        // SessionData.Instance.LapDeltaToSessionBestLapMs

        MapEntryList(crewChiefPage, graphicsPage);

        lastPhysicsPacketId = physicsPage.PacketId;
    }

    private void MapEntryList(PageFileCrewChief crewChiefPage, PageFileGraphics graphicsPage)
    {
        for (int i = 0; i < crewChiefPage.NumVehicles; i++)
        {
            AcsVehicleInfo vehicle = crewChiefPage.Vehicle[i];
            CarInfo carInfo = new CarInfo(i);
            carInfo.Position = vehicle.CarLeaderboardPosition;
            carInfo.CupPosition = vehicle.CarLeaderboardPosition;

            if (vehicle.IsCarInPit != 0)
            {
                carInfo.CarLocation = CarInfo.CarLocationEnum.Pitlane;
            }
            else if (vehicle.IsCarInPitline != 0)
            {
                if (vehicle.SpLineLength < 0.1F)
                {
                    carInfo.CarLocation = CarInfo.CarLocationEnum.PitExit;
                }
                else if (vehicle.SpLineLength > 0.9F)
                {

                    carInfo.CarLocation = CarInfo.CarLocationEnum.PitEntry;
                }
            }
            else
            {
                carInfo.CarLocation = CarInfo.CarLocationEnum.Track;
            }
            carInfo.CarClass = dummyCarClass;
            carInfo.RaceNumber = vehicle.CarId;

            LapInfo fastest = new LapInfo();
            fastest.LaptimeMS = vehicle.BestLapMS;
            carInfo.FastestLap = fastest;

            LapInfo last = new LapInfo();
            last.LaptimeMS = vehicle.LastLapTimeMS;
            carInfo.LastLap = last;

            LapInfo current = new LapInfo();
            current.LaptimeMS = vehicle.CurrentLapTimeMS;
            carInfo.CurrentLap = current;
            carInfo.CurrentLap.IsInvalid = vehicle.CurrentLapInvalid != 0;

            carInfo.TrackPercentCompleted = vehicle.SpLineLength;

            DriverInfo driverInfo = new DriverInfo();
            driverInfo.Name = Encoding.UTF8.GetString(vehicle.DriverName);
            carInfo.AddDriver(driverInfo);
            SessionData.Instance.AddOrUpdateCar(i, carInfo);

            // m/s -> km/h
            carInfo.Kmh = (int)(vehicle.SpeedMS * 3.6F);
            carInfo.Laps = vehicle.LapCount;

            carInfo.Location = new Vector3(vehicle.WorldPosition.X, vehicle.WorldPosition.Y, vehicle.WorldPosition.Z);

            carInfo.GapToClassLeaderMs = vehicle.CurrentLapTimeMS - crewChiefPage.Vehicle[0].CurrentLapTimeMS;
            carInfo.GapToRaceLeaderMs = carInfo.GapToClassLeaderMs;
            // TODO: double-check
            carInfo.GapToPlayerMs = vehicle.CurrentLapTimeMS - crewChiefPage.Vehicle[SessionData.Instance.PlayerCarIndex].CurrentLapTimeMS;
        }
    }


    private static void MapLocalCar(ref PageFileGraphics graphics, ref PageFilePhysics physics, ref LocalCarData localCar)
    {
        LocalCarMapper.AddGraphics(graphics, localCar);
        LocalCarMapper.AddPhysics(ref physics, ref localCar);

        PhysicsDataMapper.InsertPhysicsPage(ref physics, localCar.Physics);
    }

    public override List<string> GetCarClasses()
    {
        return classes;
    }

    public override bool HasTelemetry()
    {
        return lastPhysicsPacketId > 0;
    }


    internal override void Stop()
    {
        // No-op
    }

    public override bool IsSpectating(int playerCarIndex, int focusedIndex)
    {
        // TODO: Can we spectate other cars in the pits in AC1?
        return false;
    }

    public override Color GetColorForCategory(string category)
    {
        return Color.White;
    }
}
