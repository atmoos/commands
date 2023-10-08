using System;
using System.Diagnostics;
using System.Threading;

namespace progressTree;

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
        private Int32 currentIteration;
        private readonly Double expectedIterations;
        public IterativeDriver(Int32 expectedIterations)
        {
            this.currentIteration = 0;
            this.expectedIterations = expectedIterations;
        }
        public override Double Advance() => Interlocked.Increment(ref this.currentIteration) / this.expectedIterations;
        public override Double Accumulate(Double childProgress) => (this.currentIteration + childProgress) / this.expectedIterations;
    }
    private sealed class TemporalDriver : ProgressDriver
    {
        private readonly Stopwatch timer;
        private readonly Double expectedDuration;
        public TemporalDriver(TimeSpan expectedDuration)
        {
            this.timer = Stopwatch.StartNew();
            this.expectedDuration = expectedDuration.TotalSeconds;
        }
        public override Double Advance() => this.timer.Elapsed.TotalSeconds / this.expectedDuration;
        public override Double Accumulate(Double _) => Advance();
    }
    private sealed class FunctionalDriver<TProgress> : ProgressDriver
    {
        private Double stepSize;
        private Double currentDelta;
        private readonly Double range;
        private readonly Double lower;
        private readonly INonLinearProgress<TProgress> nlProgress;
        public FunctionalDriver(TProgress target, INonLinearProgress<TProgress> nlProgress)
        {
            this.nlProgress = nlProgress;
            this.lower = nlProgress.Linearise(nlProgress.Progress());
            this.range = Math.Abs(nlProgress.Linearise(target) - this.lower);
            this.currentDelta = this.stepSize = 0d;
        }
        public override Double Advance()
        {
            // const values are parameters for simple IIR filter
            const Double a = 3d / 5d;
            const Double b = 1d - a;
            Double delta = Math.Abs(this.nlProgress.Linearise(this.nlProgress.Progress()) - this.lower);
            Double stepSize = delta - Interlocked.Exchange(ref this.currentDelta, delta);
            Interlocked.Exchange(ref this.stepSize, (a * stepSize) + (b * this.stepSize));
            return delta / this.range;
        }
        public override Double Accumulate(Double childProgress) => (this.currentDelta + (this.stepSize * childProgress)) / this.range;
    }
}
public sealed class NlProgressAdapter<TProgress> : INonLinearProgress<TProgress>
{
    private readonly Func<TProgress> getProgress;
    private readonly Func<TProgress, Double> linearise;

    public NlProgressAdapter(Func<TProgress> getProgress, Func<TProgress, Double> linearise)
    {
        this.getProgress = getProgress;
        this.linearise = linearise;
    }
    public TProgress Progress() => this.getProgress();
    public Double Linearise(TProgress progress) => this.linearise(progress);
}