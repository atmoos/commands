using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace progressReporting
{
    public sealed class ParallelProgress<T>
     where T : struct, IComparable<T>
    {
        private readonly IProgress<T> root;
        private readonly Interceptor[] interceptors;

        private ParallelProgress(IProgress<T> root, IEnumerable<IProgress<T>> progress)
        {
            this.root = root;
            this.interceptors = progress.Select(p => new Interceptor(this, p)).ToArray();
        }
        private void Report(in T value)
        {
            Int32 minComparison;
            var currentState = this.interceptors.Select(i => i.Current).OrderBy(c => c).Take(2).ToArray();
            var (minValue, secondToMin) = (currentState[0], currentState[1]);
            if((minComparison = minValue.CompareTo(value)) <= 0) {
                if(minComparison == 0 || value.CompareTo(secondToMin) <= 0) {
                    this.root.Report(value);
                }
            }
        }

        public static IEnumerable<IProgress<T>> Intercept(IProgress<T> target, IEnumerable<IProgress<T>> progress)
        {
            var interceptors = new ParallelProgress<T>(target, progress).interceptors;
            return interceptors.Length switch
            {
                0 => new[] { target },
                1 => new[] { interceptors[0].Wrapped.Zip(target) },
                _ => interceptors
            };
        }

        private sealed class Interceptor : IProgress<T>
        {
            private readonly ParallelProgress<T> parent;
            private readonly ReaderWriterLockSlim guard = new();
            private T current;
            public T Current {
                get
                {
                    try {
                        this.guard.EnterReadLock();
                        return this.current;
                    } finally {
                        this.guard.ExitReadLock();
                    }
                }
            }
            internal IProgress<T> Wrapped { get; }

            public Interceptor(ParallelProgress<T> parent, IProgress<T> wrapped)
            {
                this.parent = parent;
                this.Wrapped = wrapped;
            }

            void IProgress<T>.Report(T value)
            {
                this.parent.Report(in value);
                this.guard.EnterWriteLock();
                this.current = value;
                this.guard.ExitWriteLock();
                this.Wrapped.Report(value);
            }
        }
    }
}