using System.Collections.Generic;

namespace Bonsai.Reactive
{
    static class EnumerableEx
    {
        internal static IEnumerable<TSource> Concat<TSource>(TSource first, TSource second, params TSource[] remainder)
        {
            yield return first;
            yield return second;
            for (int i = 0; i < remainder.Length; i++)
            {
                yield return remainder[i];
            }
        }
    }
}
