using System;
using System.Diagnostics;
using System.Threading;

namespace progressTree
{
    internal abstract class ProgressDriver
    {
        private ProgressDriver() { }
        public abstract Double Advance();
        public abstract Double Accumulate(Double childProgress);
        internal static ProgressDriver Create(Int32 expectedIterations) => new IterativeDriver(expectedIterations);
        internal static ProgressDriver Create(TimeSpan expectedDuration) => new TemporalDriver(expectedDuration);
        internal static ProgressDriver Create<TProgress>(TProgress target, INonLinearProgress<TProgress> nlProgress)
        {
            return new FunctionalDriver<TProgress>(target, nlProgress);
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
            public override Double Advance() => Interlocked.Increment(ref _currentIteration) / _expectedIterations;
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
            private Double _stepSize;
            private Double _currentDelta;
            private readonly Double _range;
            private readonly Double _lower;
            private readonly INonLinearProgress<TProgress> _nlProgress;
            public FunctionalDriver(TProgress target, INonLinearProgress<TProgress> nlProgress)
            {
                _nlProgress = nlProgress;
                _lower = nlProgress.Linearise(nlProgress.Progress());
                _range = Math.Abs(nlProgress.Linearise(target) - _lower);
                _currentDelta = _stepSize = 0d;
            }
            public override Double Advance()
            {
                // const values are parameters for simple IIR filter
                const Double a = 3d / 5d;
                const Double b = 1d - a;
                Double delta = Math.Abs(_nlProgress.Linearise(_nlProgress.Progress()) - _lower);
                Double stepSize = delta - Interlocked.Exchange(ref _currentDelta, delta);
                Interlocked.Exchange(ref _stepSize, (a * stepSize) + (b * _stepSize));
                return delta / _range;
            }
            public override Double Accumulate(Double childProgress) => (_currentDelta + (_stepSize * childProgress)) / _range;
        }
    }
    public sealed class NlProgressAdapter<TProgress> : INonLinearProgress<TProgress>
    {
        private readonly Func<TProgress> _getProgress;
        private readonly Func<TProgress, Double> _linearise;

        public NlProgressAdapter(Func<TProgress> getProgress, Func<TProgress, Double> linearise)
        {
            _getProgress = getProgress;
            _linearise = linearise;
        }
        public TProgress Progress() => _getProgress();
        public Double Linearise(TProgress progress) => _linearise(progress);
    }
}