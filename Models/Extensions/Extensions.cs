using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Models.Enumerations;

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
    }
}
