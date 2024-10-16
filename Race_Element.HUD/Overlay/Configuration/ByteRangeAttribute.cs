﻿using System;
using System.Collections.Generic;

namespace RaceElement.HUD.Overlay.Configuration;

[AttributeUsage(AttributeTargets.Property)]
public sealed class ByteRangeAttribute : Attribute
{
    public byte Min;
    public byte Max;
    public byte Increment;

    public ByteRangeAttribute(byte min, byte max, byte increment)
    {
        Min = min;
        Max = max;
        Increment = increment;
    }

    public static byte[] GetOptionsCollection(ByteRangeAttribute intRange)
    {
        List<byte> collection = [];

        for (byte i = intRange.Min; i < intRange.Max + intRange.Increment; i += intRange.Increment)
            collection.Add(i);

        return [.. collection];
    }
}
