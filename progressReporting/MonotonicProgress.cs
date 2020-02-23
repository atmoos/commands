using System;
using System.Threading;

namespace progressReporting
{
    public interface IMonotonicFactory<in TProgress>
    {
        IProgress<TProgress> Increasing();
        IProgress<TProgress> Decreasing();
    }
    public interface IMonotonicBuilder<in TProgress> : IMonotonicFactory<TProgress>
    {
        IMonotonicFactory<TProgress> Strictly { get; }
    }
    internal sealed class MonotonicBuilder : IMonotonicBuilder<Double>
    {
        private readonly IProgress<Double> _progress;
        internal MonotonicBuilder(IProgress<Double> progress) => _progress = progress;
        public IProgress<Double> Decreasing() => Monotonic.Decreasing(_progress);
        public IProgress<Double> Increasing() => Monotonic.Increasing(_progress);
        public IMonotonicFactory<Double> Strictly => new Strict(_progress);
        private sealed class Monotonic : IMonotonicFactory<Double>
        {
            private readonly IProgress<Double> _progress;
            internal Monotonic(IProgress<Double> progress) => _progress = progress;
            public IProgress<Double> Increasing() => Increasing(_progress);
            public IProgress<Double> Decreasing() => Decreasing(_progress);
            public static IProgress<Double> Increasing(IProgress<Double> progress) => new MonotonicProgress(progress, (c, v) => c <= v, Double.NegativeInfinity);
            public static IProgress<Double> Decreasing(IProgress<Double> progress) => new MonotonicProgress(progress, (c, v) => c >= v, Double.PositiveInfinity);
        }
        private sealed class Strict : IMonotonicFactory<Double>
        {
            private readonly IProgress<Double> _progress;
            internal Strict(IProgress<Double> progress) => _progress = progress;
            public IProgress<Double> Increasing() => new MonotonicProgress(_progress, (c, v) => c < v, Double.NegativeInfinity);
            public IProgress<Double> Decreasing() => new MonotonicProgress(_progress, (c, v) => c > v, Double.PositiveInfinity);
        }
        private sealed class MonotonicProgress : IProgress<Double>
        {
            private Double _current;
            private readonly IProgress<Double> _progress;
            private readonly Func<Double, Double, Boolean> _match;
            public MonotonicProgress(IProgress<Double> progress, Func<Double, Double, Boolean> match, Double init)
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
        }
    }
    internal sealed class MonotonicBuilder<TProgress> : IMonotonicBuilder<TProgress>
        where TProgress : IComparable<TProgress>
    {
        private readonly IProgress<TProgress> _progress;
        internal MonotonicBuilder(IProgress<TProgress> progress) => _progress = progress;
        public IProgress<TProgress> Decreasing() => Monotonic.Decreasing(_progress);
        public IProgress<TProgress> Increasing() => Monotonic.Increasing(_progress);
        public IMonotonicFactory<TProgress> Strictly => new Strict(_progress);
        private sealed class Monotonic : IMonotonicFactory<TProgress>
        {
            private readonly IProgress<TProgress> _progress;
            internal Monotonic(IProgress<TProgress> progress) => _progress = progress;
            public IProgress<TProgress> Increasing() => Increasing(_progress);
            public IProgress<TProgress> Decreasing() => Decreasing(_progress);
            public static IProgress<TProgress> Increasing(IProgress<TProgress> progress) => new MonotonicProgress(progress, (c, v) => c.CompareTo(v) <= 0);
            public static IProgress<TProgress> Decreasing(IProgress<TProgress> progress) => new MonotonicProgress(progress, (c, v) => c.CompareTo(v) >= 0);
        }
        private sealed class Strict : IMonotonicFactory<TProgress>
        {
            private readonly IProgress<TProgress> _progress;
            internal Strict(IProgress<TProgress> progress) => _progress = progress;
            public IProgress<TProgress> Increasing() => new MonotonicProgress(_progress, (c, v) => c.CompareTo(v) < 0);
            public IProgress<TProgress> Decreasing() => new MonotonicProgress(_progress, (c, v) => c.CompareTo(v) > 0);
        }
        internal sealed class MonotonicProgress : IProgress<TProgress>
        {
            private TProgress _current;
            private readonly IProgress<TProgress> _progress;
            private Func<TProgress, TProgress, Boolean> _match;
            public MonotonicProgress(IProgress<TProgress> progress, Func<TProgress, TProgress, Boolean> match)
            {
                // The default value will never be used because of the initial match lambda below, that
                // that does not touch the default value. After the first report, _current will have been
                // initialised
                _current = default!;
                _progress = progress;
                _match = (_, __) => { _match = match; return true; };
            }
            public void Report(TProgress value)
            {
                if(_match(_current, value)) {
                    _current = value;
                    _progress.Report(value);
                }
            }
        }
    }
}