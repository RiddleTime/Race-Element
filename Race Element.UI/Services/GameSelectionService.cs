// Create a new file: Race Element.UI/Services/GameSelectionService.cs
using ReactiveUI;
using System;
using System.Diagnostics;

namespace RaceElement.UI.Services;

public enum SupportedGame
{
    IRacing,
    ACC,
    ACEvo,
    LMU
}

/// <summary>
/// Provides application-wide access to the currently selected game
/// </summary>
public class GameSelectionService : ReactiveObject
{
    #region Singleton
    private static readonly Lazy<GameSelectionService> _instance =
        new Lazy<GameSelectionService>(() => new GameSelectionService());

    public static GameSelectionService Instance => _instance.Value;
    #endregion

    #region Events
    public event Action<string> GameChanged;
    #endregion

    #region Properties
    private string _selectedGame = "acc"; // Default game

    public string SelectedGame => _selectedGame;
    #endregion

    #region Constructor
    private GameSelectionService()
    {
        // Private constructor for singleton
    }
    #endregion

    #region Public Methods

    /// <summary>
    /// Updates the selected game and performs any game-specific initialization logic
    /// </summary>
    /// <param name="game">The game identifier</param>
    /// <returns>True if the selection was valid and processed, false otherwise</returns>
    public bool SelectGame(string game)
    {
        if (game == _selectedGame)
            return false;

        _selectedGame = game;
        GameChanged?.Invoke(game);
        return true;
    }

    /// <summary>
    /// Updates the selected game (internal - only used by MainTopMenuViewModel)
    /// </summary>
    public void UpdateSelectedGame(string game)
    {
        _selectedGame = game;
    }

    // Helper methods to check for specific games
    public bool IsIRacing => SelectedGame == "iracing";
    public bool IsACC => SelectedGame == "acc";
    public bool IsACEvo => SelectedGame == "acevo";
    public bool IsLMU => SelectedGame == "lmu";
    #endregion



    //internal bool SelectGame(string selection)
    //{
    //    string game = selection.ToLower();

    //    // Skip if same game selected
    //    if (_selectedGame == game) return false;

    //    // Update the selected game
    //    this.RaiseAndSetIfChanged(ref _selectedGame, game);

    //    // Perform game-specific initialization
    //    InitializeGameSpecificSettings(game);

    //    return true;
    //}

    /// <summary>
    /// Initializes game-specific settings and configurations
    /// </summary>
    private void InitializeGameSpecificSettings(string game)
    {
        switch (game)
        {
            case "iracing":
                // iRacing-specific initialization
                InitializeIRacingSettings();
                break;

            case "acc":
                // ACC-specific initialization
                InitializeACCSettings();
                break;

            case "acevo":
                // AC Evo-specific initialization
                InitializeACEvoSettings();
                break;

            case "lmu":
                // LMU-specific initialization
                InitializeLMUSettings();
                break;
        }
    }

    private void InitializeIRacingSettings()
    {
    }

    private void InitializeACCSettings()
    {
    }

    private void InitializeACEvoSettings()
    {
    }

    private void InitializeLMUSettings()
    {
    }
}
