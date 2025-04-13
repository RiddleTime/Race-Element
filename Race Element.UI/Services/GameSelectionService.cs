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
    public event Action<string>? GameChanged;
    #endregion

    #region Properties
    private string _selectedGame = "acc"; // Default game

    public string SelectedGame => _selectedGame;
    #endregion

    #region Constructor
    // Private constructor for singleton
    private GameSelectionService()
    {
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
}
