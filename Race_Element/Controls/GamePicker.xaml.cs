using RaceElement.Data.Games;
using RaceElement.Util.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
            Game currentGame = item.Game;
            GameManager.SetCurrentGame(currentGame);

            var uiSettings = new UiSettings();
            var settings = uiSettings.Get();

            settings.SelectedGame = currentGame;
            uiSettings.Save(settings);

            SetToolTip(currentGame);
        };
        GameManager.OnAutoGameRequest += GameManager_OnAutoGameRequest;

        ToolTipService.SetInitialShowDelay(this, 0);
        ToolTipService.SetPlacement(this, System.Windows.Controls.Primitives.PlacementMode.Right);
    }

    private void GameManager_OnAutoGameRequest(object sender, Game requested)
    {
        comboGamePicker.Dispatcher.BeginInvoke(() =>
        {
            var selected = comboGamePicker.SelectedItem as GamePickerModel;
            if (selected.Game != requested)
            {
                var list = comboGamePicker.ItemsSource.Cast<GamePickerModel>();
                var nextItem = list.FirstOrDefault(x => x.Game == requested);
                comboGamePicker.SelectedItem = nextItem;
                MainWindow.Instance.EnqueueSnackbarMessage($"Automatically switched to {nextItem.FriendlyName}");
            }
        });
    }

    private void SetToolTip(Game game)
    {
        this.ToolTip = $"{game.ToFriendlyName()}\n\nClick here select a different game. This might take a few seconds.";
    }

    private void GamePicker_Loaded(object sender, RoutedEventArgs e)
    {
        comboGamePicker.ItemsSource = null;

        List<GamePickerModel> availableGames = new();

        foreach (Game game in Enum.GetValues(typeof(Game)))
        {
            if (game == Game.Any) continue;

            var logStream = game.GetSteamLogo();
            BitmapImage logo = new();
            if (logStream != null)
            {
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
        Game selectedGame = Game.Any;//  GameExtensions.GetRunningGame();

        if (selectedGame == Game.Any)
        {
            selectedGame = uiSettings.Get().SelectedGame; ;
        }

        var model = availableGames.FirstOrDefault(x => x.Game == selectedGame);
        model ??= availableGames.FirstOrDefault(x => x.Game == Game.AssettoCorsaCompetizione);

        SetToolTip(model.Game);
        comboGamePicker.SelectedItem = model;
    }

    private void ComboGamePickerPreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (!comboGamePicker.IsDropDownOpen)
            return;

        // Get the ScrollViewer from the dropdown popup
        var scrollViewer = GetDropDownScrollViewer();
        if (scrollViewer == null)
            return;

        if (e.Delta > 0)
            scrollViewer.LineUp();
        else
            scrollViewer.LineDown();

        e.Handled = true;
    }

    // Improved method specifically for ComboBox dropdown
    private ScrollViewer GetDropDownScrollViewer()
    {
        if (comboGamePicker == null || !comboGamePicker.IsDropDownOpen)
            return null;

        // Try to find Popup first
        var popup = FindVisualChild<Popup>(comboGamePicker);
        if (popup?.Child != null)
        {
            return FindVisualChild<ScrollViewer>(popup.Child);
        }

        // Fallback
        return FindVisualChild<ScrollViewer>(comboGamePicker);
    }

    private static T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) return null;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);

            if (child is T t)
                return t;

            var found = FindVisualChild<T>(child);
            if (found != null)
                return found;
        }
        return null;
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
