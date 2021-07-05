using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB
{
    /// <summary>
    /// Provides completed task constants.
    /// https://github.com/StephenCleary/AsyncEx/blob/v5.0.0/src/Nito.AsyncEx.Interop.WaitHandles/Interop/WaitHandleAsyncFactory.cs
    /// </summary>
    internal static class ManualResetEventSlimAsyncExtensions
    {
        public static async Task<bool> WaitAsync(this ManualResetEventSlim manualReset)
        {
            return await FromWaitHandle(manualReset.WaitHandle, Timeout.InfiniteTimeSpan, CancellationToken.None);
        }

        public static async Task<bool> WaitAsync(this ManualResetEventSlim manualReset, CancellationToken cancellationToken)
        {
            return await FromWaitHandle(manualReset.WaitHandle, Timeout.InfiniteTimeSpan, cancellationToken);
        }

        public static Task<bool> FromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            // Handle synchronous cases.
            var alreadySignalled = handle.WaitOne(0);
            if (alreadySignalled)
                return TaskConstants.BooleanTrue;
            if (timeout == TimeSpan.Zero)
                return TaskConstants.BooleanFalse;
            if (cancellationToken.IsCancellationRequested)
                return TaskConstants<bool>.Canceled;

            // Register all asynchronous cases.
            return DoFromWaitHandle(handle, timeout, cancellationToken);
        }

        private static async Task<bool> DoFromWaitHandle(WaitHandle handle, TimeSpan timeout, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();

            using (new ThreadPoolRegistration(handle, timeout, tcs))
            using (cancellationToken.Register(state => ((TaskCompletionSource<bool>)state).TrySetCanceled(), tcs, useSynchronizationContext: false))
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }

        private sealed class ThreadPoolRegistration : IDisposable
        {
            private readonly RegisteredWaitHandle _registeredWaitHandle;

            public ThreadPoolRegistration(WaitHandle handle, TimeSpan timeout, TaskCompletionSource<bool> tcs)
            {
                _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(handle,
                    (state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut), tcs,
                    timeout, executeOnlyOnce: true);
            }

            void IDisposable.Dispose() => _registeredWaitHandle.Unregister(null);
        }
    }
}
