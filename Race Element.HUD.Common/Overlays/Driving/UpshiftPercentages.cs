using RaceElement.Data.Common;

namespace RaceElement.HUD.Common.Overlays.Driving;

internal static class UpshiftPercentages
{
    /// <summary>
    /// Resolves the early and redline thresholds as a percentage of
    /// <see cref="RaceElement.Data.Common.SimulatorData.LocalCar.EngineData.MaxRpm"/>,
    /// which is what the shift overlays compare the current rpm against.
    /// </summary>
    /// <param name="configuredEarly">Configured early percentage, 0 to 100.</param>
    /// <param name="configuredRedline">Configured redline percentage, 0 to 100.</param>
    /// <param name="revLimiterOverride">User supplied rev limiter in rpm, 0 to use the simulator data.</param>
    public static (float early, float redline) Calculate(float configuredEarly, float configuredRedline, int revLimiterOverride = 0)
    {
        float maxRpm = SimDataProvider.LocalCar.Engine.MaxRpm;
        if (maxRpm <= 0)
            return (configuredEarly, configuredRedline);

        // A simulator that reports its own shift point overrides the configured percentages.
        float shiftUpRpm = SimDataProvider.LocalCar.Engine.ShiftUpRpm;
        if (shiftUpRpm > 0)
        {
            float redline = shiftUpRpm * 100f / maxRpm;
            return (redline * 0.96f, redline);
        }

        // Otherwise the percentages apply to the rev limiter. In Forza MaxRpm is the end of
        // the tachometer scale, so percentages of MaxRpm are never reached by the engine.
        float limiterRpm = revLimiterOverride > 0
            ? revLimiterOverride
            : SimDataProvider.LocalCar.Engine.RevLimiterRpm;

        if (limiterRpm <= 0 || limiterRpm >= maxRpm)
            return (configuredEarly, configuredRedline);

        float limiterScale = limiterRpm / maxRpm;
        return (configuredEarly * limiterScale, configuredRedline * limiterScale);
    }
}
