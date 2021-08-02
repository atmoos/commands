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
            _createNext = CreateReporter;
            _current = Reporter.Root(this, root);
        }
        private Progress()
        {
            _createNext = KeepCurrent;
            _current = Reporter.Root(this, Extensions.Empty<Double>());
        }
        public Reporter Schedule(Int32 iterations) => Chain(ProgressDriver.Create(iterations));
        public Reporter Schedule(Int32 iterations, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(iterations), subProgress);
        public Reporter Schedule(TimeSpan expectedDuration) => Chain(ProgressDriver.Create(expectedDuration));
        public Reporter Schedule(TimeSpan expectedDuration, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(expectedDuration), subProgress);
        public Reporter Schedule<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress) => Chain(ProgressDriver.Create(target, nlProgress));
        public Reporter Schedule<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(target, nlProgress), subProgress);
        public static Progress Create(IProgress<Double> progress) => new Progress(progress.Monotonic().Strictly.Increasing());
        private Reporter Chain(ProgressDriver driver) => _createNext(driver, _current.Progress);
        private Reporter Branch(ProgressDriver driver, IProgress<Double> progress) => _createNext(driver, _current.Progress.Zip(progress.Monotonic().Strictly.Increasing()));
        internal Reporter Exchange(Reporter next) => Interlocked.Exchange(ref _current, next);
        private Reporter CreateReporter(ProgressDriver driver, IProgress<Double> progress) => new Reporter(this, driver, progress);
        private Reporter KeepCurrent(ProgressDriver _, IProgress<Double> __) => _current;
    }
}