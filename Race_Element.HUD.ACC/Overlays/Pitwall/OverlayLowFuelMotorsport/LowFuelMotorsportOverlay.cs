using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;

using System.Drawing.Text;
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

internal sealed class LowFuelMotorsportOverlay : AbstractOverlay
{
    private readonly LowFuelMotorsportConfiguration _config = new();
    private Font _fontFamily = null;

    private LowFuelMotorsportJob _fetchJob = null;
    private LowFuelMotorsportAPI _api = null;

    public LowFuelMotorsportOverlay(Rectangle rectangle) : base(rectangle, "Low Fuel Motorsport")
    {
    }

    public override void BeforeStart()
    {
        _fontFamily = Overlay.Util.FontUtil.FontSegoeMono(_config.Font.Size);
        _fetchJob = new LowFuelMotorsportJob(_config.Connection.User) { IntervalMillis = _config.Connection.Interval * 1000 };

        _fetchJob.OnFetchCompleted += OnLFMFetchCompleted;
        _fetchJob.RunAction();
        _fetchJob.Run();
    }

     public sealed override void BeforeStop()
     {
        _fetchJob.CancelJoin();
        _fetchJob.OnFetchCompleted -= OnLFMFetchCompleted;
     }

    public override void Render(Graphics g)
    {
        if (_api == null)
        {
            return;
        }

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

    private void OnLFMFetchCompleted(object sender, LowFuelMotorsportAPI e)
    {
        _api = e;
    }

    private string GenerateLFMLicense(LowFuelMotorsportAPI api)
    {
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
