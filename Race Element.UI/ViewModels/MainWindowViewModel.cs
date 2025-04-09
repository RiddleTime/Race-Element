using ReactiveUI;
using System.Reactive;
using Avalonia.Utilities;

namespace RaceElement.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        // Initialize the current page to the overlays page
        _currentPage = new OverlaysViewModel();

        /// @todo Instead we can use Dependency injection but it has a complex setup
        ///       Can not simply pass this as a parameter to constructor, can be investigated later
        MainTopMenu = new MainTopMenuViewModel();
        MainTopMenu.SetMainWindowViewModel(this);
    }

    public void NavigateTo(ViewModelBase page)
    {
        // Block consequent navigation to the same page
        if (CurrentPage == page) return;

        // Set the current page to the specified page
        CurrentPage = page;
    }

    public MainTopMenuViewModel MainTopMenu { get; set; }

    // Default to overlays page
    private ViewModelBase _currentPage = new OverlaysViewModel();
    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }
}
