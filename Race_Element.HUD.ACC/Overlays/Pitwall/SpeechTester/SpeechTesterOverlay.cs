using RaceElement.Data.ACC.Database.LapDataDB;
using RaceElement.Data.ACC.Database.SessionData;
using RaceElement.Data.ACC.Session;
using RaceElement.Data.ACC.Tracker;
using RaceElement.Data.ACC.Tracker.Laps;
using RaceElement.Data.ACC.Tyres;
using RaceElement.HUD.Overlay.Internal;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.SpeechTester;

[Overlay(Name = "Speech Tester", Description = "Test speech output",
OverlayType = OverlayType.Pitwall)]
internal sealed class SpeechTesterOverlay : ACCOverlay
{
    private SpeechSynthesizer _synth;

    public SpeechTesterOverlay(Rectangle rectangle) : base(rectangle, "Speech Tester")
    {
        Width = 1; Height = 1;
        RequestsDrawItself = true;
    }

    public override void BeforeStart()
    {
        if (IsPreviewing) return;

        _synth = new();
        _synth.SelectVoiceByHints(VoiceGender.Male);
        _synth.SetOutputToDefaultAudioDevice();

        RaceSessionTracker.Instance.OnNewSessionStarted += OnNewSessionStarted;
        LapTracker.Instance.LapFinished += OnLapFinished;
        TyresTracker.Instance.OnTyresInfoChanged += Instance_OnTyresInfoChanged;
    }

    private TyresTracker.TyresInfo _lastTyresInfo;
    private void Instance_OnTyresInfoChanged(object sender, TyresTracker.TyresInfo e)
    {
        if (_lastTyresInfo == null)
        {
            bool anyLoss = false;
            string[] tyreNames = ["Front left", "Front right", "Rear left", "Rear right"];

            List<string> losses = [];
            for (int i = 0; i < e.PressureLoss.Length; i++)
                if (e.PressureLoss[i] > 0)
                {
                    losses.Add(tyreNames[i]);
                    anyLoss = true;
                }

            if (anyLoss)
            {
                StringBuilder sb = new();
                for (int i = 0; i < losses.Count; i++)
                {
                    sb.Append(losses[i]);
                    if (i < losses.Count - 2) sb.Append(", ");
                    if (i == losses.Count - 2) sb.Append(" and ");
                }
                _synth.SpeakAsync($"Tyre Pressure loss detected in {sb} {(losses.Count > 1 ? "tyres" : "tyre")}.");
            }
        }
        else
        {
            bool anyLoss = false;
            string[] tyreNames = ["Front left", "Front right", "Rear left", "Rear right"];

            List<string> losses = [];
            for (int i = 0; i < e.PressureLoss.Length; i++)
                if (_lastTyresInfo.PressureLoss[i] - e.PressureLoss[i] > 0)
                {
                    losses.Add(tyreNames[i]);
                    anyLoss = true;
                }

            if (anyLoss)
            {
                StringBuilder sb = new();
                for (int i = 0; i < losses.Count; i++)
                {
                    sb.Append(losses[i]);
                    if (i < losses.Count - 2) sb.Append(", ");
                    if (i == losses.Count - 2) sb.Append(" and ");
                }
                _synth.SpeakAsync($"Tyre Pressure loss detected in {sb} {(losses.Count > 1 ? "tyres" : "tyre")}.");
            }
        }
        _lastTyresInfo = e;

    }

    public override void BeforeStop()
    {
        if (IsPreviewing) return;

        RaceSessionTracker.Instance.OnNewSessionStarted -= OnNewSessionStarted;
        LapTracker.Instance.LapFinished -= OnLapFinished;

        _synth?.SpeakAsyncCancelAll();
        _synth?.Dispose();
    }

    private void OnLapFinished(object sender, DbLapData e)
    {
        if (!e.IsValid && e.InvalidatedSectorIndex != -1)
            _synth.SpeakAsync($"Last lap was invalidated in sector {e.InvalidatedSectorIndex + 1}.");
        else
        {

        }
    }

    private void OnNewSessionStarted(object sender, DbRaceSession e)
    {
        StringBuilder sb = new();
        sb.Append($"Welcome to your {(e.IsOnline ? "On-line" : "Off-line")} {ACCSharedMemory.SessionTypeToString(e.SessionType)} session.");
        bool hasRain = pageGraphics.rainIntensity > ACCSharedMemory.AcRainIntensity.Dew;
        sb.Append($"\nWeather condition is {ACCSharedMemory.AcRainIntensityToString(pageGraphics.rainIntensity)} {(hasRain ? "rain" : string.Empty)}.");
        _synth.SpeakAsync(sb.ToString());
    }

    public override void Render(Graphics g)
    {

    }
}

