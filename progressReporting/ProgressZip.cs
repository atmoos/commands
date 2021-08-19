using System;

namespace progressReporting
{
    internal sealed class ProgressZip<TProgress> : IProgress<TProgress>
    {
        private readonly IProgress<TProgress> _first, _second;
        public ProgressZip(IProgress<TProgress> first, IProgress<TProgress> second) => (this._first, this._second) = (first, second);
        public void Report(TProgress value) { this._first.Report(value); this._second.Report(value); }
    }
}