using RaceElement.Data.Games;
using System.Windows;

namespace RaceElement.Util.Settings;

public class UiSettingsJson : IGenericSettingsJson
{
    public int SelectedTabIndex;
    public int X;
    public int Y;
    public Game SelectedGame;
}

public class UiSettings : AbstractSettingsJson<UiSettingsJson>
{
    public override string Path => FileUtil.RaceElementSettingsPath;

    public override string FileName => "UI.json";

    public override UiSettingsJson Default()
    {
        var settings = new UiSettingsJson()
        {
            SelectedTabIndex = 0,
            X = (int)SystemParameters.PrimaryScreenWidth / 2,
            Y = (int)SystemParameters.PrimaryScreenHeight / 2,
            SelectedGame = Game.AssettoCorsaCompetizione,
        };

        return settings;
    }
}
