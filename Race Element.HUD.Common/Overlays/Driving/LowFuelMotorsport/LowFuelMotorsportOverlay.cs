using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;
using System.Drawing.Text;
using System.Drawing;
using System.Windows.Forms;
using RaceElement.Core.Jobs.Timer;
using RaceElement.HUD.Overlay.Util;
using System.Collections.Concurrent;
using RaceElement.Data.Common;
using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games;
using RaceElement.HUD.Common.Overlays.Driving.LowFuelMotorsport.API;
using RaceElement.HUD.Common.Overlays.Driving.LowFuelMotorsport.Jobs;

namespace RaceElement.HUD.Common.Overlays.Driving.LowFuelMotorsport;

[Overlay(Name = "Low Fuel Motorsport",
    Description = "Shows driver license, next upcoming races and ELO gain/loss during the race.",
    Version = 1.10,
    OverlayType = OverlayType.Drive,
    OverlayCategory = OverlayCategory.All,
    SupportedGames = Game.Automobilista2,
    Authors = ["Andrei Jianu"]
)]

internal sealed class LowFuelMotorsportOverlay : CommonAbstractOverlay
{
    private readonly LowFuelMotorsportConfiguration _config = new();

    private ApiObject _apiObject;
    private LowFuelMotorsportElo _elo;
    private LowFuelMotorsportJob _lfmJob;

    internal readonly ConcurrentBag<Guid> _speechJobIds = [];
    private SizeF _previousTextBounds = Size.Empty;
    private Font _fontFamily;

    public LowFuelMotorsportOverlay(Rectangle rectangle) : base(rectangle, "Low Fuel Motorsport")
    {
        this.RefreshRateHz = 2f;
    }

    public override void SetupPreviewData()
    {
        _apiObject = new()
        {
            User = new()
            {
                FavSim = 1,
                FirstName = "Race",
                LastName = "Element",
                License = "Rookie",
                SrLicense = "E3",
                SafetyRating = "1520",
                CcRating = 1520,
            },
            Sim = new()
            {
                SimId = 1,
                SelectOrder = 1,
                Name = "Assetto Corsa Competizione",
                LogoUrl = "/assets/img/sims/acc.png",
                LogoBig = "/assets/img/sims/acc_new.png",
                Platform = "PC",
                Active = 1
            },
            Licenseclass = "ROOKIE",
            Sof = 1520,
        };
    }

