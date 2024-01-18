using System.Collections.Generic;

namespace Editor.Extensions
{
    public static class CollectionExt
    {
        public static bool Empty<T>(this ICollection<T> list) => list.Count == 0;
    }
}
