using RaceElement.Broadcast.Structs;
using RaceElement.Data.ACC.Core;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker;
using RaceElement.HUD.Overlay.Configuration;
using RaceElement.Util;
using RaceElement.Util.Settings;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using static RaceElement.ACCSharedMemory;
using static RaceElement.HUD.Overlay.Configuration.OverlaySettings;

namespace RaceElement.HUD.Overlay.Internal;

public abstract class ACCOverlay : AbstractOverlay
{    

    public SPageFilePhysics pagePhysics;
    public SPageFileGraphic pageGraphics;
    public SPageFileStatic pageStatic;
    public RealtimeUpdate broadCastRealTime;
    public TrackData broadCastTrackData;
    public RealtimeCarUpdate broadCastLocalCar;

    
    public bool SubscribeToACCData = true;

    protected ACCOverlay(Rectangle rectangle, string Name) : base(rectangle, Name)
    {
    }

    public virtual void SetupPreviewData()
    {
    }

    public new bool DefaultShouldRender()
    {
        if (HudSettings.Cached.DemoMode)
            return true;

        if (IsRepositioning)
            return true;

        if (!AccProcess.IsRunning)
            return false;

        bool shouldRender = true;

        if (pageGraphics != null)
        {
            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_OFF || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || !pagePhysics.IgnitionOn)
                shouldRender = false;

            if (pageGraphics.GlobalRed)
                shouldRender = false;

            if (RaceSessionState.IsFormationLap(pageGraphics.GlobalRed, broadCastRealTime.Phase))
                shouldRender = true;

            if (pageGraphics.Status == ACCSharedMemory.AcStatus.AC_PAUSE || pageGraphics.Status == ACCSharedMemory.AcStatus.AC_REPLAY)
                shouldRender = false;

            if (broadCastRealTime.FocusedCarIndex != pageGraphics.PlayerCarID)
                shouldRender = false;
        }

        return shouldRender;
    }

    

    public override void Start(bool addTrackers = true)
    {        
        try
        {
            if (addTrackers && SubscribeToACCData)
            {
                PageStaticTracker.Tracker += PageStaticChanged;
                PageGraphicsTracker.Instance.Tracker += PageGraphicsChanged;
                PagePhysicsTracker.Instance.Tracker += PagePhysicsChanged;
                BroadcastTracker.Instance.OnRealTimeUpdate += BroadCastRealTimeChanged;
                BroadcastTracker.Instance.OnTrackDataUpdate += BroadCastTrackDataChanged;
                BroadcastTracker.Instance.OnRealTimeLocalCarUpdate += BroadCastRealTimeLocalCarUpdateChanged;
                RaceSessionTracker.Instance.OnNewSessionStarted += Instance_OnNewSessionStarted;

                // TODO(Andrei): This is just a temporal solution until we find a better way to redo the callbacks
                if (!IsPreviewing)
                    BroadcastTracker.Instance.RequestData();
            }

            pageStatic = ACCSharedMemory.Instance.ReadStaticPageFile(false);
            pageGraphics = ACCSharedMemory.Instance.ReadGraphicsPageFile(false);
            pagePhysics = ACCSharedMemory.Instance.ReadPhysicsPageFile(false);

            
            base.Start(addTrackers);
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    private void Instance_OnNewSessionStarted(object sender, Data.ACC.Database.SessionData.DbRaceSession e)
    {
        broadCastRealTime = new();
        broadCastLocalCar = new();
    }

    private void BroadCastRealTimeLocalCarUpdateChanged(object sender, RealtimeCarUpdate e)
    {
        broadCastLocalCar = e;
    }

    private void BroadCastTrackDataChanged(object sender, TrackData e)
    {
        broadCastTrackData = e;
    }

    private void BroadCastRealTimeChanged(object sender, Broadcast.Structs.RealtimeUpdate e)
    {
        broadCastRealTime = e;
    }

    public void RequestRedraw()
    {
        if (hasClosed)
        {
            this.Show();
            hasClosed = false;
        }

        this.UpdateLayeredWindow();
    }

    private void PagePhysicsChanged(object sender, SPageFilePhysics e)
    {
        pagePhysics = e;
    }

    private void PageGraphicsChanged(object sender, SPageFileGraphic e)
    {
        pageGraphics = e;
    }

    private void PageStaticChanged(object sender, SPageFileStatic e)
    {
        pageStatic = e;
    }

    public void Stop()
    {
        try
        {
            BeforeStop();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            LogWriter.WriteToLog(ex);
        }

        PageStaticTracker.Tracker -= PageStaticChanged;
        PageGraphicsTracker.Instance.Tracker -= PageGraphicsChanged;
        PagePhysicsTracker.Instance.Tracker -= PagePhysicsChanged;
        BroadcastTracker.Instance.OnRealTimeUpdate -= BroadCastRealTimeChanged;
        BroadcastTracker.Instance.OnTrackDataUpdate -= BroadCastTrackDataChanged;
        BroadcastTracker.Instance.OnRealTimeLocalCarUpdate -= BroadCastRealTimeLocalCarUpdateChanged;
        RaceSessionTracker.Instance.OnNewSessionStarted -= Instance_OnNewSessionStarted;

        Draw = false;

        this.Close();
        this.Dispose();
    }

    }
