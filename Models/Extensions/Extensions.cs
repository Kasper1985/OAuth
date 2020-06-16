using System;
using System.Linq;

namespace Models.Extensions
{
    public static class Extensions
    {
        public static T Convert<T>(this int clientType) where T : Enum
        {
            foreach (var value in Enum.GetValues(typeof(T)))
                if ((int)value == clientType)
                    return (T)value;

            return default;
        }

        public static bool In(this int number, params int[] values) => values.Contains(number);
    }
}
