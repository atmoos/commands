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
        internal ConcurrentReporter(Reporter parent, in Int32 concurrencyLevel)
        {
            this.parent = parent;
            this.concurrentProgress = parent.Export().Concurrent(concurrencyLevel).Select(Progress.Create).ToList();
        }

        public IEnumerator<Progress> GetEnumerator() => this.concurrentProgress.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Dispose() => this.parent.Dispose();
    }
}