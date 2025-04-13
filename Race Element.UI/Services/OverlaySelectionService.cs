using Material.Icons;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.UI.Services;

/// <summary>
/// Defines all available overlay types in the application
/// </summary>
public enum OverlayType
{
    [Display(Name = "Inputs")]
    Inputs = 0,

    [Display(Name = "Standings")]
    Standings = 1,

    [Display(Name = "Relatives")]
    Relatives = 2,

    [Display(Name = "Laptime")]
    Laptimes = 3,

    [Display(Name = "Fuel")]
    Fuel = 4,

    [Display(Name = "Track Map")]
    TrackMap = 5,

    [Display(Name = "Spotter")]
    Spotter = 6,

    [Display(Name = "Accelerometer")]
    Accelerometer = 7,

    [Display(Name = "Average Laptime")]
    AverageLaptime = 8,

    [Display(Name = "Boost Gauge")]
    BoostGauge = 9,

    [Display(Name = "Brake Pressure")]
    BrakePressure = 10
}
public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var displayAttribute = enumValue.GetType()
            .GetField(enumValue.ToString())
            .GetCustomAttribute<DisplayAttribute>();

        return displayAttribute?.Name ?? enumValue.ToString();
    }
}

/// <summary>
/// Service for managing overlay availability and selection across different games
/// </summary>
public class OverlaySelectionService
{
    #region Singleton
    private static readonly Lazy<OverlaySelectionService> _instance =
        new Lazy<OverlaySelectionService>(() => new OverlaySelectionService());

    public static OverlaySelectionService Instance => _instance.Value;
    #endregion

    #region Game-Specific Overlay Availability
    // Define which overlays are available for each game
    private readonly Dictionary<SupportedGame, HashSet<OverlayType>> _gameOverlayAvailability;
    #endregion

    #region Constructor
    private OverlaySelectionService()
    {
        // Initialize availability mappings
        _gameOverlayAvailability = new Dictionary<SupportedGame, HashSet<OverlayType>>
        {
            // iRacing supports all overlays
            [SupportedGame.IRacing] = new HashSet<OverlayType>
            {
                OverlayType.Inputs,
                OverlayType.Standings,
                OverlayType.Relatives,
                OverlayType.Laptimes,
                OverlayType.Fuel,
                OverlayType.TrackMap,
                OverlayType.Spotter,
                OverlayType.Accelerometer,
                OverlayType.AverageLaptime,
                OverlayType.BoostGauge,
                OverlayType.BrakePressure
            },

            // ACC doesn't support BoostGauge and BrakePressure
            [SupportedGame.ACC] = new HashSet<OverlayType>
            {
                OverlayType.Inputs,
                OverlayType.Standings,
                OverlayType.Relatives,
                OverlayType.Laptimes,
                OverlayType.Fuel,
                OverlayType.TrackMap,
                OverlayType.Accelerometer,
                OverlayType.AverageLaptime
            },

            // AC Evo doesn't support Spotter
            [SupportedGame.ACEvo] = new HashSet<OverlayType>
            {
                OverlayType.Inputs,
                OverlayType.Standings,
                OverlayType.Relatives,
                OverlayType.Laptimes,
                OverlayType.Fuel,
                OverlayType.TrackMap,
                OverlayType.Accelerometer,
                OverlayType.AverageLaptime,
                OverlayType.BoostGauge,
                OverlayType.BrakePressure
            },

            // LMU has limited overlay support
            [SupportedGame.LMU] = new HashSet<OverlayType>
            {
                OverlayType.Inputs,
                OverlayType.Standings,
                OverlayType.Laptimes,
                OverlayType.Fuel,
                OverlayType.Accelerometer
            }
        };
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Checks if an overlay is available for the specified game
    /// </summary>
    /// <param name="overlayType">The overlay type to check</param>
    /// <param name="game">The game identifier</param>
    /// <returns>True if the overlay is available, false otherwise</returns>
    public bool IsOverlayAvailableForGame(OverlayType overlayType, SupportedGame game)
    {
        if (_gameOverlayAvailability.TryGetValue(game, out var availableItems))
        {
            return availableItems.Contains(overlayType);
        }
        return false;
    }

    /// <summary>
    /// Gets all available overlay types for a specific game
    /// </summary>
    /// <param name="game">The game identifier</param>
    /// <returns>An enumerable of available overlay types</returns>
    public IEnumerable<OverlayType> GetAvailableOverlaysForGame(SupportedGame game)
    {
        if (_gameOverlayAvailability.TryGetValue(game, out var availableItems))
        {
            return availableItems;
        }
        return Enumerable.Empty<OverlayType>();
    }

    /// <summary>
    /// Finds the first available overlay for the specified game
    /// </summary>
    /// <param name="game">The game identifier</param>
    /// <returns>The first available overlay type</returns>
    public OverlayType FindFirstAvailableOverlay(SupportedGame game)
    {
        return GetAvailableOverlaysForGame(game).FirstOrDefault();
    }

    /// <summary>
    /// Gets the Material Icon kind for a specific overlay type
    /// </summary>
    /// <param name="overlayType"></param>
    /// <returns></returns>
    public MaterialIconKind GetIconForOverlayType(OverlayType overlayType)
    {
        return overlayType switch
        {
            OverlayType.Inputs => MaterialIconKind.GraphBar,
            OverlayType.Standings => MaterialIconKind.ListBox,
            OverlayType.Relatives => MaterialIconKind.TimelineClock,
            OverlayType.Laptimes => MaterialIconKind.Timelapse,
            OverlayType.Fuel => MaterialIconKind.Fuel,
            OverlayType.TrackMap => MaterialIconKind.Map,
            OverlayType.Spotter => MaterialIconKind.ViewCarousel,
            OverlayType.Accelerometer => MaterialIconKind.Speedometer,
            OverlayType.AverageLaptime => MaterialIconKind.TimeOfDay,
            OverlayType.BoostGauge => MaterialIconKind.PowerMeter,
            OverlayType.BrakePressure => MaterialIconKind.StopPause,
            _ => MaterialIconKind.QuestionMark
        };
    }

    /// <summary>
    /// Gets menu name for the overlay type
    /// </summary>
    /// <param name="overlayType"></param>
    /// <returns></returns>
    public string GetMenuNameForOverlayType(OverlayType overlayType)
    {
        return overlayType.GetDisplayName();
    }
    #endregion
}
