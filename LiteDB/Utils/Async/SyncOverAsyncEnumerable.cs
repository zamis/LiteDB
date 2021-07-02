using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using static LiteDB.Constants;

namespace LiteDB
{
    /// <summary>
    /// Implement a sync-over-async enumerator to use in BsonExpression agreggations
    /// </summary>
    internal class SyncOverAsyncEnumerable<T> : IAsyncEnumerable<T>
    {
        private readonly IEnumerable<T> _source;

        public SyncOverAsyncEnumerable(IEnumerable<T> source)
        {
            _source = source;
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new SyncOverAsyncEnumerator<T>(_source.GetEnumerator());
        }
    }
}