using System.Collections.Generic;
using System.Linq;

namespace qASIC
{
    public static class IEnumerableExtensions
    {
        public static bool IndexInRange<T>(this IEnumerable<T> list, int index) =>
            index >= 0 && index < list.Count();
    }
}