using System.Collections.Generic;

namespace GitCleanup
{
    internal static class EnumerableExtension
    {
        public static IEnumerable<IEnumerable<T>> Paged<T>(this IEnumerable<T> list, int pageSize)
        {
            var page = new List<T>();
            foreach (var item in list)
            {
                if (page.Count >= pageSize)
                {
                    yield return page;
                    page = new List<T>();
                }
                page.Add(item);
            }
            yield return page;
        }
    }
}