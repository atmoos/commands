using System;
using System.Diagnostics;

namespace progress
{
    public abstract class Reporter : IDisposable
    {
        private readonly Action _cleanup;
        private readonly IProgress<Double> _progress;

        private Reporter(IProgress<Double> progress, Action cleanup)
        {
            _progress = progress;
            _cleanup = cleanup;
            _progress.Report(0);
        }
        public abstract void Report();
        public void Dispose()
        {
            _progress.Report(1d);
            _cleanup();
        }

        internal static Reporter Create(IProgress<Double> progress, Int32 expectedIterations, Action cleanup) => new IterativeReporter(progress, expectedIterations, cleanup);
        internal static Reporter Create(IProgress<Double> progress, TimeSpan expectedDuration, Action cleanup) => new TemporalReporter(progress, expectedDuration, cleanup);

        private sealed class IterativeReporter : Reporter
        {
            private Int32 _currentIteration;
            private readonly Int32 _expectedIterations;

            public IterativeReporter(IProgress<Double> progress, Int32 expectedIterations, Action cleanup) : base(progress, cleanup)
            {
                _currentIteration = 0;
                _expectedIterations = expectedIterations;
            }

            public override void Report()
            {
                _progress.Report(((double)++_currentIteration) / _expectedIterations);
            }
        }
        private sealed class TemporalReporter : Reporter
        {
            private readonly Stopwatch _timer;
            private readonly Double _expectedDuration;
            public TemporalReporter(IProgress<Double> progress, TimeSpan expectedDuration, Action cleanup) : base(progress, cleanup)
            {
                _timer = Stopwatch.StartNew();
                _expectedDuration = expectedDuration.TotalSeconds;
            }
            public override void Report()
            {
                _progress.Report(_timer.Elapsed.TotalSeconds / _expectedDuration);
            }
        }
    }
}