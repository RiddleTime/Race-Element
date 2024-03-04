using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;
using System.Drawing.Text;
using System.Drawing;
using System;
using System.Threading.Tasks;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport.LowFuelMotorsportConfiguration;
using RaceElement.HUD.Overlay.Util;
using RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport.API;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport;

[Overlay(Name = "Low Fuel Motorsport",
    Description = "Shows driver license and next upcoming races.",
    OverlayType = OverlayType.Drive,
    Authors = []
)]

internal sealed class LowFuelMotorsportOverlay : AbstractOverlay
{
    private readonly LowFuelMotorsportConfiguration _config = new();

    private Font _fontFamily;
    private ApiObject _apiObject;
    private LowFuelMotorsportJob _lfmJob;

    public LowFuelMotorsportOverlay(Rectangle rectangle) : base(rectangle, "Low Fuel Motorsport")
    {
        this.RefreshRateHz = 1f;
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
                SafetyRating = "8.08",
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
            FontFamilyConfig.SegoeMono => FontUtil.FontSegoeMono(_config.Font.Size),
            FontFamilyConfig.Conthrax => FontUtil.FontConthrax(_config.Font.Size),
            FontFamilyConfig.Orbitron => FontUtil.FontOrbitron(_config.Font.Size),
            FontFamilyConfig.Roboto => FontUtil.FontRoboto(_config.Font.Size),
            _ => FontUtil.FontSegoeMono(_config.Font.Size)
        };

        if (IsPreviewing) return;

        _lfmJob = new LowFuelMotorsportJob(_config.Connection.User)
        {
            IntervalMillis = _config.Connection.Interval * 1000,
        };

        _lfmJob.OnNewApiObject += OnNewApiObject;

        Task.Run(() => _lfmJob.RunAction());

        _lfmJob.Run();
    }
    private void OnNewApiObject(object sender, ApiObject apiObject) => _apiObject = apiObject;

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _lfmJob.OnNewApiObject -= OnNewApiObject;
        _lfmJob.Cancel();
    }

    public override void Render(Graphics g)
    {
        string licenseText = GenerateLFMLicense();
        SizeF bounds = g.MeasureString(licenseText, _fontFamily);

        this.Height = (int)(bounds.Height + 1);
        this.Width = (int)(bounds.Width + 1);

        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.DrawStringWithShadow(licenseText, _fontFamily, Brushes.White, new PointF(0, 0));
    }

    public override bool ShouldRender()
    {
        if (_config.Connection.User.Trim() == "")
            return false;

        return base.ShouldRender();
    }

    private string TimeSpanToStringCountDown(TimeSpan diff)
    {
        if (diff.TotalSeconds <= 0)
            return "Session Live";

        return $"{diff:dd\\:hh\\:mm\\:ss}";
    }

    private string GenerateLFMLicense()
    {
        if (_apiObject == null || _apiObject.User == null) return "No data";

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
                race.Sof,
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
                string time = TimeSpanToStringCountDown(race.RaceDate.Subtract(DateTime.Now));
                licenseText = string.Format("{0}\n{1}", licenseText, time.PadLeft(licenseText.IndexOf('\n'), ' '));
            }

        }

        return licenseText;
    }
}
