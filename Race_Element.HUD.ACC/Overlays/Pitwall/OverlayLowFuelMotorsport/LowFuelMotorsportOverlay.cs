using RaceElement.HUD.Overlay.OverlayUtil;
using RaceElement.HUD.Overlay.Internal;
using System.Drawing.Text;
using System.Drawing;
using System;
using System.Windows.Forms;
using static RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport.LowFuelMotorsportConfiguration;
using RaceElement.HUD.Overlay.Util;
using RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport.API;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

[Overlay(Name = "Low Fuel Motorsport",
    Description = "Shows driver license and next upcoming races.",
    OverlayType = OverlayType.Drive,
    Authors = ["Andrei Jianu"]
)]

internal sealed class LowFuelMotorsportOverlay : AbstractOverlay
{
    private readonly LowFuelMotorsportConfiguration _config = new();

    private Font _fontFamily;

    private ApiObject _apiObject;
    private LowFuelMotorsportJob _lfmJob;

    private SizeF _previousTextBounds = Size.Empty;

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
            FontFamilyConfig.SegoeMono => FontUtil.FontSegoeMono(_config.Font.Size),
            FontFamilyConfig.Conthrax => FontUtil.FontConthrax(_config.Font.Size),
            FontFamilyConfig.Orbitron => FontUtil.FontOrbitron(_config.Font.Size),
            FontFamilyConfig.Roboto => FontUtil.FontRoboto(_config.Font.Size),
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

        _lfmJob.OnNewApiObject += OnNewApiObject;

        _lfmJob.Run();
    }

    private void OnNewApiObject(object sender, ApiObject apiObject) => _apiObject = apiObject;

    public sealed override void BeforeStop()
    {
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
        string licenseText = GenerateLFMLicense();

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

    private string GenerateLFMLicense()
    {
        if (_apiObject.User.UserName == null && !IsPreviewing) return "No data";

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
                string time = TimeSpanToStringCountDown(race.RaceDate.Subtract(DateTime.Now));
                licenseText = string.Format("{0}\n{1}", licenseText, time.PadLeft(licenseText.IndexOf('\n'), ' '));
            }

        }

        return licenseText;
    }
}
