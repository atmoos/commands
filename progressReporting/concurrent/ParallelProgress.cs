using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace progressReporting.concurrent
{
    internal sealed class ParallelProgress<TProgress>
     where TProgress : struct
    {
        private readonly Norm<TProgress> norm;
        private readonly IProgress<TProgress> progress;
        private readonly List<ProgressReceiver> receivers;

        private ParallelProgress(IProgress<TProgress> progress, Norm<TProgress> norm)
        {
            this.norm = norm;
            this.progress = progress;
            this.receivers = new List<ProgressReceiver>();
        }
        private void Report(in TProgress value) => this.progress.Report(this.norm(in value, this.receivers.Select(r => r.Current)));
        internal static IEnumerable<(IProgress<TProgress> progress, TItem item)> Create<TItem>(IProgress<TProgress> progress, Norm<TProgress> norm, IEnumerable<TItem> items)
        {
            var parent = new ParallelProgress<TProgress>(progress, norm);
            return items.Select<TItem, (IProgress<TProgress>, TItem)>(i => (new ProgressReceiver(parent), i)).ToList();
        }

        private sealed class ProgressReceiver : IProgress<TProgress>
        {
            private readonly ParallelProgress<TProgress> parent;
            private Current current;
            public TProgress Current => this.current.Value;
            public ProgressReceiver(ParallelProgress<TProgress> parent)
            {
                this.parent = parent;
                this.current = new Current(default);
                parent.receivers.Add(this);
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