    public override void BeforeStart()
    {
        _fontFamily = _config.Font.FontFamily switch
        {
            LowFuelMotorsportConfiguration.FontFamilyConfig.SegoeMono => FontUtil.FontSegoeMono(_config.Font.Size),
            LowFuelMotorsportConfiguration.FontFamilyConfig.Conthrax => FontUtil.FontConthrax(_config.Font.Size),
            LowFuelMotorsportConfiguration.FontFamilyConfig.Orbitron => FontUtil.FontOrbitron(_config.Font.Size),
            LowFuelMotorsportConfiguration.FontFamilyConfig.Roboto => FontUtil.FontRoboto(_config.Font.Size),
            _ => FontUtil.FontSegoeMono(_config.Font.Size)
        };

        if (IsPreviewing) return;

        if (_config.Connection.User.Trim().Length == 0)
        {
            var message = "Missing user identifier (check your configuration)\n(https://lowfuelmotorsport.com/profile/[HERE_IS_THE_ID]";
            MessageBox.Show(message, "LFM Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        _lfmJob = new LowFuelMotorsportJob(_config.Connection.User)
        {
            IntervalMillis = _config.Connection.Interval * 1000,
        };

        _lfmJob.OnNewSplitObject += OnNewSplitInfo;
        _lfmJob.OnNewApiObject += OnNewApiObject;

        _lfmJob.Run();
    }

    private void OnNewApiObject(object sender, ApiObject apiObject) => _apiObject = apiObject;
    private void OnNewSplitInfo(object sender, RaceInfo raceInfo) => _elo = new(raceInfo);

    public override void BeforeStop()
    {
        foreach (Guid jobId in _speechJobIds)
            JobTimerExecutor.Instance().Remove(jobId);

        if (IsPreviewing) return;

        _lfmJob.OnNewApiObject -= OnNewApiObject;
        _lfmJob.Cancel();
    }

    public override bool ShouldRender()
    {
        if (_config.Connection.User.Trim().Length == 0)
            return false;

        return _config.Others.ShowAlways || base.ShouldRender();
    }

    public override void Render(Graphics g)
    {
        string licenseText = GenerateLicense();

        SizeF bounds = g.MeasureString(licenseText, _fontFamily);
        if (!bounds.Equals(_previousTextBounds))
        {
            this.Height = (int)(bounds.Height + 1);
            this.Width = (int)(bounds.Width + 1);
        }
        _previousTextBounds = bounds;

        g.TextRenderingHint = TextRenderingHint.AntiAlias;

        if (licenseText != string.Empty)
        {
            using SolidBrush backgroundBrush = new(Color.FromArgb(175, 0, 0, 0));
            g.FillRoundedRectangle(backgroundBrush, new Rectangle(0, 0, Width, Height), 2);

            using StringFormat stringFormat = new() { Alignment = StringAlignment.Near };
            g.DrawStringWithShadow(licenseText, _fontFamily, Brushes.White, new PointF(0, 0), stringFormat);
        }
    }

    private static string TimeSpanToStringCountDown(TimeSpan diff)
    {
        if (diff.TotalSeconds <= 0)
            return "Session Live";

        return $"{diff:dd\\:hh\\:mm\\:ss}";
    }

    private string GenerateLicense()
    {
        if (_apiObject.User.UserName == null && !IsPreviewing)
        {
            return "No data";
        }

        string licenseText = string.Format
        (
            "{0} {1}    {2} ({3}) ({4})    ELO {5}    {6} ({7})",
            _apiObject.User.FirstName,
            _apiObject.User.LastName,
            _apiObject.User.License,
            _apiObject.User.SrLicense,
            _apiObject.User.SafetyRating,
            _apiObject.User.CcRating,
            _apiObject.Sim.Name,
            _apiObject.Sim.Platform
        ).Trim();

        if (_apiObject.Races != null && _apiObject.Races.Count > 0)
        {
            Race race = _apiObject.Races[0]; // current race or upcoming race
            string raceText = string.Format
            (
                "{0} | #{1} | Split {2} | SOF {3} | Drivers {4}",
                race.EventName,
                race.RaceId,
                race.Split,
                _apiObject.Sof,
                _apiObject.Drivers
            ).Trim();

            int raceLen = raceText.Length;
            int licenseLen = licenseText.Length;
            int pad = Math.Abs(licenseLen - raceLen);

            if (licenseLen > raceLen)
            {
                raceText = raceText.PadLeft(pad + raceLen, ' ');
            }
            else
            {
                licenseText = licenseText.PadLeft(pad + licenseLen, ' ');
            }

            licenseText = String.Format
            (
                "{0}\n{1}",
                licenseText,
                raceText
            );

            if (race.RaceDate.Year != 1)
            {
                var timeDiff = race.RaceDate.Subtract(DateTime.Now);
                string time = TimeSpanToStringCountDown(race.RaceDate.Subtract(DateTime.Now));

                if (_elo != null)
                {
                    string elo = "-";
                    int carNumber = _elo.GetCarNumber();
                    int threshold = _elo.GetPositionThreshold();

                    if (SimDataProvider.Session.SessionType == RaceSessionType.Race)
                    {
                        var pos = SimDataProvider.LocalCar.Race.ClassPosition;
                        elo = _elo.GetElo(pos).ToString();
                    }

                    time = string.Format("Car: {0}  Threshold: {1}  Elo: {2}   {3}", carNumber, threshold, elo, time);
                    licenseText = string.Format("{0}\n{1}", licenseText, time.PadLeft(licenseText.IndexOf('\n'), ' '));
                }
                else
                {
                    licenseText = string.Format("{0}\n{1}", licenseText, time.PadLeft(licenseText.IndexOf('\n'), ' '));
                }

                if (_config.Others.SpeechWarnings && _speechJobIds.Count == 0 && timeDiff.TotalMinutes >= 0)
                {
                    var speech = new LowFuelMotorsportSpeechSynthesizer(race.RaceDate.ToUniversalTime(), this);
                    JobTimerExecutor.Instance().Add(speech, DateTime.UtcNow.AddSeconds(5), out Guid jobId);
                    _speechJobIds.Add(jobId);
                }
            }
        }
        else if (!_speechJobIds.IsEmpty)
        {
            if (_speechJobIds.TryTake(out Guid toRemove))
                JobTimerExecutor.Instance().Remove(toRemove);
        }

        return licenseText;
    }
}
