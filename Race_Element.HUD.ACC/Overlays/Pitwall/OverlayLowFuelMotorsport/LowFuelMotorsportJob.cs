using RaceElement.Core.Jobs.LoopJob;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport.API;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

internal sealed class LowFuelMotorsportJob(string userId) : AbstractLoopJob
{
    public static readonly string DRIVER_RACE_API_URL = "https://api2.lowfuelmotorsport.com/api/licenseWidgetUserData/{0}";
    public EventHandler<ApiObject> OnNewApiObject;

    public override void RunAction()
    {
        using HttpClient client = new();
        string url = string.Format(DRIVER_RACE_API_URL, userId);

        using HttpResponseMessage response = client.GetAsync(url).Result;
        using HttpContent content = response.Content;

        string json = content.ReadAsStringAsync().Result;
        ApiObject root = JObject.Parse(json).ToObject<ApiObject>();
        OnNewApiObject?.Invoke(null, root);
    }
}
