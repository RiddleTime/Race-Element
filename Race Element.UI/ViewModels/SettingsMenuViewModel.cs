using Avalonia.Controls;
using Avalonia.Themes.Neumorphism.Dialogs;
using Avalonia.Themes.Neumorphism.Dialogs.Interfaces;
using Avalonia.Themes.Neumorphism.Dialogs.ViewModels;
using Avalonia.Themes.Neumorphism.Dialogs.ViewModels.Elements;
using ReactiveUI;

namespace RaceElement.UI.ViewModels;

/// <summary>
/// View model for the settings menu dialog it should derive from DialogWindowViewModel
/// to block mainwindow interaction during dialog open
/// </summary>
public class SettingsMenuViewModel : DialogWindowViewModel
{
    #region Private Fields

    /// <summary>
    /// Reference to the dialog window
    /// </summary>
    private readonly Window _window;

    /// <summary>
    /// OK button view model
    /// </summary>
    private ResultBasedDialogButtonViewModel? _buttonOk;

    /// <summary>
    /// Cancel button view model
    /// </summary>
    private ResultBasedDialogButtonViewModel? _buttonCancel;
    #endregion

    #region Public Properties

    /// <summary>
    /// Gets or sets the OK button view model
    /// </summary>
    public ResultBasedDialogButtonViewModel ButtonOk
    {
        get { return _buttonOk; }
        set
        {
            _buttonOk = value;
            OnPropertyChanged(nameof(ButtonOk));
        }
    }

    /// <summary>
    /// Gets or sets the Cancel button view model
    /// </summary>
    public ResultBasedDialogButtonViewModel ButtonCancel
    {
        get { return _buttonCancel; }
        set
        {
            _buttonCancel = value;
            OnPropertyChanged(nameof(ButtonCancel));
        }
    }
    #endregion

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsMenuViewModel"/> class
    /// </summary>
    /// <param name="window">The dialog window</param>
    public SettingsMenuViewModel(Window window) : base(window)
    {
        _window = window;

        // Create ReactiveCommands that accept ResultBasedDialogButtonViewModel
        var saveCommand = ReactiveCommand.Create<ResultBasedDialogButtonViewModel>(_ =>
        {
            // Save settings here
            HandleSave();
        });

        var cancelCommand = ReactiveCommand.Create<ResultBasedDialogButtonViewModel>(_ =>
        {
            // Maybe warn for changes will not be saved
            HandleCancel();
        });

        // Initialize the OK button
        ButtonOk = new ResultBasedDialogButtonViewModel(this, "OK", "ok")
        {
            Content = "OK",
            Command = saveCommand
        };

        // Initialize the Cancel button
        ButtonCancel = new ResultBasedDialogButtonViewModel(this, "Cancel", "cancel")
        {
            Content = "Cancel",
            Command = cancelCommand
        };
    }
    #endregion

    #region Private Methods

    /// <summary>
    /// Persist all settings
    /// </summary>
    private void HandleSave()
    {
        // Set dialog result
        DialogResult = new DialogResult("Ok");
        
        // Close the window directly
        _window.Close();
    }

    /// <summary>
    /// Handles the cancel command execution
    /// </summary>
    private void HandleCancel()
    {
        // Set dialog result
        DialogResult = new DialogResult("Cancel");

        // Close the window directly
        _window.Close();
    }
    #endregion
}