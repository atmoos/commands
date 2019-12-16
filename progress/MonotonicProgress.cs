using System;
using System.Threading;

namespace progress
{
    public sealed class MonotonicProgress : IProgress<Double>
    {
        private Double _current;
        private readonly Double _lowerBound;
        private readonly Double _upperBound;
        private readonly IProgress<Double> _progress;
        public MonotonicProgress(IProgress<Double> progress, Double lowerBound = 0d, Double upperBound = 1d)
        {
            if(upperBound <= lowerBound) {
                var msg = $"Progress must increase monotonically. Recieved range [lower: '{lowerBound}', upper: '{upperBound}'].";
                throw new ArgumentNullException(msg);
            }
            _progress = progress;
            _lowerBound = lowerBound;
            _upperBound = upperBound;
            _current = lowerBound - 1d;
        }
        public void Report(Double progress)
        {
            if(progress <= _current || progress < _lowerBound || progress > _upperBound) {
                return;
            }
            Interlocked.Exchange(ref _current, progress);
            _progress.Report(progress);
        }
    }
}