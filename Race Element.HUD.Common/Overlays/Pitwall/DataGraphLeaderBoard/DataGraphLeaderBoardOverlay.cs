using RaceElement.Data.Common;
using RaceElement.Data.Common.Graph;
using RaceElement.Data.Games;
using RaceElement.Graph;
using RaceElement.Graph.Edge;
using RaceElement.HUD.Overlay.Internal;
using RaceElement.HUD.Overlay.Util;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.Json;

namespace RaceElement.HUD.Common.Overlays.Pitwall.DataGraphLeaderBoard;

[Overlay(
    Name = "Data Graph Leaderboard",
    Description = "A data test for the data-graph.",
    Authors = ["Reinier Klarenberg"],
    OverlayType = OverlayType.Pitwall,
    SupportedGames = Game.RaceRoom
)]
internal sealed class DataGraphLeaderBoardOverlay(Rectangle rectangle) : CommonAbstractOverlay(rectangle, "Data Graph Leaderboard")
{
    private InfoPanel _panel;

    public sealed override void BeforeStart()
    {
        if (IsPreviewing) return;

        _panel = new InfoPanel(12, 550);
        Width = 550;
        Height = 250;
        RefreshRateHz = 3;
    }

    public sealed override void BeforeStop()
    {
        _panel?.Dispose();
    }

    public sealed override bool ShouldRender() => true;

    public sealed override void Render(Graphics g)
    {
        if (SimDataProvider.RacingGraph.IsEmpty || IsPreviewing) return;

        var graph = SimDataProvider.RacingGraph;

        IEnumerable<LapDataNode?> allValidLapTimes = graph.Where(x => x is LapDataNode).Select(x => x as LapDataNode);
        IEnumerable<DriverNode?> allDrivers = graph.Where(x => x is DriverNode).Select(x => x as DriverNode);
        IEnumerable<CarNode?> allCars = graph.Where(x => x is CarNode).Select(x => x as CarNode);

        if (allValidLapTimes.Any())
        {
            LapDataNode? fastestLap = allValidLapTimes.Where(x => x.IsValid).MinBy(x => x?.LapTimeMs);
            if (fastestLap != null)
            {
                _ = graph.TryGetEdgesTo(fastestLap.Id, out var fastestLapEdges);
                if (fastestLapEdges.Count != 0)
                {
                    var fastestDriverId = fastestLapEdges.First().ParentId;
                    var fastestDriver = allDrivers.First(x => x?.Id == fastestDriverId);

                    _ = graph.TryGetEdgesTo(fastestDriverId, out var driverEdgesTo);

                    CarNode fastestCar = allCars.First(x => driverEdgesTo.Select(x => x.ParentId).Contains(x.Id));
                    _panel.AddLine("Fastest", $"#{fastestCar.CarNumber} - {fastestDriver.Name} - L{fastestLap.LapIndex}");

                    _panel.AddLine("Fastest Lap", $"{TimeSpan.FromMilliseconds(fastestLap.LapTimeMs):mm\\:ss\\.fff} ");

                    StringBuilder sectorTimes = new();
                    for (int i = 0; i < fastestLap.SectorTimesMs.Length; i++)
                    {
                        _ = sectorTimes.Append($"S{i + 1}: {TimeSpan.FromMilliseconds(fastestLap.SectorTimesMs[i]):mm\\:ss\\.fff}");
                        if (i < fastestLap.SectorTimesMs.Length - 1)
                            _ = sectorTimes.Append(", ");
                    }
                    _panel.AddLine("Sectors", $" {sectorTimes}");
                }
            }
        }


        if (allValidLapTimes.Any())
        {
            _panel.AddLine("Laps", $"{allValidLapTimes.Count()}");
            int[] avgLapTimeMs = allValidLapTimes.Select(x => x.LapTimeMs).ToArray();
            AddTimeStats(_panel, [.. avgLapTimeMs]);
        }

        _panel.AddLine("Nodes", $"{graph.Count}");
        _panel.AddLine("Edges", $"{graph.Edges.Count}");

        _panel.Draw(g);
    }

    private static (double min, double max, double mean, double median, double std) CalculateMetrics(List<double> list)
    {
        var mean = list.Average();
        var std = Math.Sqrt(list.Aggregate(0.0, (a, x) => a + (x - mean) * (x - mean)) / list.Count);
        var sorted = list.OrderBy(x => x).ToList();
        var median = sorted.Count % 2 == 0 ? (sorted[sorted.Count / 2 - 1] + sorted[sorted.Count / 2]) / 2 : sorted[sorted.Count / 2];
        return (sorted[0], sorted[^1], mean, median, std);
    }

    private static void AddTimeStats(InfoPanel panel, List<double> data)
    {
        var (min, max, mean, median, std) = CalculateMetrics(data);
        panel.AddLine("Min", $"{TimeSpan.FromMilliseconds(min):mm\\:ss\\.fff}");
        panel.AddLine("Avg", $"{TimeSpan.FromMilliseconds(mean):mm\\:ss\\.ffff}");
        panel.AddLine("Max", $"{TimeSpan.FromMilliseconds(max):mm\\:ss\\.fff}");
        panel.AddLine("Median", $"{TimeSpan.FromMilliseconds(median):mm\\:ss\\.ffff}");
        panel.AddLine("StDev", $"{TimeSpan.FromMilliseconds(std):s\\.ffff}");
    }
}
