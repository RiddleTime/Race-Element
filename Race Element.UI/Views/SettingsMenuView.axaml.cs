using Avalonia;
using Avalonia.Controls;
using Avalonia.Themes.Neumorphism.Dialogs.Interfaces;
using Avalonia.Themes.Neumorphism.Dialogs;
using RaceElement.UI.ViewModels;
using Avalonia.Markup.Xaml;

namespace RaceElement.UI.Views;

public partial class SettingsMenuView : Window, IDialogWindowResult<DialogResult>, IHasNegativeResult
{

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsMenuView"/> class.
    /// </summary>
    public SettingsMenuView()
    {
        AvaloniaXamlLoader.Load(this);
        WindowStartupLocation = WindowStartupLocation.CenterOwner;
        SystemDecorations = SystemDecorations.None;
        ExtendClientAreaToDecorationsHint = true;
    }

    /// <summary>
    /// Gets the result of the dialog.
    /// </summary>
    /// <returns></returns>
    public DialogResult GetResult() => (DataContext as SettingsMenuViewModel)?.DialogResult;

    /// <summary>
    /// Ensures that even if the user closes the dialog without explicitly clicking one of the buttons, 
    /// dialog returns a meaningful result.
    /// </summary>
    /// <param name="result"></param>
    public void SetNegativeResult(DialogResult result)
    {
        if (DataContext is SettingsMenuViewModel viewModel)
            viewModel.DialogResult = result;
    }
}
