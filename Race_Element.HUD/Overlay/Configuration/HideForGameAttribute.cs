using RaceElement.Data.Games;
using System;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class HideForGameAttribute(Game game) : Attribute
{
    public Game Game { get; init; } = game;
}
