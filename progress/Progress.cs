using System;
using System.Threading;

namespace progress
{
    public sealed class Progress
    {
        public static Progress Empty { get; } = new Progress(EmptyProgress<Double>.Empty);
        private IProgress<Double> _tree;
        private Progress(IProgress<Double> tree)
        {
            _tree = tree;
        }
        public Reporter Setup(Int32 iterations) => Chain(ProgressDriver.Create(iterations));
        public Reporter Setup(Int32 iterations, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(iterations), subProgress);
        public Reporter Setup(TimeSpan expectedDuration) => Chain(ProgressDriver.Create(expectedDuration));
        public Reporter Setup(TimeSpan expectedDuration, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(expectedDuration), subProgress);
        public Reporter Setup<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress) => Chain(ProgressDriver.Create(target, nlProgress));
        public Reporter Setup<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(target, nlProgress), subProgress);
        public static Progress Create(IProgress<Double> progress) => new Progress(new MonotonicProgress(progress));
        private Reporter Chain(ProgressDriver driver)
        {
            return new Reporter(driver, Interlocked.Exchange(ref _tree, new DriverAdapter(driver, _tree)), Push);
        }
        private Reporter Branch(ProgressDriver driver, IProgress<Double> subProgress)
        {
            var zip = new MonotonicProgress(new ProgressZip<Double>(subProgress, _tree));
            return new Reporter(driver, zip, Interlocked.Exchange(ref _tree, new DriverAdapter(driver, zip)), Push);
        }
        private void Push(IProgress<Double> tree) => Interlocked.Exchange(ref _tree, tree);
    }
}