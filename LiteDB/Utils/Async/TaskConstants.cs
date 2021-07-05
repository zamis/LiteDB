using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiteDB
{
    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    internal static class TaskConstants
    {
        private static readonly Task<bool> _booleanTrue = Task.FromResult(true);
        private static readonly Task<int> _intNegativeOne = Task.FromResult(-1);

        /// <summary>
        /// A task that has been completed with the value <c>true</c>.
        /// </summary>
        public static Task<bool> BooleanTrue => _booleanTrue;

        /// <summary>
        /// A task that has been completed with the value <c>false</c>.
        /// </summary>
        public static Task<bool> BooleanFalse => TaskConstants<bool>.Default;

        /// <summary>
        /// A task that has been completed with the value <c>0</c>.
        /// </summary>
        public static Task<int> Int32Zero => TaskConstants<int>.Default;

        /// <summary>
        /// A task that has been completed with the value <c>-1</c>.
        /// </summary>
        public static Task<int> Int32NegativeOne => _intNegativeOne;

        /// <summary>
        /// A <see cref="Task"/> that has been completed.
        /// </summary>
        public static Task Completed => Task.CompletedTask;

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task Canceled => TaskConstants<object>.Canceled;

    }

    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    public static class TaskConstants<T>
    {
        private static readonly Task<T> _defaultValue = Task.FromResult(default(T));
        private static readonly Task<T> _canceled = Task.FromCanceled<T>(new CancellationToken(true));

        /// <summary>
        /// A task that has been completed with the default value of <typeparamref name="T"/>.
        /// </summary>
        public static Task<T> Default => _defaultValue;

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task<T> Canceled => _canceled;
    }
}
