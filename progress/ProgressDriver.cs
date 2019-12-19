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

        private sealed class EmptyDriver : ProgressDriver
        {
            public override Double Accumulate(Double childProgress) => childProgress;

            public override Double Advance() => 0d;
        }
    }
}