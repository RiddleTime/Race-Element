using RaceElement.Data.Games;
using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaceElement.Controls;
/// <summary>
/// Interaction logic for GamePicker.xaml
/// </summary>
public partial class GamePicker : UserControl
{
    public GamePicker()
    {
        InitializeComponent();
        this.Loaded += GamePicker_Loaded;

        comboGamePicker.SelectionChanged += (s, e) =>
        {
            var item = comboGamePicker.SelectedItem as GamePickerModel;

            Game currentGame = (Game)item.Game;
            GameManager.SetCurrentGame(currentGame);

            var uiSettings = new UiSettings();
            var settings = uiSettings.Get();
            settings.SelectedGame = currentGame;
            uiSettings.Save(settings);
        };
    }

    private void GamePicker_Loaded(object sender, RoutedEventArgs e)
    {
        comboGamePicker.Items.Clear();

        List<GamePickerModel> availableGames = new();

        foreach (Game game in Enum.GetValues(typeof(Game)))
        {
            if (game == Game.Any) continue;

            availableGames.Add(new GamePickerModel()
            {
                Name = game.ToShortName(),
                FriendlyName = game.ToFriendlyName(),
                Game = game,
                Logo = new BitmapImage(new Uri(game.GetSteamLogo())),
                Icon = new BitmapImage(new Uri(game.GetGameClientIcon()))
            });
        }
        availableGames.Sort((a, b) => a.FriendlyName.CompareTo(b.FriendlyName));

        comboGamePicker.ItemsSource = availableGames;

        var uiSettings = new UiSettings();
        Game selectedGame = uiSettings.Get().SelectedGame;

        var model = availableGames.FirstOrDefault(x => x.Game == selectedGame);
        model ??= availableGames.FirstOrDefault(x => x.Game == Game.AssettoCorsaCompetizione);

        comboGamePicker.SelectedItem = model;
    }
}

public record class GamePickerModel
{
    public string Name { get; set; }
    public string FriendlyName { get; set; }
    public ImageSource Logo { get; set; }
    public ImageSource Icon { get; set; }
    public Game Game { get; set; }
}
