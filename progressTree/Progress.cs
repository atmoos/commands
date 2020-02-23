using System;
using progressReporting;

namespace progressTree
{
    public sealed class Progress
    {
        public static Progress Empty { get; } = new Progress(Extensions.Empty<Double>());
        private readonly Stack<Reporter> _stack;
        private Progress(IProgress<Double> root) => _stack = Reporter.Root(root);
        public Reporter Schedule(Int32 iterations) => Chain(ProgressDriver.Create(iterations));
        public Reporter Schedule(Int32 iterations, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(iterations), subProgress);
        public Reporter Schedule(TimeSpan expectedDuration) => Chain(ProgressDriver.Create(expectedDuration));
        public Reporter Schedule(TimeSpan expectedDuration, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(expectedDuration), subProgress);
        public Reporter Schedule<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress) => Chain(ProgressDriver.Create(target, nlProgress));
        public Reporter Schedule<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress, IProgress<Double> subProgress) => Branch(ProgressDriver.Create(target, nlProgress), subProgress);
        public static Progress Create(IProgress<Double> progress) => new Progress(progress.Monotonic().Strictly.Increasing());
        private Reporter Chain(ProgressDriver driver) => Add(driver, _stack.Peek().Progress);
        private Reporter Branch(ProgressDriver driver, IProgress<Double> progress) => Add(driver, _stack.Peek().Progress.Zip(progress.Monotonic().Strictly.Increasing()));
        private Reporter Add(ProgressDriver driver, IProgress<Double> progress) => new Reporter(_stack, driver, progress);
    }
}