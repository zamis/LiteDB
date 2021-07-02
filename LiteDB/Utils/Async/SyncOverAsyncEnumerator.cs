using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using static LiteDB.Constants;

namespace LiteDB
{
    /// <summary>
    /// Implement a sync-over-async enumerator to use in BsonExpression 
    /// </summary>
    internal class SyncOverAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _source;

        public SyncOverAsyncEnumerator(IEnumerator<T> source)
        {
            _source = source;

            this.Current = _source.Current;
        }

        public T Current { get; private set; }

        public ValueTask DisposeAsync()
        {
            _source.Dispose();

            return new ValueTask();
        }

        public ValueTask<bool> MoveNextAsync()
        {
            var result = _source.MoveNext();

            this.Current = _source.Current;

            return new ValueTask<bool>(Task.FromResult(result));
        }
    }
}