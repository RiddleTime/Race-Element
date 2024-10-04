using RaceElement.Core.Jobs.Loop;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Diagnostics;
using RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport.API;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.LowFuelMotorsport;

internal sealed class LowFuelMotorsportJob(string userId) : AbstractLoopJob
{
    public static readonly string DRIVER_RACE_API_URL = "https://api2.lowfuelmotorsport.com/api/licenseWidgetUserData/{0}";
    public EventHandler<ApiObject> OnNewApiObject;

    public override void RunAction()
    {
        if (userId.Trim().Length == 0)
        {
            return;
        }

        try
        {
            using HttpClient client = new();
            string url = string.Format(DRIVER_RACE_API_URL, userId);

            using HttpResponseMessage response = client.GetAsync(url).Result;
            using HttpContent content = response.Content;

            string json = content.ReadAsStringAsync().Result;
            ApiObject root = JObject.Parse(json).ToObject<ApiObject>();
            OnNewApiObject?.Invoke(null, root);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}
