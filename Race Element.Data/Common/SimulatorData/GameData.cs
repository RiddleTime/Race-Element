
namespace RaceElement.Data.Common.SimulatorData
{
    public sealed record GameData
    {
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;

        public bool IsGamePaused { get; internal set; } = false;
        public bool IsInReplay { get; internal set; } = false;

    }
}
