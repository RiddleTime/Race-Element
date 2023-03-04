using RaceElement.Data.ACC.Core;
using RaceElement.Data.ACC.EntryList;
using RaceElement.Util;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.Data.ACC.Tracker
{
    public class PageGraphicsTracker : IDisposable
    {
        private static PageGraphicsTracker _instance;
        public static PageGraphicsTracker Instance
        {
            get
            {
                _instance ??= new PageGraphicsTracker();

                return _instance;
            }
        }

        public event EventHandler<SPageFileGraphic> Tracker;

        private bool isTracking = false;

        private readonly Task trackingTask;

        private PageGraphicsTracker()
        {

            isTracking = true;

            trackingTask = Task.Run(() =>
            {
                while (isTracking)
                {
                    if (AccProcess.IsRunning)
                    {
                        Thread.Sleep(2);
                        Tracker?.Invoke(this, ACCSharedMemory.Instance.ReadGraphicsPageFile());
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
            });

            Task.Run(() =>
            {
                int previousPhysicsPackedId = 0;
                while (isTracking)
                {
                    try
                    {
                        Thread.Sleep(100);

                        if (AccProcess.IsRunning)
                        {
                            SPageFileGraphic sPageFileGraphic = ACCSharedMemory.Instance.ReadGraphicsPageFile(false);
                            SPageFilePhysics sPageFilePhysics = ACCSharedMemory.Instance.ReadPhysicsPageFile(false);

                            if (sPageFileGraphic.Status == AcStatus.AC_OFF)
                            {
                                if (BroadcastTracker.Instance.IsConnected)
                                {
                                    if (sPageFilePhysics.PacketId <= previousPhysicsPackedId || sPageFilePhysics.PacketId == 0)
                                    {
                                        BroadcastTracker.Instance.Disconnect();
                                        //#if DEBUG
                                        EntryListTracker.Instance.Stop();
                                        //#endif

                                        previousPhysicsPackedId = 0;
                                    }
                                }
                                else
                                {
                                    if (!BroadcastTracker.Instance.IsConnected)
                                        if (sPageFilePhysics.PacketId != previousPhysicsPackedId)
                                        {
                                            BroadcastTracker.Instance.Connect();
                                            //#if DEBUG
                                            EntryListTracker.Instance.Start();
                                            //#endif
                                        }

                                }


                                previousPhysicsPackedId = sPageFilePhysics.PacketId;

                                if (SetupHiderTracker.Instance.IsTracking)
                                    SetupHiderTracker.Instance.Dispose();
                            }
                            else if (sPageFileGraphic.Status != AcStatus.AC_OFF)
                            {
                                if (!BroadcastTracker.Instance.IsConnected)
                                {
                                    BroadcastTracker.Instance.Connect();
                                    //#if DEBUG
                                    EntryListTracker.Instance.Start();
                                    //#endif
                                }

                                if (!SetupHiderTracker.Instance.IsTracking)
                                    SetupHiderTracker.Instance.StartTracker();
                            }
                        }
                        else
                        {
                            if (BroadcastTracker.Instance.IsConnected)
                            {
                                BroadcastTracker.Instance.Disconnect();
                                //#if DEBUG
                                EntryListTracker.Instance.Stop();
                                //#endif
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LogWriter.WriteToLog(e);
                        Debug.WriteLine(e);
                    }
                }
            });

            _instance = this;
        }

        public void Dispose()
        {
            isTracking = false;
            trackingTask.Dispose();
        }
    }
}
