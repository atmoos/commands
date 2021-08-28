using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace progressReporting
{
    internal sealed class ParallelProgress<T>
     where T : struct, IComparable<T>
    {
        private Current? current;
        private readonly IProgress<T> root;
        private readonly ProgressReceiver[] receivers;

        private ParallelProgress(IProgress<T> root, Int32 concurrencyLevel)
        {
            this.root = root;
            this.current = null; // this enables us to actually report the default value of T should it arrive as initial value.
            this.receivers = Enumerable.Range(0, concurrencyLevel).Select(_ => new ProgressReceiver(this)).ToArray();
        }
        private void Report(in T value)
        {
            var minValue = this.receivers.Min(r => r.Current);
            if(minValue.CompareTo(value) <= 0) {
                // Also ensure we're strictly monotonic
                var minState = this.current;
                if(minState == null || minState.Value.CompareTo(minValue) < 0) {
                    Interlocked.Exchange(ref this.current, new Current(minValue));
                    this.root.Report(minValue);
                }
            }
        }

        public static IEnumerable<IProgress<T>> FanOut(IProgress<T> target, Int32 concurrencyLevel)
            => concurrencyLevel switch
            {
                > 1 => new ParallelProgress<T>(target, concurrencyLevel).receivers,
                _ => new[] { target }
            };

        private sealed class ProgressReceiver : IProgress<T>
        {
            private readonly ParallelProgress<T> parent;
            private Current current;
            public T Current => this.current.Value;

            public ProgressReceiver(ParallelProgress<T> parent)
            {
                this.parent = parent;
                this.current = new Current(default);
            }

            void IProgress<T>.Report(T value)
            {
                Interlocked.Exchange(ref this.current, new Current(value));
                this.parent.Report(in value);
            }
        }

        // To avoid having to use a locking mechanism, we wrap
        // reported values (structs) in a class, that we can atomically
        // exchange. This gives us good enough thread safe reporting consistency.
        private sealed class Current
        {
            public T Value { get; }
            public Current(T value) => Value = value;
        }
    }
}