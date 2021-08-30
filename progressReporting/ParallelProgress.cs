using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace progressReporting
{
    internal sealed class ParallelProgress<TProgress, TItem>
     where TProgress : struct, IComparable<TProgress>
    {
        private readonly IProgress<TProgress> root;
        private readonly ProgressReceiver[] receivers;

        private ParallelProgress(IProgress<TProgress> root, IEnumerable<TItem> items)
        {
            this.root = root;
            this.receivers = items.Select(item => new ProgressReceiver(this, item)).ToArray();
        }
        private void Report(in TProgress value)
        {
            var minValue = this.receivers.Min(r => r.Current);
            if(minValue.CompareTo(value) <= 0) {
                this.root.Report(minValue);
            }
        }

        public static IEnumerable<(IProgress<TProgress> progress, TItem item)> Create(IProgress<TProgress> target, IEnumerable<TItem> items)
        {
            var parallelProgress = new ParallelProgress<TProgress, TItem>(target, items).receivers;
            return parallelProgress.Select<ProgressReceiver, (IProgress<TProgress>, TItem)>(p => (p, p.Item));
        }

        private sealed class ProgressReceiver : IProgress<TProgress>
        {
            private readonly ParallelProgress<TProgress, TItem> parent;
            private Current current;
            public TProgress Current => this.current.Value;

            public TItem Item { get; }

            public ProgressReceiver(ParallelProgress<TProgress, TItem> parent, TItem item)
            {
                this.parent = parent;
                this.current = new Current(default);
                Item = item;
            }

            void IProgress<TProgress>.Report(TProgress value)
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
            public TProgress Value { get; }
            public Current(TProgress value) => Value = value;
        }
    }
}