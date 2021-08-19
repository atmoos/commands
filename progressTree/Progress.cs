using System;
using System.Threading;
using progressReporting;

namespace progressTree
{
    public sealed class Progress
    {
        public static Progress Empty { get; } = new Progress();
        private Reporter _current;
        private readonly Func<ProgressDriver, IProgress<Double>, Reporter> _createNext;
        private Progress(IProgress<Double> root)
        {
            this._createNext = CreateReporter;
            this._current = Reporter.Root(this, root);
        }
        private Progress()
        {
            this._createNext = KeepCurrent;
            this._current = Reporter.Root(this, Extensions.Empty<Double>());
        }
        public Reporter Schedule(Int32 iterations) => Chain(ProgressDriver.Create(iterations));
        public Reporter Schedule(Int32 iterations, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(iterations), subProgress);
        public Reporter Schedule(TimeSpan expectedDuration) => Chain(ProgressDriver.Create(expectedDuration));
        public Reporter Schedule(TimeSpan expectedDuration, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(expectedDuration), subProgress);
        public Reporter Schedule<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress) => Chain(ProgressDriver.Create(target, nlProgress));
        public Reporter Schedule<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(target, nlProgress), subProgress);
        public static Progress Create(IProgress<Double> progress) => new(Guard(progress));
        private Reporter Chain(ProgressDriver driver) => this._createNext(driver, this._current.Progress);
        private Reporter Branch(ProgressDriver driver, IProgress<Double> progress) => this._createNext(driver, Guard(this._current.Progress.Zip(progress)));
        internal Reporter Exchange(Reporter next) => Interlocked.Exchange(ref this._current, next);
        private Reporter CreateReporter(ProgressDriver driver, IProgress<Double> progress) => new(this, driver, progress);
        private Reporter KeepCurrent(ProgressDriver _, IProgress<Double> __) => this._current;
        private static IProgress<Double> Guard(IProgress<Double> progress) => progress.Monotonic().Strictly.Increasing().Bounded(0, 1).Inclusive();
    }
}