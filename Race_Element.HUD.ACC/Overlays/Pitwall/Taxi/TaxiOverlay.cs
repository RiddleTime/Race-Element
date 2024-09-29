using RaceElement.Core.Jobs.LoopJob;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracks;
using RaceElement.HUD.ACC.Overlays.Driving.TrackMap;
using RaceElement.HUD.ACC.Overlays.Pitwall.OverlayReplayAssist;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.Taxi;

#if DEBUG
[Overlay(
    Name = "Taxi",
    Description = "No more driving (Experimental Self Driving AI, Uses Arrow keys to drive)",
    OverlayType = OverlayType.Pitwall
)]
#endif
internal sealed class TaxiOverlay(Rectangle rectangle) : AbstractOverlay(rectangle, "Taxi")
{
    private InputSimulatorStandard.KeyboardSimulator kb;
    private TrackMapData trackMap;


    private SteeringJob _steeringJob;
    public override void BeforeStart()
    {
        if (IsPreviewing) return;

        kb = new InputSimulatorStandard.KeyboardSimulator();

        trackMap = new();
        trackMap.LoadMapFromFile();
        RefreshRateHz = 3;
        RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;


        _steeringJob = new(this) { IntervalMillis = 100 };
        _steeringJob.Run();
    }

    private void Instance_OnNewSessionStarted(object sender, RaceElement.Data.ACC.Database.SessionData.DbRaceSession e)
    {
        trackMap = new();
        trackMap.LoadMapFromFile();
    }

    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;
        _steeringJob.CancelJoin();
    }
    public override void Render(Graphics g)
    {
        if (pagePhysics.SpeedKmh < 30 && AccHasFocus())
        {
            kb.KeyDown(InputSimulatorStandard.Native.VirtualKeyCode.UP);
            Thread.Sleep(200);
            kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.UP);
        }
    }

    private sealed class SteeringJob(TaxiOverlay overlay) : AbstractLoopJob
    {
        private InputSimulatorStandard.KeyboardSimulator kb;

        public override void BeforeRun()
        {
            kb = new();
            kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.RIGHT);
            kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.LEFT);
        }

        public override void AfterCancel()
        {
            kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.RIGHT);
            kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.LEFT);
        }

        public override void RunAction()
        {
            if (!overlay.AccHasFocus()) return;

            if (overlay.pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE) return;
            if (overlay.pageGraphics.Status == ACCSharedMemory.AcStatus.AC_REPLAY) return;

            var nextPoint = overlay.GetNext();

            if (nextPoint == null)
            {
                Debug.WriteLine("No Next Poitn");
                return;
            }


            PointF player = new();
            for (var i = 0; i < overlay.pageGraphics.ActiveCars; ++i)
            {
                if (overlay.pageGraphics.CarIds[i] != overlay.pageGraphics.PlayerCarID)
                {
                    continue;
                }
                else
                {
                    player = new(overlay.pageGraphics.CarCoordinates[i].X, overlay.pageGraphics.CarCoordinates[i].Z);
                    break;
                }
            }

            // get heading to next point and press arrow left or arrow right to correct the cars heading
            Debug.WriteLine($"{nextPoint.X}, {nextPoint.Y} - {player.X}, {player.Y}");
            var degreeToNextPoint = DegreesBetween2Points(player.X, player.Y, nextPoint.X, nextPoint.Y);

            Vector2 a = new();
            Vector2 b = new();

            Debug.WriteLine($"Bearing to Next: {degreeToNextPoint}");
            float carDirection = (float)(Single.RadiansToDegrees((float)(overlay.pagePhysics.Heading) - 90));
            //if (carDirection < 0) carDirection *= -1;
            Debug.WriteLine($"Car Heading: {carDirection:F2} - {Single.RadiansToDegrees(overlay.pagePhysics.Heading):F5}");
            var diff = degreeToNextPoint - carDirection;
            diff = (diff) % 180;
            diff *= -1;
            Debug.WriteLine($"Diff to Next: {diff}");
            if (diff < -10)
            {
                if (diff > -35)
                {
                    Debug.WriteLine("short R");
                    kb.KeyDown(InputSimulatorStandard.Native.VirtualKeyCode.RIGHT);
                    Thread.Sleep(75);
                    kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.RIGHT);
                }
                else if (diff <= -50)
                {
                    Debug.WriteLine("long R");
                    kb.KeyDown(InputSimulatorStandard.Native.VirtualKeyCode.RIGHT);
                    Thread.Sleep(diff > -150 ? 100 : 125);
                    kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.RIGHT);
                }
            }
            else if (diff > 10)
            {
                if (diff >= 35)
                {
                    Debug.WriteLine("short L");
                    kb.KeyDown(InputSimulatorStandard.Native.VirtualKeyCode.LEFT);
                    Thread.Sleep(75);
                    kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.LEFT);
                }
                else if (diff < 50)
                {
                    Debug.WriteLine("long L");
                    kb.KeyDown(InputSimulatorStandard.Native.VirtualKeyCode.LEFT);
                    Thread.Sleep(diff < 150 ? 100 : 125);
                    kb.KeyUp(InputSimulatorStandard.Native.VirtualKeyCode.LEFT);
                }
            }
        }
    }

    public TrackPoint GetNext()
    {
        if (trackMap._trackedPositions.Count > 0)
        {
            var trackData = TrackData.GetCurrentTrack(pageStatic.Track);
            float meterSpline = (float)trackData.TrackLength / trackMap._trackedPositions.Count / 100f;
            var offset = meterSpline * 5;

            offset = 0.0005f;
            Vector2 player = new();
            for (var i = 0; i < pageGraphics.ActiveCars; ++i)
            {
                if (pageGraphics.CarIds[i] != pageGraphics.PlayerCarID)
                {
                    continue;
                }
                else
                {
                    player = new(pageGraphics.CarCoordinates[i].X, pageGraphics.CarCoordinates[i].Z);
                    break;
                }
            }

            //Debug.WriteLine($"offset:{offset}");
            var next = trackMap._trackedPositions.Find(x =>
            {
                if (x.Spline < pageGraphics.NormalizedCarPosition + offset) return false;

                float distance = Vector2.Distance(new Vector2(x.X, x.Y), player);
                //Debug.WriteLine($"dist {distance}");

                return distance < 30 && (x.Spline > pageGraphics.NormalizedCarPosition + offset && x.Spline < pageGraphics.NormalizedCarPosition + offset * 5f);
                //    return distance > 3 && distance < 50;

                //return distance < 50 && x.Spline > pageGraphics.NormalizedCarPosition + offset / 2;
            });
            return next;
        }
        Debug.WriteLine("Didn't find a next point");
        return null;
    }

    #region https://stackoverflow.com/a/2042883
    static double DegreeBearing(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        var dLon = ToRad(lon2 - lon1);
        var dPhi = Math.Log(
            Math.Tan(ToRad(lat2) / 2 + Math.PI / 4) / Math.Tan(ToRad(lat1) / 2 + Math.PI / 4));
        if (Math.Abs(dLon) > Math.PI)
            dLon = dLon > 0 ? -(2 * Math.PI - dLon) : (2 * Math.PI + dLon);
        return ToBearing(Math.Atan2(dLon, dPhi));
    }

    static double DegreesBetween2Points(double X1, double Y1, double X2, double Y2)
    {
        var deltaX = X2 - X1;
        var deltaY = Y2 - Y1;

        //pythagoras theorem for distance
        var distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        //atan2 for angle
        var radians = Math.Atan2(deltaY, deltaX);

        //radians into degrees
        var angle = radians * (180 / Math.PI); deltaX = X2 - X1;
        deltaY = Y2 - Y1;

        //pythagoras theorem for distance
        distance = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);

        //atan2 for angle
        radians = Math.Atan2(deltaY, deltaX);

        //radians into degrees
        angle = radians * (180 / Math.PI);
        return angle;
    }

    public static double ToRad(double degrees)
    {
        return degrees * (Math.PI / 180);
    }

    public static double ToDegrees(double radians)
    {
        return radians * 180 / Math.PI;
    }

    public static double ToBearing(double radians)
    {
        // convert radians to degrees (as bearing: 0...360)
        return (ToDegrees(radians) + 360) % 360;
    }

    #endregion

    private Process _accProcess;
    private bool AccHasFocus() => User32.GetForegroundWindow() == GetAccProcess()?.MainWindowHandle;
    private Process GetAccProcess()
    {
        try
        {
            _accProcess = Process.GetProcessesByName("AC2-Win64-Shipping").FirstOrDefault();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
        return _accProcess;
    }







    private class TrackMapData
    {
        internal readonly List<TrackPoint> _trackedPositions = [];

        private readonly byte[] _magic = [(byte)'r', (byte)'e', (byte)'t', (byte)'m'];
        internal void LoadMapFromFile()
        {
            //const string msg = "Tracking state -> Map found on disk, loading it.";
            //OnMapProgressCallback?.Invoke(null, msg);

            var trackName = ACCSharedMemory.Instance.PageFileStatic.Track.ToLower();
            var founds = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.EndsWith($"{trackName}.bin"));
            if (!founds.Any())
                Debug.WriteLine("F no tracks found!");

            using Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(founds.First());
            using BinaryReader binaryReader = new(resourceStream);

            {
                var magic = binaryReader.ReadBytes(4);  // Magic number
                binaryReader.ReadInt16();               // Version
                binaryReader.ReadInt32();               // Reserved 01 (future use)
                binaryReader.ReadInt32();               // Reserved 02 (future use)

                if (magic[0] != _magic[0] || magic[1] != _magic[1] || magic[2] != _magic[2] || magic[3] != _magic[3])
                {
                    binaryReader.Close();
                    resourceStream.Close();

                    {
                        string msg = "Tracking state -> Corrupt map file. Delete the file and track again the track.\n" + trackName; //path;
                    }

                    //return CreationState.Error;
                }
            }

            while (resourceStream.Position < resourceStream.Length)
            {
                {
                    //OnMapProgressCallback?.Invoke(null, String.Format(msg, ((float)resourceStream.Position / resourceStream.Length) * 100.0f));
                }

                TrackPoint pos = new()
                {
                    X = binaryReader.ReadSingle(),
                    Y = binaryReader.ReadSingle(),
                    Spline = binaryReader.ReadSingle(),
                };

                _trackedPositions.Add(pos);
            }

            binaryReader.Close();
            resourceStream.Close();

            List<TrackPoint> newPoints = [];
            foreach (var group in _trackedPositions.GroupBy(x => x.Spline))
                newPoints.Add(group.First());

            _trackedPositions.Clear();
            _trackedPositions.InsertRange(0, newPoints);

            Debug.WriteLine($"TRACK IS SET {_trackedPositions.Count}");
        }
    }
}
