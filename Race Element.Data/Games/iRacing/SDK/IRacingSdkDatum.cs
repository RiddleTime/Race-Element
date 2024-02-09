using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceElement.Data.Games.iRacing.SDK
{
    public class IRacingSdkDatum
    {
        public const int MaxNameLength = 32;
        public const int MaxDescLength = 64;
        public const int MaxUnitLength = 32;

        public const int Size = sizeof(IRacingSdkEnum.VarType) + sizeof(int) * 2 + sizeof(bool) + 3 /* padding */ + MaxNameLength + MaxDescLength + MaxUnitLength;

        public IRacingSdkEnum.VarType VarType { get; }
        public int Offset { get; }
        public int Count { get; }
        public bool CountAsTime { get; }
        public string Name { get; }
        public string Desc { get; }
        public string Unit { get; }

        public IRacingSdkDatum(IRacingSdkEnum.VarType varType, int offset, int count, bool countAsTime, string name, string desc, string unit)
        {
            VarType = varType;
            Offset = offset;
            Count = count;
            CountAsTime = countAsTime;
            Name = name;
            Desc = desc;
            Unit = unit;
        }
    }
}
