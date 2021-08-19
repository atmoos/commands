using System;

namespace progressReporting
{
    internal sealed class ProgressZip<TProgress> : IProgress<TProgress>
    {
        private readonly IProgress<TProgress> first, second;
        public ProgressZip(IProgress<TProgress> first, IProgress<TProgress> second) => (this.first, this.second) = (first, second);
        public void Report(TProgress value) { this.first.Report(value); this.second.Report(value); }
    }
}