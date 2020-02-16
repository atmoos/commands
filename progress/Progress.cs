using System;
using progress.reporters;

namespace progress
{
    public sealed class Progress
    {
        public static Progress Empty { get; } = new Progress(EmptyProgress<Double>.Empty);
        private readonly Stack<DriverAdapter> _stack;
        private Progress(IProgress<Double> root) => _stack = DriverAdapter.Root(root);
        public Reporter Setup(Int32 iterations) => Chain(ProgressDriver.Create(iterations));
        public Reporter Setup(Int32 iterations, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(iterations), subProgress);
        public Reporter Setup(TimeSpan expectedDuration) => Chain(ProgressDriver.Create(expectedDuration));
        public Reporter Setup(TimeSpan expectedDuration, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(expectedDuration), subProgress);
        public Reporter Setup<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress) => Chain(ProgressDriver.Create(target, nlProgress));
        public Reporter Setup<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(target, nlProgress), subProgress);
        public static Progress Create(IProgress<Double> progress) => new Progress(MonotonicProgress.Strictly.Increasing(progress));
        private Reporter Chain(ProgressDriver driver) => Make(driver, _stack.Peek());
        private Reporter Branch(ProgressDriver driver, IProgress<Double> subProgress) => Make(driver, MonotonicProgress.Strictly.Increasing(new ProgressZip<Double>(subProgress, _stack.Peek())));
        private Reporter Make(ProgressDriver driver, IProgress<Double> progress)
        {
            var adapter = new DriverAdapter(driver, _stack, progress);
            return new Reporter(driver, adapter, adapter);
        }
    }
}