using RaceElement.Data.Games;
using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
using System.IO;
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
            SetToolTip(currentGame);
        };
        ToolTipService.SetInitialShowDelay(this, 0);
        ToolTipService.SetPlacement(this, System.Windows.Controls.Primitives.PlacementMode.Right);
    }

    private void SetToolTip(Game game)
    {
        this.ToolTip = $"Game: {game.ToFriendlyName()}\n\nClick here select a different game. This might take a few seconds.";
    }

    private void GamePicker_Loaded(object sender, RoutedEventArgs e)
    {
        comboGamePicker.Items.Clear();

        List<GamePickerModel> availableGames = new();

        foreach (Game game in Enum.GetValues(typeof(Game)))
        {
            if (game == Game.Any) continue;

            var logStream = game.GetSteamLogo();
            BitmapImage logo = new();
            if (logStream != null)
            {
                var bitmap = new BitmapImage();
                logo.BeginInit();
                logo.StreamSource = logStream;
                logo.CacheOption = BitmapCacheOption.OnLoad;
                logo.EndInit();
                logo.Freeze();

                logStream.Close();
                logStream.Dispose();
            }

            var iconStream = game.GetGameClientIcon();
            BitmapImage icon = new();
            if (iconStream != null)
            {
                var bitmap = new BitmapImage();
                icon.BeginInit();
                icon.StreamSource = iconStream;
                icon.CacheOption = BitmapCacheOption.OnLoad;
                icon.EndInit();
                icon.Freeze();

                iconStream.Close();
                iconStream.Dispose();
            }

            availableGames.Add(new GamePickerModel()
            {
                Name = game.ToShortName(),
                FriendlyName = game.ToFriendlyName(),
                Game = game,
                Logo = logo,
                Icon = icon,
            });
        }
        availableGames.Sort((a, b) => a.FriendlyName.CompareTo(b.FriendlyName));

        comboGamePicker.ItemsSource = availableGames;

        var uiSettings = new UiSettings();
        Game selectedGame = uiSettings.Get().SelectedGame;
        var all = Resources.Values;
        var model = availableGames.FirstOrDefault(x => x.Game == selectedGame);
        model ??= availableGames.FirstOrDefault(x => x.Game == Game.AssettoCorsaCompetizione);
        SetToolTip(model.Game);
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
