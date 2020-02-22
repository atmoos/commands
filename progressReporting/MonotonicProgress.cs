using System;
using System.Threading;

namespace progressReporting
{
    public interface IMonotonicFactory<out TMonotonic, TProgress>
        where TMonotonic : IProgress<TProgress>
    {
        TMonotonic Increasing(IProgress<TProgress> progress);
        TMonotonic Decreasing(IProgress<TProgress> progress);
    }
    public sealed class MonotonicProgress : IProgress<Double>
    {
        private Double _current;
        private readonly IProgress<Double> _progress;
        private readonly Func<Double, Double, Boolean> _match;
        private MonotonicProgress(IProgress<Double> progress, Func<Double, Double, Boolean> match, Double init)
        {
            _progress = progress;
            _match = match;
            _current = init;
        }
        public void Report(Double value)
        {
            if(_match(_current, value)) {
                Interlocked.Exchange(ref _current, value);
                _progress.Report(value);
            }
        }
        public static IMonotonicFactory<MonotonicProgress, Double> Strictly { get; } = new Strict();
        public static MonotonicProgress Increasing(IProgress<Double> progress) => new MonotonicProgress(progress, (c, v) => c <= v, Double.NegativeInfinity);
        public static MonotonicProgress Decreasing(IProgress<Double> progress) => new MonotonicProgress(progress, (c, v) => c >= v, Double.PositiveInfinity);
        private sealed class Strict : IMonotonicFactory<MonotonicProgress, Double>
        {
            public MonotonicProgress Increasing(IProgress<Double> progress) => new MonotonicProgress(progress, (c, v) => c < v, Double.NegativeInfinity);
            public MonotonicProgress Decreasing(IProgress<Double> progress) => new MonotonicProgress(progress, (c, v) => c > v, Double.PositiveInfinity);
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