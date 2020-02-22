using System;

namespace progressReporting
{
    internal sealed class ProgressZip<TProgress> : IProgress<TProgress>
    {
        private readonly IProgress<TProgress> _first, _second;
        public ProgressZip(IProgress<TProgress> first, IProgress<TProgress> second) => (_first, _second) = (first, second);
        public void Report(TProgress value) { _first.Report(value); _second.Report(value); }
    }
}