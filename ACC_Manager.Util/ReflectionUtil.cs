using System;
using System.Reflection;
using static RaceElement.ACCSharedMemory;

namespace RaceElement.Util
{
    public class ReflectionUtil
    {
        public static object FieldTypeValue(FieldInfo member, object value)
        {

            if (member.FieldType.Name == typeof(byte[]).Name)
            {
                byte[] arr = (byte[])value;
                value = string.Empty;
                foreach (byte v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }


            if (member.FieldType.Name == typeof(Int32[]).Name)
            {
                Int32[] arr = (Int32[])value;
                value = string.Empty;
                foreach (Int32 v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(Single[]).Name)
            {
                Single[] arr = (Single[])value;
                value = string.Empty;
                foreach (Single v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(StructVector3[]).Name)
            {
                StructVector3[] arr = (StructVector3[])value;
                value = string.Empty;
                foreach (StructVector3 v in arr)
                {
                    value += $"{{{v}}}, ";
                }
            }

            if (member.FieldType.Name == typeof(StructVector3).Name)
            {
                value = (StructVector3)value;
            }

            return value;
        }
    }
}
