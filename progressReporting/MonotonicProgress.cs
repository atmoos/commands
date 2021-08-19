using System;
using System.Threading;

namespace progressReporting
{
    internal delegate Boolean Monotonic<TProgress>(in TProgress current, in TProgress value);
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
        private readonly IProgress<Double> progress;
        internal MonotonicBuilder(IProgress<Double> progress) => this.progress = progress;
        public IProgress<Double> Decreasing() => Monotonic.Decreasing(this.progress);
        public IProgress<Double> Increasing() => Monotonic.Increasing(this.progress);
        public IMonotonicFactory<Double> Strictly => new Strict(this.progress);
        private sealed class Monotonic : IMonotonicFactory<Double>
        {
            private readonly IProgress<Double> progress;
            internal Monotonic(IProgress<Double> progress) => this.progress = progress;
            public IProgress<Double> Increasing() => Increasing(this.progress);
            public IProgress<Double> Decreasing() => Decreasing(this.progress);
            public static IProgress<Double> Increasing(IProgress<Double> progress) => new MonotonicProgress(progress, (in Double c, in Double v) => c <= v, Double.NegativeInfinity);
            public static IProgress<Double> Decreasing(IProgress<Double> progress) => new MonotonicProgress(progress, (in Double c, in Double v) => c >= v, Double.PositiveInfinity);
        }
        private sealed class Strict : IMonotonicFactory<Double>
        {
            private readonly IProgress<Double> progress;
            internal Strict(IProgress<Double> progress) => this.progress = progress;
            public IProgress<Double> Increasing() => new MonotonicProgress(this.progress, (in Double c, in Double v) => c < v, Double.NegativeInfinity);
            public IProgress<Double> Decreasing() => new MonotonicProgress(this.progress, (in Double c, in Double v) => c > v, Double.PositiveInfinity);
        }
        private sealed class MonotonicProgress : IProgress<Double>
        {
            private Double current;
            private readonly IProgress<Double> progress;
            private readonly Monotonic<Double> monotonic;
            public MonotonicProgress(IProgress<Double> progress, Monotonic<Double> monotonic, Double init)
            {
                this.progress = progress;
                this.monotonic = monotonic;
                this.current = init;
            }
            public void Report(Double value)
            {
                if(this.monotonic(in this.current, in value)) {
                    Interlocked.Exchange(ref this.current, value);
                    this.progress.Report(value);
                }
            }
        }
    }
    internal sealed class MonotonicBuilder<TProgress> : IMonotonicBuilder<TProgress>
        where TProgress : IComparable<TProgress>
    {
        private readonly IProgress<TProgress> progress;
        internal MonotonicBuilder(IProgress<TProgress> progress) => this.progress = progress;
        public IProgress<TProgress> Decreasing() => Monotonic.Decreasing(this.progress);
        public IProgress<TProgress> Increasing() => Monotonic.Increasing(this.progress);
        public IMonotonicFactory<TProgress> Strictly => new Strict(this.progress);
        private sealed class Monotonic : IMonotonicFactory<TProgress>
        {
            private readonly IProgress<TProgress> progress;
            internal Monotonic(IProgress<TProgress> progress) => this.progress = progress;
            public IProgress<TProgress> Increasing() => Increasing(this.progress);
            public IProgress<TProgress> Decreasing() => Decreasing(this.progress);
            public static IProgress<TProgress> Increasing(IProgress<TProgress> progress) => new MonotonicProgress(progress, (in TProgress c, in TProgress v) => c.CompareTo(v) <= 0);
            public static IProgress<TProgress> Decreasing(IProgress<TProgress> progress) => new MonotonicProgress(progress, (in TProgress c, in TProgress v) => c.CompareTo(v) >= 0);
        }
        private sealed class Strict : IMonotonicFactory<TProgress>
        {
            private readonly IProgress<TProgress> progress;
            internal Strict(IProgress<TProgress> progress) => this.progress = progress;
            public IProgress<TProgress> Increasing() => new MonotonicProgress(this.progress, (in TProgress c, in TProgress v) => c.CompareTo(v) < 0);
            public IProgress<TProgress> Decreasing() => new MonotonicProgress(this.progress, (in TProgress c, in TProgress v) => c.CompareTo(v) > 0);
        }
        internal sealed class MonotonicProgress : IProgress<TProgress>
        {
            private TProgress current;
            private readonly IProgress<TProgress> progress;
            private Monotonic<TProgress> monotonic;
            public MonotonicProgress(IProgress<TProgress> progress, Monotonic<TProgress> monotonic)
            {
                // The default value will never be used because of the initial match lambda below, that
                // that does not touch the default value. After the first report, _current will have been
                // initialised
                this.current = default!;
                this.progress = progress;
                this.monotonic = (in TProgress _, in TProgress __) => { this.monotonic = monotonic; return true; };
            }
            public void Report(TProgress value)
            {
                if(this.monotonic(in this.current, in value)) {
                    this.current = value;
                    this.progress.Report(value);
                }
            }
        }
    }
}