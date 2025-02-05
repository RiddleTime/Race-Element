using Microsoft.VisualBasic.FileIO;
using RaceElement.Core.Jobs.Loop;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Common.SimulatorData.LocalCar;
using RaceElement.Data.Games.AssettoCorsaEvo.DataMapper;
using RaceElement.Data.Games.AssettoCorsaEvo.SharedMemory;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;

namespace RaceElement.Data.Games.AssettoCorsaEvo;

internal sealed class AssettoCorsaEvoDataProvider : AbstractSimDataProvider
{
    static int lastPhysicsPacketId = -1;


    internal override int PollingRate() => 200;

    private static string GameName => Game.AssettoCorsaEvo.ToShortName();

    public sealed override void Update(ref LocalCarData localCar, ref SessionData sessionData, ref GameData gameData)
    {
        var physicsPage = AcEvoSharedMemory.Instance.ReadPhysicsPageFile();
        if (lastPhysicsPacketId == physicsPage.PacketId) // no need to remap the physics page if packet is the same
        {
            lastPhysicsPacketId = physicsPage.PacketId;
            SimDataProvider.GameData.IsGamePaused = true;
            return;
        }
        else
        {
            lastPhysicsPacketId = physicsPage.PacketId;
            SimDataProvider.GameData.IsGamePaused = false;
        }

        LocalCarMapper.AddPhysics(ref physicsPage, ref localCar, ref sessionData);

        gameData.Name = GameName;


        // For now only physics page works, so no need to map other pages.

        //var graphicsPage = AcEvoSharedMemory.Instance.ReadGraphicsPageFile();
        //var staticPage = AcEvoSharedMemory.Instance.ReadStaticPageFile();
        //LocalCarMapper.AddGraphics(ref graphicsPage, ref localCar, ref sessionData);

        //SessionData.Instance.PlayerCarIndex = graphicsPage.PlayerCarID;
        //SimDataProvider.LocalCar.CarModel.CarClass = dummyCarClass;

    }

    private LogFileJob _logFileJob;
    internal sealed override void Start()
    {
        _logFileJob = new() { IntervalMillis = 200 };
        _logFileJob.Run();
    }

    internal override void Stop()
    {
        _logFileJob.CancelJoin();
    }

    private sealed class LogFileJob : AbstractLoopJob
    {
        private readonly string Directory = Path.Combine(SpecialDirectories.MyDocuments, "ACE");
        private readonly string File = "log.txt";
        private long _lastSize = -1;

        private FileInfo _logFile;

        public sealed override void BeforeRun() => _logFile = new(Path.Combine(Directory, File));
        public sealed override void RunAction()
        {
            try
            {
                _logFile.Refresh();
                long currentSize = _logFile.Length;

                // Only read new content if the file size has increased
                if (currentSize != _lastSize)
                {
                    if (currentSize > 0)
                    {
                        using FileStream fs = new(_logFile.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        if (_lastSize < 0 || _lastSize > fs.Length)
                        {
                            //Debug.WriteLine($"Invalid seek position: {_lastSize} for file size {fs.Length}");
                            _lastSize = 0; // Reset to start if it's out of bounds
                        }

                        fs.Seek(_lastSize, SeekOrigin.Begin);
                        using StreamReader sr = new(fs);
                        string newContent = sr.ReadToEnd();

                        if (newContent.Contains("[gameplay] [info] Game Started!"))
                        {
                            var splits = newContent.Split("\n");
                            foreach (string splitted in splits)
                            {
                                if (splitted.Contains("[gameplay] [info] Game Started!"))
                                {
                                    string carModel = splitted.Split("|")[2].Trim();
                                    SimDataProvider.LocalCar.CarModel.GameName = carModel;
                                    break;
                                }
                            }
                        }

                        //Debug.WriteLine($"New content {newContent.Count()}:\n" + newContent);
                        //sr.Close();
                        sr.Close();
                        sr.Dispose();
                        fs.Close();
                        fs.Dispose();
                    }
                    // Update last known size
                    _lastSize = currentSize;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // File might be locked, wait and retry
            }
        }
    }

    // decouple..


    // AC1 seems to have only one class. Or at least no race class info in the telemetry.
    static string dummyCarClass = "Race";
    List<string> classes = [dummyCarClass];
    public override bool IsSpectating(int playerCarIndex, int focusedIndex)
    {
        // TODO: Can we spectate other cars in the pits in AC1?
        return false;
    }

    public override Color GetColorForCategory(string category)
    {
        return Color.White;
    }

    public override List<string> GetCarClasses()
    {
        return classes;
    }

    public override bool HasTelemetry()
    {
        return lastPhysicsPacketId > 0;
    }

}
