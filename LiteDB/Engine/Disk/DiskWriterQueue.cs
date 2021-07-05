using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static LiteDB.Constants;

namespace LiteDB.Engine
{
    /// <summary>
    /// Implement disk write queue and async writer thread - used only for write on LOG file
    /// [ThreadSafe]
    /// </summary>
    internal class DiskWriterQueue : IDisposable
    {
        private readonly Stream _stream;

        private Task _task;
        private bool _running;
        private ManualResetEventSlim _waitHandle = new ManualResetEventSlim(false);

        private ConcurrentQueue<PageBuffer> _queue = new ConcurrentQueue<PageBuffer>();

        public DiskWriterQueue(Stream stream)
        {
            _stream = stream;
        }

        /// <summary>
        /// Get how many pages are waiting for store
        /// </summary>
        public int Length => _queue.Count;

        /// <summary>
        /// Add new pages into write queue list to be written on disk on LOG file
        /// </summary>
        public void EnqueuePages(IEnumerable<PageBuffer> pages)
        {
            if (_running == false) return;

            foreach(var page in pages)
            {
                _queue.Enqueue(page);
            }

            _waitHandle.Set();
        }

        /// <summary>
        /// Create a background task to keep running over queue to write on stream
        /// </summary>
        public void StartTask(CancellationToken cancellationToken)
        {
            if (_task != null) throw new Exception("Background task aldready created");

            _task = Task.Run(async () =>
            {
                while (_running)
                {
                    _waitHandle.Reset();

                    await _waitHandle.WaitAsync(cancellationToken);

                    if (_running == false) break;

                    await this.ExecuteQueueAsync(cancellationToken);
                }
            });
        }

        /// <summary>
        /// Execute all items in queue sync
        /// </summary>
        private async Task ExecuteQueueAsync(CancellationToken cancellationToken)
        {
            if (_queue.Count == 0) return;

            var count = 0;

            try
            {
                while (_queue.TryDequeue(out var page))
                {
                    ENSURE(page.ShareCounter > 0, "page must be shared at least 1");

                    // set stream position according to page
                    _stream.Position = page.Position;

                    await _stream.WriteAsync(page.Array, page.Offset, PAGE_SIZE, cancellationToken);

                    // release page here (no page use after this)
                    page.Release();

                    count++;
                }

                // after this I will have 100% sure data are safe on log file
                await _stream.FlushAsync(cancellationToken);
            }
            catch (IOException)
            {
                //TODO: notify database to stop working (throw error in all operations)
            }
        }

        public void Dispose()
        {
            LOG($"disposing disk writer queue (with {_queue.Count} pages in queue)", "DISK");

            _running = false;
            _waitHandle.Set();
            _task.Wait();
        }
    }
}