using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progress;
using progress.reporters;

namespace progressTest
{
    public sealed class ReporterTest
    {
        private const Int32 STEPS = 2;
        private readonly Reset _reset;
        private readonly Reporter _reporter;
        private readonly ProgressRecorder<Double> _progress;
        public ReporterTest()
        {
            _reset = new Reset();
            _progress = new ProgressRecorder<Double>();
            _reporter = new Reporter(ProgressDriver.Create(STEPS), _progress, _reset);
        }
        [Fact]
        public void ZeroIsReportedUponCreation()
        {
            Assert.Equal(0d, _progress.First());
        }
        [Fact]
        public void OneIsReportedUponDisposal()
        {
            _reporter.Dispose();
            Assert.Equal(1d, _progress.Last());
        }
        [Fact]
        public void ResetIsPerformedUponDisposal()
        {
            _reporter.Dispose();
            Assert.True(_reset.Disposed, "Reset action must be called!");
        }
        [Fact]
        public void ReportAdvancesProgress()
        {
            const Int32 steps = 4;
            var progress = new ProgressRecorder<Double>();
            using(var reporter = new Reporter(ProgressDriver.Create(steps), progress, _reset)) {
                foreach(var _ in Enumerable.Range(0, steps)) {
                    reporter.Report();
                }
            }
            Assert.Equal(Enumerable.Range(0, steps + 1).Select(v => ((Double)v) / steps).Append(1d), progress);
        }
        [Fact]
        public void ExportedProgressIsScaled()
        {
            Double scale = 1d / STEPS;
            var input = new[] { 0d, 0.25, 0.5, 0.75, 1 };
            IProgress<Double> progress = _reporter.Export();
            _reporter.Report(); // -> 1/STEPS
            var count = Report(progress, input);
            var expectedTail = input.Select(v => scale * (1d + v)).ToList();
            Assert.Equal(expectedTail, _progress.TakeLast(count));
        }
        [Fact]
        public void ExportedProgressIsStrictlyMonotonic()
        {
            Double scale = 1d / STEPS;
            var input = new[] { 0d, 0.25, 0.5, 0.5, 0.75, 1 };
            IProgress<Double> progress = _reporter.Export();
            _reporter.Report(); // -> 1/STEPS
            Report(progress, input);
            var expectedTail = MakeStrictlyMonotonic(input.Select(v => scale * (1d + v))).ToList();
            Assert.Equal(expectedTail, _progress.TakeLast(expectedTail.Count));
        }
        private static Int32 Report(IProgress<Double> progress, params Double[] range)
        {
            foreach(var value in range) {
                progress.Report(value);
            }
            return range.Length;
        }
        private static IEnumerable<Double> MakeStrictlyMonotonic(IEnumerable<Double> range)
        {
            Double current = Double.NegativeInfinity;
            foreach(var value in range) {
                if(value > current) {
                    current = value;
                    yield return value;
                }
            }
        }
        private sealed class Reset : IDisposable
        {
            public Boolean Disposed { get; private set; } = false;
            public void Dispose() => Disposed = true;
        }
    }
}