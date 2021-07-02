using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using static LiteDB.Constants;

namespace LiteDB
{
    internal static class IAsyncEnumerableExtensions
    {
        public static async Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
        {
            var result = new List<T>();

            await foreach(var item in source)
            {
                result.Add(item);
            }

            return result;
        }
    }
}