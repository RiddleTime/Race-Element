using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform;

namespace RaceElement.UI.Views.Stylers;

/// <summary>
/// Sets the default size of the window.
/// </summary>
/// <param name="width"></param>
/// <param name="height"></param>
public readonly struct WindowSize(int width, int height)
{
  public readonly double DefaultWidth { get; } = width;
  public readonly double DefaultHeight { get; } = height;
}

internal class MainWindowStyler
{
  public static void Decorate(Window window)
  {
    _mainWindow = window;

    // Initialize a WindowSize struct with 1200 and 768 values
    _windowSize = new WindowSize(1200, 768);

    // Assign the values to the mainWindow attributes
    _mainWindow.Width = _windowSize.DefaultWidth;
    _mainWindow.Height = _windowSize.DefaultHeight;
    _mainWindow.MinWidth = _windowSize.DefaultWidth;
    _mainWindow.MinHeight = _windowSize.DefaultHeight;

    // Window configuration
    _mainWindow.Background = Brushes.Transparent;
    _mainWindow.ExtendClientAreaToDecorationsHint = true;
    _mainWindow.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
    _mainWindow.ExtendClientAreaTitleBarHeightHint = -1;
    _mainWindow.SystemDecorations = SystemDecorations.None;
    _mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

    // To make mainwindow draggable
    _mainWindow.Loaded += MainWindow_Loaded;
  }

  private static void MainWindow_Loaded(object? sender, RoutedEventArgs e)
  {
    var TopMenuBar = _mainWindow.FindControl<Control>("TopMenuBar");
    // Access TopMenuBar directly by its generated field x:Name
    if (TopMenuBar != null)
    {
      TopMenuBar.PointerPressed += (s, args) =>
      {
        _isDragging = true;
        _lastPosition = args.GetPosition(_mainWindow);
        _mainWindow.PointerReleased += PointerReleasedHandler;
        _mainWindow.PointerMoved += PointerMovedHandler;
      };
    }
  }

  private static void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
  {
    _isDragging = false;
    _mainWindow.PointerReleased -= PointerReleasedHandler;
    _mainWindow.PointerMoved -= PointerMovedHandler;
  }

  private static void PointerMovedHandler(object? sender, PointerEventArgs e)
  {
    if (_isDragging)
    {
      var currentPoint = e.GetPosition(_mainWindow);
      var offset = currentPoint - _lastPosition;

      // Fix: Use _mainWindow.Position to update the window's position
      _mainWindow.Position = new PixelPoint(
        _mainWindow.Position.X + (int)offset.X,
        _mainWindow.Position.Y + (int)offset.Y
      );
    }
  }

  /// ------------------------------------------------------------------------------
  /// Private Fields 
  /// ------------------------------------------------------------------------------

  // Fields to track the dragging state and last position of the pointer
  private static bool _isDragging = false;
  private static Point _lastPosition;

  // WindowSize struct to hold the default window size
  private static WindowSize _windowSize;

  // Reference to the main window
  private static Window _mainWindow;
}
