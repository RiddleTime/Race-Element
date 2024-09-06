using RaceElement.Data.Common.SimulatorData;
using RaceElement.Data.Games.RaceRoom.SharedMemory;
using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.RaceRoom.DataMapper;
[Mapper]
internal static partial class RR3SessionDataMapper
{
    [MapProperty(nameof(Shared.TimeDeltaBestSelf), nameof(@SessionData.LapDeltaToSessionBestLapMs))]
    static partial void WithR3SharedMemory(Shared sharedData, SessionData sessionData);

    public static void AddR3SharedMemory(Shared sharedData, SessionData sessionData)
    {
        WithR3SharedMemory(sharedData, sessionData);
    }
}
