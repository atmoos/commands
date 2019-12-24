using System;
using System.Threading;

namespace progress.reporters
{
    public sealed class MonotonicProgress : IProgress<Double>
    {
        private Double _current;
        private readonly IProgress<Double> _progress;
        public MonotonicProgress(IProgress<Double> progress)
        {
            _progress = progress;
            _current = Double.NegativeInfinity;
        }
        public void Report(Double value)
        {
            if(value <= _current) {
                return;
            }
            Interlocked.Exchange(ref _current, value);
            _progress.Report(value);
        }
    }
    public sealed class MonotonicProgress<TProgress> : IProgress<TProgress>
   where TProgress : class, IComparable<TProgress>
    {
        private TProgress _current;
        private readonly IProgress<TProgress> _progress;
        public MonotonicProgress(IProgress<TProgress> progress, TProgress init = default)
        {
            _progress = progress;
            _current = init;
        }
        public void Report(TProgress value)
        {
            if(_current.CompareTo(value) > 0) {
                return;
            }
            Interlocked.Exchange(ref _current, value);
            _progress.Report(value);
        }
    }
}