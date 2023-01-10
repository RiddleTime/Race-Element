using System;

namespace RaceElement.Util.SystemExtensions
{
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
    }
}
