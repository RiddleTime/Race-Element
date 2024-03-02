using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Drawing;
using System;

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

internal sealed class LowFuelMotorsport : AbstractOverlay
{
    public static readonly string DRIVER_RACE_API_URL = "https://api2.lowfuelmotorsport.com/api/licenseWidgetUserData/{0}";
    public static readonly string DRIVER_LICENSE_URL = "https://api.lowfuelmotorsport.de/lfmlicense/{0}?size=sm";

    private DateTime _nextTimeToRefresh = DateTime.Now;
    private DateTime _nextRaceTime = DateTime.MinValue;

    private DownloadState _lfmLicenseState = DownloadState.Undefined;
    private readonly LowFuelMotorsportConfiguration _config = new();

    private string _lfmLicense = null;
    private Font _fontFamily = null;

    public LowFuelMotorsport(Rectangle rectangle) : base(rectangle, "Low Fuel Motorsport")
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
        string text_to_display = _lfmLicense;
        TimeSpan timeDiff = _nextRaceTime.Subtract(DateTime.Now);

        if (_nextRaceTime.Year != 1)
        {
            string time = TimeSpanToString(_nextRaceTime.Subtract(DateTime.Now));
            int diff = _lfmLicense.IndexOf('\n') - time.Length;

            text_to_display = string.Format("{0}\n{1}", text_to_display, time.PadLeft(diff + time.Length, ' '));
        }

        Graphics grfx = Graphics.FromImage(new Bitmap(1, 1));
        SizeF bounds = grfx.MeasureString(text_to_display, _fontFamily);

        this.Height = (int)(bounds.Height + 1);
        this.Width = (int)(bounds.Width + 1);

        g.TextRenderingHint = global::System.Drawing.Text.TextRenderingHint.AntiAlias;
        g.DrawStringWithShadow(text_to_display, _fontFamily, Brushes.White, new PointF(0, 0));

        _lfmLicenseState = DownloadState.Undefined;
    }

    public override bool ShouldRender()
    {
        if (_config.Credentials.User.Trim() == "")
        {
            return false;
        }

        if (_lfmLicenseState == DownloadState.Undefined && DateTime.Now >= _nextTimeToRefresh)
        {
            _nextRaceTime = DateTime.MinValue;
            Task.Run(() => DownloadLFMLicense());
            _nextTimeToRefresh = DateTime.Now.AddSeconds(_config.Update.Interval);
        }

        return base.ShouldRender();
    }

    private string TimeSpanToString(TimeSpan diff)
    {
        if (diff.TotalSeconds <= 0)
        {
            return "Session Live";
        }

        long seconds = (long)(diff.TotalSeconds + 1);
        long d = 0, h = 0, m = 0, s = 0;

        if (seconds >= 60)
        {
            m = seconds / 60;
            s = seconds % 60;

            h = m / 60;
            m %= 60;

            d = h / 24;
            h %= 24;
        }


        return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", d, h, m , s);
    }

    private void GenerateLFMLicense(JObject lfm_object)
    {
        string user_vorname = lfm_object["user"]["vorname"].Value<string>();
        string user_nachname = lfm_object["user"]["nachname"].Value<string>();

        string user_license = lfm_object["user"]["license"].Value<string>();
        string user_cc_rating = lfm_object["user"]["cc_rating"].Value<string>();
        string user_sr_license = lfm_object["user"]["sr_license"].Value<string>();
        string user_safety_rating = lfm_object["user"]["safety_rating"].Value<string>();

        string sim_name = "";
        string sim_platform = "";

        if (lfm_object["sim"] != null)
        {
            sim_name = lfm_object["sim"]["name"].Value<string>();
            sim_platform = lfm_object["sim"]["platform"].Value<string>();
        }

        string output_text = string.Format
        (
            "{0} {1}    {2} ({3}) ({4})    ELO {5}    {6} ({7})",
            user_vorname.Trim(),
            user_nachname.Trim(),
            user_license.Trim(),
            user_sr_license.Trim(),
            user_safety_rating.Trim(),
            user_cc_rating.Trim(),
            sim_name.Trim(),
            sim_platform.Trim()
        ).Trim();

        if (((JArray)lfm_object["race"]).Count > 0)
        {
            string race_name = lfm_object["race"][0]["event_name"].Value<string>();
            string race_split = lfm_object["race"][0]["split"].Value<string>();
            string race_date = lfm_object["race"][0]["race_date"].Value<string>();
            string race_id = lfm_object["race"][0]["race_id"].Value<string>();

            string drivers = lfm_object["drivers"].Value<string>();
            string sof = lfm_object["sof"].Value<string>();
            _nextRaceTime = DateTime.Parse(race_date);

            string race_text = string.Format
            (
                "{0} | #{1} | Split {2} | SOF {3} | Drivers {4}",
                race_name.Trim(),
                race_id.Trim(),
                race_split.Trim(),
                sof.Trim(),
                drivers.Trim()
            ).Trim();

            int race_len = race_text.Length;
            int driver_len = output_text.Length;
            int pad = Math.Abs(driver_len - race_len);

            if (driver_len > race_len)
            {
                race_text = race_text.PadLeft(pad + race_len, ' ');
            }
            else
            {
                output_text = output_text.PadLeft(pad + driver_len, ' ');
            }

            output_text = String.Format
            (
                "{0}\n{1}",
                output_text,
                race_text
            );
        }

        _lfmLicense = output_text;
    }

    private void DownloadLFMLicense()
    {
        _lfmLicenseState = DownloadState.Downloading;

        var client = new global::System.Net.Http.HttpClient();
        string url = string.Format(DRIVER_RACE_API_URL, _config.Credentials.User);

        using global::System.Net.Http.HttpResponseMessage response = client.GetAsync(url).Result;
        using global::System.Net.Http.HttpContent content = response.Content;

        string json = content.ReadAsStringAsync().Result;
        GenerateLFMLicense(JObject.Parse(json));

        _lfmLicenseState = DownloadState.DownloadFinished;
    }
}
