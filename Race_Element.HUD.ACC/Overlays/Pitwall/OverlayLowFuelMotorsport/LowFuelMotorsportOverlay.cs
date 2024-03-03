using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;

using System.Drawing.Text;
using System.Drawing;

using System.Threading.Tasks;
using System.Threading;

using System.Net.Http;
using System;

using Newtonsoft.Json.Linq;


namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport;

enum DownloadState : int
{
    Undefined,
    Downloading,
    DownloadFinished
}

[Overlay(Name = "Low Fuel Motorsport",
    Description = "Shows driver license and next race info",
    OverlayType = OverlayType.Pitwall)]

internal sealed class LowFuelMotorsportOverlay : AbstractOverlay
{
    public static readonly string DRIVER_RACE_API_URL = "https://api2.lowfuelmotorsport.com/api/licenseWidgetUserData/{0}";
    public static readonly string DRIVER_LICENSE_URL = "https://api.lowfuelmotorsport.de/lfmlicense/{0}?size=sm";

    private DateTime _nextTimeToRefresh = DateTime.Now;
    private DateTime _nextRaceTime = DateTime.MinValue;

    private DownloadState _lfmLicenseState = DownloadState.Undefined;
    private readonly LowFuelMotorsportConfiguration _config = new();

    private string _lfmLicenseText = null;
    private Font _fontFamily = null;
    private Mutex _mutex = new();

    public LowFuelMotorsportOverlay(Rectangle rectangle) : base(rectangle, "Low Fuel Motorsport")
    {
    }

    public override void BeforeStart()
    {
        _nextRaceTime = DateTime.MinValue;
        _lfmLicenseState = DownloadState.Undefined;
        _fontFamily = Overlay.Util.FontUtil.FontSegoeMono(_config.Font.Size);
    }

    public override void Render(Graphics g)
    {
        _mutex.WaitOne();
            string licenseText = _lfmLicenseText;
        _mutex.ReleaseMutex();

        TimeSpan timeDiff = _nextRaceTime.Subtract(DateTime.Now);
        if (_nextRaceTime.Year != 1)
        {
            string time = TimeSpanToStringCountDown(_nextRaceTime.Subtract(DateTime.Now));
            licenseText = string.Format("{0}\n{1}", licenseText, time.PadLeft(licenseText.IndexOf('\n'), ' '));
        }

        SizeF bounds = g.MeasureString(licenseText, _fontFamily);
        this.Height = (int)(bounds.Height + 1);
        this.Width = (int)(bounds.Width + 1);

        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.DrawStringWithShadow(licenseText, _fontFamily, Brushes.White, new PointF(0, 0));

        _lfmLicenseState = DownloadState.Undefined;
    }

    public override bool ShouldRender()
    {
        if (_config.Connection.User.Trim() == "")
        {
            return false;
        }

        if (_lfmLicenseState == DownloadState.Undefined && DateTime.Now >= _nextTimeToRefresh)
        {
            _nextRaceTime = DateTime.MinValue;
            Task.Run(() => DownloadLFMLicense());
            _nextTimeToRefresh = DateTime.Now.AddSeconds(_config.Connection.Interval);
        }

        return base.ShouldRender();
    }

    private string TimeSpanToStringCountDown(TimeSpan diff)
    {
        if (diff.TotalSeconds <= 0)
        {
            return "Session Live";
        }

       return $"{diff:dd\\:hh\\:mm\\:ss}";
    }

    private void GenerateLFMLicense(JObject lfm_object)
    {
        string userFirstName = lfm_object["user"]["vorname"].Value<string>();
        string userLastName = lfm_object["user"]["nachname"].Value<string>();

        string userLicense = lfm_object["user"]["license"].Value<string>();
        string userElo = lfm_object["user"]["cc_rating"].Value<string>();
        string userSafetyRatingLicense = lfm_object["user"]["sr_license"].Value<string>();
        string userSafetyRating = lfm_object["user"]["safety_rating"].Value<string>();

        string gameName = "";
        string gamPlatform = "";

        if (lfm_object["sim"] != null)
        {
            gameName = lfm_object["sim"]["name"].Value<string>();
            gamPlatform = lfm_object["sim"]["platform"].Value<string>();
        }

        string licenseText = string.Format
        (
            "{0} {1}    {2} ({3}) ({4})    ELO {5}    {6} ({7})",
            userFirstName.Trim(),
            userLastName.Trim(),
            userLicense.Trim(),
            userSafetyRatingLicense.Trim(),
            userSafetyRating.Trim(),
            userElo.Trim(),
            gameName.Trim(),
            gamPlatform.Trim()
        ).Trim();

        if (((JArray)lfm_object["race"]).Count > 0)
        {
            string raceName = lfm_object["race"][0]["event_name"].Value<string>();
            string raceSplit = lfm_object["race"][0]["split"].Value<string>();
            string raceDate = lfm_object["race"][0]["race_date"].Value<string>();
            string raceId = lfm_object["race"][0]["race_id"].Value<string>();

            string drivers = lfm_object["drivers"].Value<string>();
            string sof = lfm_object["sof"].Value<string>();
            _nextRaceTime = DateTime.Parse(raceDate);

            string raceText = string.Format
            (
                "{0} | #{1} | Split {2} | SOF {3} | Drivers {4}",
                raceName.Trim(),
                raceId.Trim(),
                raceSplit.Trim(),
                sof.Trim(),
                drivers.Trim()
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
        }

        _mutex.WaitOne();
            _lfmLicenseText = licenseText;
        _mutex.ReleaseMutex();
    }

    private void DownloadLFMLicense()
    {
        _lfmLicenseState = DownloadState.Downloading;

        HttpClient client = new();
        string url = string.Format(DRIVER_RACE_API_URL, _config.Connection.User);

        using HttpResponseMessage response = client.GetAsync(url).Result;
        using HttpContent content = response.Content;

        string json = content.ReadAsStringAsync().Result;
        GenerateLFMLicense(JObject.Parse(json));

        _lfmLicenseState = DownloadState.DownloadFinished;
    }
}
