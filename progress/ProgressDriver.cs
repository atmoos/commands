using System;
using System.Threading;
using System.Diagnostics;

namespace progress
{
    internal abstract class ProgressDriver
    {
        public static ProgressDriver Empty { get; } = new EmptyDriver();
        private ProgressDriver() { }
        public abstract Double Advance();
        public abstract Double Accumulate(Double childProgress);
        internal static ProgressDriver Create(Int32 expectedIterations) => new IterativeDriver(expectedIterations);
        internal static ProgressDriver Create(TimeSpan expectedDuration) => new TemporalDriver(expectedDuration);
        internal static ProgressDriver Create<TProcess>(TProcess target, Func<TProcess> progressGetter, Func<TProcess, Double> linearization)
        {
            return new FunctionalDriver<TProcess>(target, progressGetter, linearization);
        }
        private sealed class IterativeDriver : ProgressDriver
        {
            private Int32 _currentIteration;
            private readonly Double _expectedIterations;
            public IterativeDriver(Int32 expectedIterations)
            {
                _currentIteration = 0;
                _expectedIterations = expectedIterations;
            }
            public override Double Advance() => ((Double)Interlocked.Increment(ref _currentIteration)) / _expectedIterations;
            public override Double Accumulate(Double childProgress) => (_currentIteration + childProgress) / _expectedIterations;
        }
        private sealed class TemporalDriver : ProgressDriver
        {
            private readonly Stopwatch _timer;
            private readonly Double _expectedDuration;
            public TemporalDriver(TimeSpan expectedDuration)
            {
                _timer = Stopwatch.StartNew();
                _expectedDuration = expectedDuration.TotalSeconds;
            }
            public override Double Advance() => _timer.Elapsed.TotalSeconds / _expectedDuration;
            public override Double Accumulate(Double _) => Advance();
        }
        private sealed class FunctionalDriver<TProgress> : ProgressDriver
        {
            private Double _currentDelta;
            private readonly Double _range;
            private readonly Double _lower;
            private readonly TProgress _target;
            private readonly Func<TProgress> _getProgress;
            private readonly Func<TProgress, Double> _linearize;
            public FunctionalDriver(TProgress target, Func<TProgress> getProgress, Func<TProgress, Double> linearize)
            {
                _target = target;
                _getProgress = getProgress;
                _linearize = linearize;
                _lower = _linearize(_getProgress());
                _range = Math.Abs(linearize(target) - _lower);
                _currentDelta = 0d;
            }
            public override Double Advance()
            {
                Double delta = Math.Abs(_linearize(_getProgress()) - _lower);
                Interlocked.Exchange(ref _currentDelta, delta);
                return delta / _range;
            }
            public override Double Accumulate(Double childProgress) => (_currentDelta + childProgress) / _range;
        }
        private sealed class EmptyDriver : ProgressDriver
        {
            public override Double Accumulate(Double childProgress) => childProgress;

            public override Double Advance() => 0d;
        }
    }
}