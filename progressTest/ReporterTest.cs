using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progress;

using Stack = progress.Stack<progress.Reporter>;

using static progressTest.Convenience;

namespace progressTest
{
    public sealed class ReporterTest
    {
        private const Int32 STEPS = 2;
        private readonly Stack _stack;
        private readonly Reporter _reporter;
        private readonly ProgressRecorder<Double> _progress;
        public ReporterTest()
        {
            _progress = new ProgressRecorder<Double>();
            _stack = Reporter.Root(_progress);
            _reporter = new Reporter(_stack, ProgressDriver.Create(STEPS), _progress);
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
            Assert.Contains(1d, _progress);
        }
        [Fact]
        public void ResetIsPerformedUponDisposal()
        {
            _reporter.Dispose();
            Assert.NotEqual(_stack.Peek(), _reporter);
        }
        [Fact]
        public void ReportAdvancesProgress()
        {
            const Int32 steps = 4;
            var progress = new ProgressRecorder<Double>();
            using(var reporter = new Reporter(_stack, ProgressDriver.Create(steps), progress)) {
                foreach(var _ in Enumerable.Range(0, steps)) {
                    reporter.Report();
                }
            }
            Assert.Equal(ExpectedProgress(steps), progress);
        }
        [Fact]
        public void ReportedProgressStaysWithinBounds()
        {
            const Int32 steps = 4;
            const Int32 overflowFactor = 2;
            var progress = new ProgressRecorder<Double>();
            using(var reporter = new Reporter(_stack, ProgressDriver.Create(steps), progress)) {
                foreach(var _ in Enumerable.Range(0, steps * overflowFactor)) {
                    reporter.Report();
                }
            }
            Assert.Equal(ExpectedProgress(steps), progress);
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
    }
}