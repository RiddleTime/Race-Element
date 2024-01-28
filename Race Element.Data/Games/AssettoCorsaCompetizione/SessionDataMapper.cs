using RaceElement.Data.Common.SimulatorData;
using Riok.Mapperly.Abstractions;
using static RaceElement.Data.Games.AssettoCorsaCompetizione.SharedMemory.AccSharedMemory;

namespace RaceElement.Data.Games.AssettoCorsaCompetizione
{
    [Mapper]
    public static partial class SessionDataMapper
    {
        // -- Session Conditions
        [MapProperty($"{nameof(PageFilePhysics.AirTemp)}", $"{nameof(SessionData.Weather)}.{nameof(WeatherConditions.AirTemperature)}")]
        [MapProperty($"{nameof(PageFilePhysics.RoadTemp)}", $"{nameof(SessionData.Weather)}.{nameof(WeatherConditions.TrackTemperature)}")]
        public static partial void WithAccPhysicsPage(PageFilePhysics pagePhysics, SessionData sessionData);

        [MapProperty($"{nameof(PageFileGraphics.WindSpeed)}", $"{nameof(SessionData.Weather)}.{nameof(WeatherConditions.AirVelocity)}")]
        [MapProperty($"{nameof(PageFileGraphics.WindDirection)}", $"{nameof(SessionData.Weather)}.{nameof(WeatherConditions.AirDirection)}")]
        public static partial void WithAccGraphicsPage(PageFileGraphics pageGraphics, SessionData sessionData);
    }
}
