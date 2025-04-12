using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Reactive;
using System.Diagnostics;
using RaceElement.UI.Services;

namespace RaceElement.UI.ViewModels;
public class MainTopMenuViewModel : ViewModelBase
{
    #region PRIVATE FIELDS

    // Reference to the main window view model for Navigation, DI can be used to access
    private MainWindowViewModel? _mainWindowViewModel;
    // Default page
    private string _currentPageName = "Overlays";
    // Track the current game icon source 
    private string _currentGameIconSource;
    #endregion

    #region CONSTRUCTORS
    public MainTopMenuViewModel()
    {
        // Initialize navigation commands
        NavigateToOverlaysPageCommand = ReactiveCommand.Create(() => { _mainWindowViewModel?.NavigateTo(new OverlaysViewModel()); CurrentPageName = "Overlays"; });
        NavigateToDataPageCommand = ReactiveCommand.Create(() => { _mainWindowViewModel?.NavigateTo(new DataViewModel()); CurrentPageName = "Data"; });
        NavigateToSetupsPageCommand = ReactiveCommand.Create(() => { _mainWindowViewModel?.NavigateTo(new SetupsViewModel()); CurrentPageName = "Setups"; });
        NavigateToLiveriesPageCommand = ReactiveCommand.Create(() => { _mainWindowViewModel?.NavigateTo(new LiveriesViewModel()); CurrentPageName = "Liveries"; });
        NavigateToToolsPageCommand = ReactiveCommand.Create(() => { _mainWindowViewModel?.NavigateTo(new ToolsViewModel()); CurrentPageName = "Tools"; });

        // Initialize GameSelectionCommand
        GameSelectionCommand = ReactiveCommand.Create<string>(HandleGameSelection);
    }
    #endregion

    #region PUBIC PROPERTIES
    public string CurrentPageName
    {
        get => _currentPageName;
        private set => this.RaiseAndSetIfChanged(ref _currentPageName, value);
    }

    public string SelectedGame
    {
        get => GameSelectionService.Instance.SelectedGame;
        private set => GameSelectionService.Instance.UpdateSelectedGame(value);
    }

    public string CurrentGameIconSource => $"avares://RaceElement.UI/Assets/{SelectedGame}.ico";
    #endregion

    #region PUBLIC COMMANDS/METHODS
    public bool IsCurrentPage(ViewModelBase page)
    {
        return _mainWindowViewModel?.CurrentPage == page;
    }

    // Navigation commands
    public ReactiveCommand<Unit, Unit> NavigateToOverlaysPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToDataPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToSetupsPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToLiveriesPageCommand { get; }
    public ReactiveCommand<Unit, Unit> NavigateToToolsPageCommand { get; }

    // Command for game selection from dropdown
    public ReactiveCommand<string, Unit> GameSelectionCommand { get; }

    // Command to close the application
    public static ReactiveCommand<Unit, Unit> CloseApplicationCommand => ReactiveCommand.Create(() => { Environment.Exit(0); });

    // Command to minimize the application
    public static ReactiveCommand<Unit, Unit> MinimizeApplicationCommand => ReactiveCommand.Create(() =>
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.MainWindow.WindowState = WindowState.Minimized;
        }
    });
    #endregion

    #region INTERNAL METHODS
    internal void SetMainWindowViewModel(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }
    #endregion

    #region PRIVATE METHODS
    /// <summary>
    /// Handles the game selection from the dropdown menu.
    /// </summary>
    /// <param name="selection"></param>
    private void HandleGameSelection(string selection)
    {
        // Let the service handle game selection and initialization
        if (GameSelectionService.Instance.SelectGame(selection))
        {
            // Update UI only if the game actually changed
            this.RaisePropertyChanged(nameof(CurrentGameIconSource));
        }
    }
    #endregion
}
