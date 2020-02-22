using System;
using System.Collections;
using System.Collections.Generic;

namespace progressReporting
{
    public sealed class ProgressRecorder<TProgress> : IProgress<TProgress>, IEnumerable<TProgress>
    {
        public List<TProgress> _record = new List<TProgress>();
        public TProgress this[Int32 index] { get { return _record[index]; } }
        public void Report(TProgress value) => _record.Add(value);
        public IEnumerator<TProgress> GetEnumerator() => _record.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}