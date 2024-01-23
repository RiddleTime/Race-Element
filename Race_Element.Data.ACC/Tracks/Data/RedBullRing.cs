﻿using RaceElement.Util.DataTypes;
using System;
using System.Collections.Generic;
using static RaceElement.Data.ACC.Tracks.TrackData;

namespace RaceElement.Data.ACC.Tracks.Data;
internal class RedBullRing : AbstractTrackData
{
    public override Guid Guid => new("f40d09f5-d548-4206-978c-61275840e808");

    // -- TODO, collect the game name for the track itself, verify this (this is based on the pattern that kunos simulazioni has used for the most recent new tracks).
    public override string GameName => "red_bull_ring";

    public override string FullName => "Red Bull Ring";

    public override int TrackLength => 4318;

    // -- TODO, collect the sector transition
    public override List<float> Sectors => [];

    // -- TODO, fill out the start and end the float range structs
    public override Dictionary<FloatRangeStruct, (int, string)> CornerNames => new()
    {
        //{ new FloatRangeStruct(0, 0.1f), (1, "Niki Lauda")},
        //{ new FloatRangeStruct(0, 0.2f), (2, "Münzer")},
        //{ new FloatRangeStruct(0, 0.3f), (3, "")},
        //{ new FloatRangeStruct(0, 0.4f), (4, "Rauch")},
        //{ new FloatRangeStruct(0, 0.5f), (5, "")},
        //{ new FloatRangeStruct(0, 0.6f), (6, "")},
        //{ new FloatRangeStruct(0, 0.7f), (7, "Graz")},
        //{ new FloatRangeStruct(0, 0.8f), (8, "")},
        //{ new FloatRangeStruct(0, 0.9f), (9, "Jochen Rindt")},
        //{ new FloatRangeStruct(0, 0.95f), (10, "")},
    };
}