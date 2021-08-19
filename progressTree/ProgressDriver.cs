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
                this._currentIteration = 0;
                this._expectedIterations = expectedIterations;
            }
            public override Double Advance() => Interlocked.Increment(ref this._currentIteration) / this._expectedIterations;
            public override Double Accumulate(Double childProgress) => (this._currentIteration + childProgress) / this._expectedIterations;
        }
        private sealed class TemporalDriver : ProgressDriver
        {
            private readonly Stopwatch _timer;
            private readonly Double _expectedDuration;
            public TemporalDriver(TimeSpan expectedDuration)
            {
                this._timer = Stopwatch.StartNew();
                this._expectedDuration = expectedDuration.TotalSeconds;
            }
            public override Double Advance() => this._timer.Elapsed.TotalSeconds / this._expectedDuration;
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
                this._nlProgress = nlProgress;
                this._lower = nlProgress.Linearise(nlProgress.Progress());
                this._range = Math.Abs(nlProgress.Linearise(target) - this._lower);
                this._currentDelta = this._stepSize = 0d;
            }
            public override Double Advance()
            {
                // const values are parameters for simple IIR filter
                const Double a = 3d / 5d;
                const Double b = 1d - a;
                Double delta = Math.Abs(this._nlProgress.Linearise(this._nlProgress.Progress()) - this._lower);
                Double stepSize = delta - Interlocked.Exchange(ref this._currentDelta, delta);
                Interlocked.Exchange(ref this._stepSize, (a * stepSize) + (b * this._stepSize));
                return delta / this._range;
            }
            public override Double Accumulate(Double childProgress) => (this._currentDelta + (this._stepSize * childProgress)) / this._range;
        }
    }
    public sealed class NlProgressAdapter<TProgress> : INonLinearProgress<TProgress>
    {
        private readonly Func<TProgress> _getProgress;
        private readonly Func<TProgress, Double> _linearise;

        public NlProgressAdapter(Func<TProgress> getProgress, Func<TProgress, Double> linearise)
        {
            this._getProgress = getProgress;
            this._linearise = linearise;
        }
        public TProgress Progress() => this._getProgress();
        public Double Linearise(TProgress progress) => this._linearise(progress);
    }
}