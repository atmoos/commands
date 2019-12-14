using System;
using System.Diagnostics;

namespace commands.progress
{
    public abstract class Reporter : IDisposable
    {
        private readonly Progress _progress;
        private Reporter(Progress progress)
        {
            _progress = progress;
            _progress.Report(0);
        }
        public abstract void Report();
        public void Dispose() => _progress.Report(1d);

        internal static Reporter Create(Progress progress, Int32 expectedIterations) => new IterativeReporter(progress, expectedIterations);
        internal static Reporter Create(Progress progress, TimeSpan expectedDuration) => new TemporalReporter(progress, expectedDuration);

        private sealed class IterativeReporter : Reporter
        {
            private Int32 _currentIteration;
            private readonly Int32 _expectedIterations;

            public IterativeReporter(Progress progress, Int32 expectedIterations) : base(progress)
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
            public TemporalReporter(Progress progress, TimeSpan expectedDuration) : base(progress)
            {
                _timer = Stopwatch.StartNew();
                _expectedDuration = expectedDuration.TotalSeconds;
            }
            public override void Report()
            {
                _progress.Report(_timer.Elapsed.TotalSeconds/_expectedDuration);
            }
        }
    }
}