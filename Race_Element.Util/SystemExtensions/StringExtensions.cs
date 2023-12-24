using System;
using System.Text;

namespace RaceElement.Util.SystemExtensions;

public static class StringExtensions
{
    public static string FillStart(this String text, int maxLength, char filler)
    {
        int length = text.Length;

        if (length < maxLength)
        {
            int missing = maxLength - length;
            for (int i = 0; i < missing; i++)
                text = filler + text;
        }

        return text;
    }

    public static string FillEnd(this String text, int maxLength, char filler)
    {
        int length = text.Length;

        if (length < maxLength)
        {
            int missing = maxLength - length;
            for (int i = 0; i < missing; i++)
                text += filler;
        }

        return text;
    }


    public static string ToString(this string[] values)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < values.Length; i++)
        {
            string v = values[i];
            builder.Append($"{{{v}}}");
            if (i < values.Length - 1)
                builder.Append(", ");
        }
        return builder.ToString();
    }
}
