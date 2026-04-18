using RaceElement.Data.Common.Graph;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using RaceElement.Graph;
using RaceElement.Graph.Edge;
using System.Diagnostics;

namespace RaceElement.Data.Games.RaceRoom.DataMapper;



/// <summary>
/// Very rough testing data mapper, should have state tracking of tree nodes!
/// </summary>
internal static class R3EDataGraphMapper
{
    public static void AddSharedMemory(DataGraph graph, Shared shared)
    {
        if (shared.SessionType == (int)Constants.Session.Unavailable || shared.DriverData == null)
        {
            graph.ClearGraph();
            return;
        }

        var existingCars = graph.Where(x => x is CarNode).Select(x => x as CarNode);
        var existingDrivers = graph.Where(x => x is DriverNode).Select(x => x as DriverNode);
        var existingLaps = graph.Where(x => x is LapDataNode).Select(x => x as LapDataNode);

        for (int i = 0; i < shared.DriverData.Length; i++)
        {
            var driverData = shared.DriverData[i];
            if (driverData.Place == -1) continue;

            var matchingCars = existingCars.Where(x => x?.CarNumber == driverData.DriverInfo.CarNumber);
            if (!matchingCars.Any())
            {
                var raceCarNode = new CarNode()
                {
                    CarNumber = driverData.DriverInfo.CarNumber,
                    CarModelGameID = $"{driverData.DriverInfo.ModelId}",
                    Position = driverData.Place,
                    Laps = driverData.CompletedLaps,
                };
                var raceDriverNode = new DriverNode()
                {
                    DriverId = driverData.DriverInfo.UserId,
                    Name = System.Text.Encoding.UTF8.GetString(driverData.DriverInfo.Name).TrimEnd('\0'),
                };
                graph.Add(raceCarNode);
                graph.Add(raceDriverNode);
                graph.TryAddEdge(new OwnsEdge() { ParentId = raceCarNode.Id, ChildId = raceDriverNode.Id });

                Debug.WriteLine($"Added new driver + car:\n- {raceCarNode}\n- {raceDriverNode}");
            }
            else
            {
                var raceCarNode = matchingCars.FirstOrDefault();

                if (raceCarNode != null)
                {

                    // Update data
                    if (raceCarNode.Position != driverData.Place)
                        raceCarNode.Position = driverData.Place;

                    if (raceCarNode.Laps < driverData.CompletedLaps && !graph.Edges.IsEmpty)
                    {
                        raceCarNode.Laps = driverData.CompletedLaps;


                        int[] sectors = [
                            (int)Math.Round(Math.Abs(driverData.SectorTimePreviousSelf.Sector1 * 1000.000f)),
                            (int)Math.Round(Math.Abs((driverData.SectorTimePreviousSelf.Sector2 - driverData.SectorTimePreviousSelf.Sector1) * 1000.000f)),
                            (int)Math.Round(Math.Abs((driverData.SectorTimePreviousSelf.Sector3 - driverData.SectorTimePreviousSelf.Sector2) * 1000.000f)),
                        ];
                        int lapTimeMs = sectors.Sum();

                        bool hasInvalidSectors = false;
                        foreach (int sector in sectors)
                            if (sector == 0)
                            {
                                hasInvalidSectors = true;
                                break;
                            }

                        if (lapTimeMs <= 0)
                            continue;

                        bool isValid = driverData.CurrentLapValid == 1 && !hasInvalidSectors;

                        LapDataNode lapNode = new() { SectorTimesMs = sectors, LapIndex = raceCarNode.Laps, LapTimeMs = lapTimeMs, IsValid = isValid };
                        Debug.WriteLine($"Added new lap for:\n- {raceCarNode}\n- {lapNode}");

                        graph.Add(lapNode);

                        graph.TryGetEdgesFrom(raceCarNode, out var carEdgesFrom);

                        DriverNode driverNode = existingDrivers.First(x => carEdgesFrom.Select(x => x.ChildId).Contains(x.Id));


                        graph.TryAddEdge(new OwnsEdge() { ParentId = driverNode.Id, ChildId = lapNode.Id });
                    }
                }

            }
        }
    }
}
