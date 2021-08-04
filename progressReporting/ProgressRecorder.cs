using System;
using System.Collections;
using System.Collections.Generic;

namespace progressReporting
{
    public sealed class ProgressRecorder<TProgress> : IProgress<TProgress>, IEnumerable<TProgress>
    {
        public readonly List<TProgress> _record = new();
        public TProgress this[Int32 index] => _record[index];
        public void Report(TProgress value) => _record.Add(value);
        public IEnumerator<TProgress> GetEnumerator() => _record.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}