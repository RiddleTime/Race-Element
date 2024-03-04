using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace RaceElement.HUD.ACC.Overlays.Pitwall.OverlayLowFuelMotorsport.API;

public record ApiObject(
    [property: JsonProperty("user")] User User,
    [property: JsonProperty("race")] IReadOnlyList<Race> Race,
    [property: JsonProperty("drivers")] int Drivers,
    [property: JsonProperty("sim")] Sim Sim,
    [property: JsonProperty("licenseclass")] string Licenseclass,
    [property: JsonProperty("safetyplate")] string Safetyplate,
    [property: JsonProperty("sof")] int Sof
);

public record Sim(
    [property: JsonProperty("sim_id")] int SimId,
    [property: JsonProperty("select_order")] int SelectOrder,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("logo_url")] string LogoUrl,
    [property: JsonProperty("logo_big")] string LogoBig,
    [property: JsonProperty("platform")] string Platform,
    [property: JsonProperty("active")] int Active
);

public record Race(
    [property: JsonProperty("event_name")] string EventName,
    [property: JsonProperty("race_id")] int RaceId,
    [property: JsonProperty("split")] int Split,
    [property: JsonProperty("sim_id")] int SimId,
    [property: JsonProperty("race_date")] DateTime RaceDate,
    [property: JsonProperty("sof")] int Sof,
    [property: JsonProperty("split2_sof")] int Split2Sof,
    [property: JsonProperty("split3_sof")] int Split3Sof,
    [property: JsonProperty("split4_sof")] int Split4Sof,
    [property: JsonProperty("split5_sof")] int Split5Sof,
    [property: JsonProperty("split6_sof")] int Split6Sof,
    [property: JsonProperty("split7_sof")] int Split7Sof,
    [property: JsonProperty("split8_sof")] int Split8Sof,
    [property: JsonProperty("split9_sof")] int Split9Sof,
    [property: JsonProperty("split10_sof")] int Split10Sof
);

public record User(
    [property: JsonProperty("id")] int Id,
    [property: JsonProperty("name")] string Name,
    [property: JsonProperty("username")] string UserName,
    [property: JsonProperty("vorname")] string FirstName,
    [property: JsonProperty("shortname")] string Shortname,
    [property: JsonProperty("nachname")] string LastName,
    [property: JsonProperty("steamid")] string SteamId,
    [property: JsonProperty("avatar")] string Avatar,
    [property: JsonProperty("email")] string Email,
    [property: JsonProperty("email_verified_at")] object EmailVerifiedAt,
    [property: JsonProperty("password")] string Password,
    [property: JsonProperty("remember_token")] string RememberToken,
    [property: JsonProperty("admin")] int Admin,
    [property: JsonProperty("created_at")] string CreatedAt,
    [property: JsonProperty("updated_at")] string UpdatedAt,
    [property: JsonProperty("discord_id")] string DiscordId,
    [property: JsonProperty("profile_extras")] string ProfileExtras,
    [property: JsonProperty("origin")] string Origin,
    [property: JsonProperty("c_rating")] int CRating,
    [property: JsonProperty("cc_rating")] int CcRating,
    [property: JsonProperty("twitch_channel")] string TwitchChannel,
    [property: JsonProperty("is_tv_broadcaster")] int IsTvBroadcaster,
    [property: JsonProperty("youtube_channel")] string YoutubeChannel,
    [property: JsonProperty("popometer_id")] string PopometerId,
    [property: JsonProperty("license")] string License,
    [property: JsonProperty("safety_rating")] string SafetyRating,
    [property: JsonProperty("division")] int Division,
    [property: JsonProperty("valid_license")] int ValidLicense,
    [property: JsonProperty("darkmode")] int Darkmode,
    [property: JsonProperty("patreon")] int Patreon,
    [property: JsonProperty("fav_sim")] int FavSim,
    [property: JsonProperty("sr_license")] string SrLicense
);
