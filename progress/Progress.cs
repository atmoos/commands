using System;
using System.Threading;

namespace progress
{
    public sealed class Progress
    {
        public static Progress Empty { get; } = new Progress(EmptyProgress<Double>.Empty);
        private readonly Stack<IProgress<Double>> _stack;
        private Progress(IProgress<Double> root) => _stack = new Stack<IProgress<Double>>(root);
        public Reporter Setup(Int32 iterations) => Chain(ProgressDriver.Create(iterations));
        public Reporter Setup(Int32 iterations, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(iterations), subProgress);
        public Reporter Setup(TimeSpan expectedDuration) => Chain(ProgressDriver.Create(expectedDuration));
        public Reporter Setup(TimeSpan expectedDuration, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(expectedDuration), subProgress);
        public Reporter Setup<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress) => Chain(ProgressDriver.Create(target, nlProgress));
        public Reporter Setup<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(target, nlProgress), subProgress);
        public static Progress Create(IProgress<Double> progress) => new Progress(new MonotonicProgress(progress));
        private Reporter Chain(ProgressDriver driver)
        {
            IProgress<Double> progress = _stack.Push(new DriverAdapter(driver, _stack.Peek()));
            return new Reporter(driver, progress, _stack.ResetWith(progress));
        }
        private Reporter Branch(ProgressDriver driver, IProgress<Double> subProgress)
        {
            var zip = new MonotonicProgress(new ProgressZip<Double>(subProgress, _stack.Peek()));
            return new Reporter(driver, zip, _stack.ResetWith(_stack.Push(new DriverAdapter(driver, zip))));
        }
    }
}