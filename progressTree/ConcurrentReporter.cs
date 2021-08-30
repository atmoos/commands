using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using progressReporting;

namespace progressTree
{
    public sealed class ConcurrentReporter : IEnumerable<Progress>, IDisposable
    {
        private readonly Reporter parent;
        private readonly List<Progress> concurrentProgress;
        public Progress this[Int32 index] => this.concurrentProgress[index];
        public Int32 Count => this.concurrentProgress.Count;
        internal ConcurrentReporter(Reporter parent, CreateNorm<Double> norm, in Int32 concurrencyLevel)
        {
            this.parent = parent;
            this.concurrentProgress = parent.Export().Concurrent(norm, concurrencyLevel).Select(Progress.Create).ToList();
        }

        public IEnumerator<Progress> GetEnumerator() => this.concurrentProgress.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Dispose() => this.parent.Dispose();
    }

    public sealed class ConcurrentReporter<T> : IEnumerable<(T item, Progress progress)>, IDisposable
    {
        private readonly Reporter parent;
        private readonly List<(T item, Progress progress)> concurrentProgress;
        internal ConcurrentReporter(Reporter parent, CreateNorm<Double> norm, IEnumerable<T> items)
        {
            this.parent = parent;
            this.concurrentProgress = parent.Export().Concurrent(norm, items).Select(v => (v.item, Progress.Create(v.progress))).ToList();
        }

        public IEnumerator<(T item, Progress progress)> GetEnumerator() => this.concurrentProgress.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Dispose() => this.parent.Dispose();
    }
}