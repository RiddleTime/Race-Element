using RaceElement.Data.Games;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace RaceElement.Controls;
/// <summary>
/// Interaction logic for GameSettingsTab.xaml
/// </summary>
public partial class GameSettingsTab : UserControl
{
    private readonly GamePortSettings _gamePortSettings;

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex OnlyNumberRegex();
    public GameSettingsTab()
    {
        InitializeComponent();
        _gamePortSettings = new();

        Loaded += OnLoaded;

        buttonRestartApp.Click += (s, e) =>
        {

            string assemblyStart = AppContext.BaseDirectory + "RaceElement.exe";

            ProcessStartInfo startInfo = new()
            {
                FileName = "cmd",
                Arguments = $"/c start \"RaceElement.exe\" \"{assemblyStart}\"",
                WindowStyle = ProcessWindowStyle.Hidden,
            };
            Process.Start(startInfo);

            Application.Current.Shutdown();
        };
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var settings = _gamePortSettings.Get();
        var availableGames = _gamePortSettings.Default().GamePorts;

        stackPanelSettings.Children.Clear();


        foreach (var game in availableGames)
        {

            Grid grid = new()
            {
                Margin = new Thickness(0, 0, 0, 10),
            };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(2, GridUnitType.Star) });

            TextBlock title = new TextBlock()
            {
                Text = game.Key.ToFriendlyName(),
                Tag = game.Key,
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(title);
            Grid.SetColumn(title, 0);


            TextBox textBox = new()
            {
                Tag = game,
                Margin = new Thickness(10, 0, 0, 0),
                ToolTip = $"Default port for {game.Key.ToFriendlyName()} is: {game.Value}"
            };
            if (settings.GamePorts.TryGetValue(game.Key, out int port))
                textBox.Text = $"{port}";
            else
                textBox.Text = $"{game.Value}";
            ToolTipService.SetInitialShowDelay(textBox, 0);
            ToolTipService.SetPlacement(textBox, System.Windows.Controls.Primitives.PlacementMode.Left);
            ToolTipService.SetHorizontalOffset(textBox, -20);

            textBox.TextChanged += (s, e) =>
            {
                if (s is TextBox tb)
                {
                    tb.Text = OnlyNumberRegex().Replace(tb.Text, "");
                    tb.CaretIndex = tb.Text.Length; // Force cursor to end
                }

                if (int.TryParse(textBox.Text, out int port))
                {
                    settings.GamePorts[game.Key] = port;
                    _gamePortSettings.Save(settings);
                }
            };
            grid.Children.Add(textBox);
            Grid.SetColumn(textBox, 1);

            stackPanelSettings.Children.Add(grid);
        }
    }

}
