using System.Diagnostics;
using System.Threading;
using static RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerData.CornerDataOverlay;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerData
{
    internal class CornerDataCollector
    {
        private bool IsCollecting = false;

        public void Start(CornerDataOverlay overlay)
        {
            if (overlay == null) return;

            IsCollecting = true;
            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(20);
                    Collect(overlay);
                }

                IsCollecting = false;
            }).Start();
        }


        public void Stop()
        {
            IsCollecting = false;
        }

        private void Collect(CornerDataOverlay overlay)
        {
            if (overlay.pagePhysics != null)
            {
                int currentCornerIndex = overlay.GetCurrentCorner(overlay.pageGraphics.NormalizedCarPosition);

                if (currentCornerIndex == -1 && overlay._previousCorner != -1)
                {
                    CornerExited(overlay);
                }

                if (currentCornerIndex != -1)
                {
                    if (currentCornerIndex == overlay._previousCorner)
                    {
                        // we're still in the current corner..., check the data
                        if (overlay._currentCorner.MinimumSpeed > overlay.pagePhysics.SpeedKmh)
                            overlay._currentCorner.MinimumSpeed = overlay.pagePhysics.SpeedKmh;

                        overlay._currentCorner.AverageSpeed = (overlay.pagePhysics.SpeedKmh + overlay._currentCorner.AverageSpeed) / 2;

                        if (overlay._config.Data.MaxLatG)
                        {
                            float latG = overlay.pagePhysics.AccG[0];
                            if (latG < 0) latG *= -1;
                            if (overlay._currentCorner.MaxLatG < latG)
                                overlay._currentCorner.MaxLatG = latG;
                        }
                    }
                    else
                    {
                        if (overlay._previousCorner != -1)
                        {
                            CornerExited(overlay);
                        }

                        // entered a new corner
                        overlay._previousCorner = currentCornerIndex;
                        overlay._currentCorner = new CornerData()
                        {
                            CornerNumber = currentCornerIndex,
                            MinimumSpeed = float.MaxValue,
                            AverageSpeed = overlay.pagePhysics.SpeedKmh,
                            EntryDeltaMilliseconds = overlay.pageGraphics.DeltaLapTimeMillis,
                        };
                    }
                }
            }
        }

        private void CornerExited(CornerDataOverlay overlay)
        {
            // corner exited
            overlay._currentCorner.ExitDeltaMilliseconds = overlay.pageGraphics.DeltaLapTimeMillis;
            overlay._cornerDatas.Add(overlay._currentCorner);
            overlay._previousCorner = -1;
            Debug.WriteLine("Exited corner");
        }

    }

}
