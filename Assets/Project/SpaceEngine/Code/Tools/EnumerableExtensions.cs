using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SpaceEngine.Tools
{
    public static class EnumerableExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AddRange<T>(this ICollection<T> self, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                self.Add(item);
            }
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> self, HashSet<T> hashSet)
        {
            hashSet.AddRange(self);

            return hashSet;
        }
    }
}