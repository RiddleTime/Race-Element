using RaceElement.Core.Jobs.LoopJob;

using Newtonsoft.Json.Linq;

using System.Net.Http;
using System;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport;

internal sealed class LowFuelMotorsportUserLicense
{
    public string FirstName;
    public string LastName;

    public string License;
    public string Elo;

    public string SafetyRatingLicense;
    public string SafetyRating;

    public string GameName;
    public string GamePlatform;
}

internal sealed class LowFuelMotorsportNextRace
{
    public string Name;
    public string Id;

    public DateTime Date;
    public string Split;

    public string NumberOfDrivers;
    public string Sof;
}

internal sealed class LowFuelMotorsportAPI
{
    public LowFuelMotorsportUserLicense UserLicense;
    public LowFuelMotorsportNextRace NextRace;
}

internal sealed class LowFuelMotorsportJob : AbstractLoopJob
{
    public static readonly string DRIVER_RACE_API_URL = "https://api2.lowfuelmotorsport.com/api/licenseWidgetUserData/{0}";
    private string _userId = null;

    public EventHandler<LowFuelMotorsportAPI> OnFetchCompleted;

    public LowFuelMotorsportJob(string userId)
    {
        _userId = userId;
    }

    private void JsonToLFMObject(JObject lfmObject)
    {
        LowFuelMotorsportUserLicense userLicense = new();
        LowFuelMotorsportNextRace nextRace = null;

        userLicense.FirstName = lfmObject["user"]["vorname"].Value<string>().Trim();
        userLicense.LastName = lfmObject["user"]["nachname"].Value<string>().Trim();

        userLicense.License = lfmObject["user"]["license"].Value<string>().Trim();
        userLicense.Elo = lfmObject["user"]["cc_rating"].Value<string>().Trim();

        userLicense.SafetyRatingLicense = lfmObject["user"]["sr_license"].Value<string>().Trim();
        userLicense.SafetyRating = lfmObject["user"]["safety_rating"].Value<string>().Trim();

        if (lfmObject["sim"] != null)
        {
            userLicense.GameName = lfmObject["sim"]["name"].Value<string>().Trim();
            userLicense.GamePlatform = lfmObject["sim"]["platform"].Value<string>().Trim();
        }

        if (((JArray)lfmObject["race"]).Count > 0)
        {
            nextRace = new()
            {
                Name = lfmObject["race"][0]["event_name"].Value<string>().Trim(),
                Split = lfmObject["race"][0]["split"].Value<string>().Trim(),

                Date = DateTime.Parse(lfmObject["race"][0]["race_date"].Value<string>().Trim()),
                Id = lfmObject["race"][0]["race_id"].Value<string>().Trim(),

                NumberOfDrivers = lfmObject["drivers"].Value<string>().Trim(),
                Sof = lfmObject["sof"].Value<string>().Trim()
            };
        }

        LowFuelMotorsportAPI api = new()
        {
            UserLicense = userLicense,
            NextRace = nextRace
        };

        OnFetchCompleted?.Invoke(this, api);
    }

    public override void RunAction()
    {
        HttpClient client = new();
        string url = string.Format(DRIVER_RACE_API_URL, _userId);

        using HttpResponseMessage response = client.GetAsync(url).Result;
        using HttpContent content = response.Content;

        string json = content.ReadAsStringAsync().Result;
        JsonToLFMObject(JObject.Parse(json));
    }
}
