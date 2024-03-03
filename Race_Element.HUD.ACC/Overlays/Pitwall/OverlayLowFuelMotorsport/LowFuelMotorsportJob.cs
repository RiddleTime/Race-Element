using RaceElement.Core.Jobs.LoopJob;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport;

internal record struct LowFuelMotorsportUserLicense
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

internal record LowFuelMotorsportNextRace
{
    public string Name;
    public string Id;

    public DateTime Date;
    public string Split;

    public string NumberOfDrivers;
    public string Sof;
}

internal record LowFuelMotorsportAPI(LowFuelMotorsportUserLicense UserLicense, LowFuelMotorsportNextRace NextRace);

internal sealed class LowFuelMotorsportJob(string userId) : AbstractLoopJob
{
    public static readonly string DRIVER_RACE_API_URL = "https://api2.lowfuelmotorsport.com/api/licenseWidgetUserData/{0}";
    public EventHandler<LowFuelMotorsportAPI> OnFetchCompleted;

    private void JsonToLFMObject(JObject lfmObject)
    {
        LowFuelMotorsportUserLicense userLicense = new()
        {

            FirstName = lfmObject["user"]["vorname"].Value<string>().Trim(),
            LastName = lfmObject["user"]["nachname"].Value<string>().Trim(),

            License = lfmObject["user"]["license"].Value<string>().Trim(),
            Elo = lfmObject["user"]["cc_rating"].Value<string>().Trim(),

            SafetyRating = lfmObject["user"]["safety_rating"].Value<string>().Trim()
        };

        if (lfmObject["user"]["sr_license"] != null)
            userLicense.SafetyRatingLicense = lfmObject["user"]["sr_license"].Value<string>().Trim();

        if (lfmObject["sim"] != null)
        {
            userLicense.GameName = lfmObject["sim"]["name"].Value<string>().Trim();
            userLicense.GamePlatform = lfmObject["sim"]["platform"].Value<string>().Trim();
        }

        // TODO deserialize into an Object 

        LowFuelMotorsportNextRace nextRace = null;
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

        LowFuelMotorsportAPI api = new(userLicense, nextRace);
        OnFetchCompleted?.Invoke(this, api);
    }

    public override void RunAction()
    {
        using HttpClient client = new();
        string url = string.Format(DRIVER_RACE_API_URL, userId);

        using HttpResponseMessage response = client.GetAsync(url).Result;
        using HttpContent content = response.Content;

        string json = content.ReadAsStringAsync().Result;
        JsonToLFMObject(JObject.Parse(json));
    }
}
