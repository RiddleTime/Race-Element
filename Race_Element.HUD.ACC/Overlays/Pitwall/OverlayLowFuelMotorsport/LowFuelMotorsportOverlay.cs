using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;

using System.Drawing.Text;
using System.Drawing;
using System;
using System.Threading.Tasks;
using static RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport.LowFuelMotorsportConfiguration;
using RaceElement.HUD.Overlay.Util;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport;

[Overlay(Name = "Low Fuel Motorsport",
    Description = "Shows driver license and next race info",
    OverlayType = OverlayType.Pitwall)]

internal sealed class LowFuelMotorsportOverlay : AbstractOverlay
{
    private readonly LowFuelMotorsportConfiguration _config = new();
    private Font _fontFamily = null;

    private LowFuelMotorsportJob _fetchJob = null;
    private LowFuelMotorsportAPI _api = null;

    public LowFuelMotorsportOverlay(Rectangle rectangle) : base(rectangle, "Low Fuel Motorsport")
    {
        this.RefreshRateHz = 1f;
    }

    public override void SetupPreviewData()
    {
        _api = new(new LowFuelMotorsportUserLicense()
        {
            Elo = "9000",
            FirstName = "Race",
            LastName = "Element",
            GameName = "ACC",
            GamePlatform = "",
            License = "",
            SafetyRating = "",
            SafetyRatingLicense = "",
        },
        new LowFuelMotorsportNextRace()
        {
            Date = DateTime.Now.AddMinutes(45),
            NumberOfDrivers = "28",
            Name = "",
            Sof = string.Empty,
            Split = string.Empty,
            Id = string.Empty,
            // TODO: add some data for the preview
        });
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

        _fetchJob = new LowFuelMotorsportJob(_config.Connection.User)
        {
            IntervalMillis = _config.Connection.Interval * 1000,
        };

        _fetchJob.OnFetchCompleted += OnLFMFetchCompleted;
        Task.Run(() => _fetchJob.RunAction());
        _fetchJob.Run();
    }
    private void OnLFMFetchCompleted(object sender, LowFuelMotorsportAPI e)
    {
        _api = e;
    }

    public sealed override void BeforeStop()
    {
        if (IsPreviewing) return;

        _fetchJob.OnFetchCompleted -= OnLFMFetchCompleted;
        _fetchJob.Cancel();
    }

    public override void Render(Graphics g)
    {
        string licenseText = GenerateLFMLicense(_api);
        SizeF bounds = g.MeasureString(licenseText, _fontFamily);

        this.Height = (int)(bounds.Height + 1);
        this.Width = (int)(bounds.Width + 1);

        g.TextRenderingHint = TextRenderingHint.AntiAlias;
        g.DrawStringWithShadow(licenseText, _fontFamily, Brushes.White, new PointF(0, 0));
    }

    public override bool ShouldRender()
    {
        if (_config.Connection.User.Trim() == "")
        {
            return false;
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

    private string GenerateLFMLicense(LowFuelMotorsportAPI api)
    {
        if (api == null) return "No data";

        string licenseText = string.Format
        (
            "{0} {1}    {2} ({3}) ({4})    ELO {5}    {6} ({7})",
            api.UserLicense.FirstName,
            api.UserLicense.LastName,
            api.UserLicense.License,
            api.UserLicense.SafetyRatingLicense,
            api.UserLicense.SafetyRating,
            api.UserLicense.Elo,
            api.UserLicense.GameName,
            api.UserLicense.GamePlatform
        ).Trim();

        if (api.NextRace != null)
        {
            string raceText = string.Format
            (
                "{0} | #{1} | Split {2} | SOF {3} | Drivers {4}",
                api.NextRace.Name,
                api.NextRace.Id,
                api.NextRace.Split,
                api.NextRace.Sof,
                api.NextRace.NumberOfDrivers
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

            if (api.NextRace.Date.Year != 1)
            {
                string time = TimeSpanToStringCountDown(api.NextRace.Date.Subtract(DateTime.Now));
                licenseText = string.Format("{0}\n{1}", licenseText, time.PadLeft(licenseText.IndexOf('\n'), ' '));
            }
        }

        return licenseText;
    }
}
