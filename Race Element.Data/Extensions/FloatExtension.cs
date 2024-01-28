using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Extensions;

internal static class FloatExtension
{
    public static float ToRadians(this float degrees) => (float)(degrees * (Math.PI / 180));
}
