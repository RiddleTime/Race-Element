using RaceElement.Util.SystemExtensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.PerformanceData;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using static RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerSpeeds.CornerDataOverlay;

namespace RaceElement.HUD.ACC.Overlays.Driving.OverlayCornerSpeeds
{
    internal class CornerDataCollector
    {
        private bool IsCollecting = false;

        public void Start(CornerDataOverlay overlay)
        {
            IsCollecting = true;

            new Thread(x =>
            {
                while (IsCollecting)
                {
                    Thread.Sleep(20);

                    if (overlay != null && overlay.pagePhysics != null)
                    {
                        int currentCorner = overlay.GetCurrentCorner(overlay.pageGraphics.NormalizedCarPosition);
                        if (currentCorner == -1 && overlay._previousCorner != -1)
                        {  // corner exited
                            overlay._cornerDatas.Add(overlay._currentCorner);
                            overlay._previousCorner = -1;
                        }

                        bool isInCorner = false;
                        if (currentCorner != -1)
                        {
                            if (currentCorner == overlay._previousCorner)
                            {
                                isInCorner = true;
                                // we're still in the current corner..., check the data and build the first row
                                if (overlay._currentCorner.MinimumSpeed > overlay.pagePhysics.SpeedKmh)
                                    overlay._currentCorner.MinimumSpeed = overlay.pagePhysics.SpeedKmh;

                                List<string> columns = new List<string>();
                                string minSpeed = $"{overlay._currentCorner.MinimumSpeed:F1}";
                                minSpeed = minSpeed.FillStart(5, ' ');
                                columns.Add($"{minSpeed}");

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
                                overlay._previousCorner = currentCorner;
                                Debug.WriteLine("Entered a new corner!");
                                overlay._currentCorner = new CornerData()
                                {
                                    CornerNumber = currentCorner,
                                    MinimumSpeed = float.MaxValue
                                };
                            }
                        }
                    }
                }
            }).Start();
        }


        public void Stop()
        {
            IsCollecting = false;
        }
    }
}
