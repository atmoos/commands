using System;
using System.Collections.Generic;
using System.Linq;
using progressReporting;
using progressTree;
using Xunit;
using static progressTreeTest.Convenience;

namespace progressTreeTest
{
    public sealed class ReporterTest : IExportedProgressTest
    {
        private const Int32 steps = 2;
        private readonly Progress root;
        private readonly Reporter reporter;
        private readonly ProgressRecorder<Double> progress;
        public ReporterTest()
        {
            this.progress = new ProgressRecorder<Double>();
            this.root = Progress.Create(this.progress);
            this.reporter = new Reporter(this.root, ProgressDriver.Create(steps), this.progress);
        }
        [Fact]
        public void ZeroIsReportedUponCreation()
        {
            Assert.Equal(0d, this.progress.First());
        }
        [Fact]
        public void OneIsReportedUponDisposal()
        {
            this.reporter.Dispose();
            Assert.Contains(1d, this.progress);
        }
        [Fact]
        public void ResetIsPerformedUponDisposal()
        {
            var someNonNullReporter = this.reporter;
            this.reporter.Dispose();
            Assert.NotEqual(this.root.Exchange(someNonNullReporter), this.reporter);
        }
        [Fact]
        public void ReportAdvancesProgress()
        {
            const Int32 steps = 4;
            var progress = new ProgressRecorder<Double>();
            using(var reporter = new Reporter(this.root, ProgressDriver.Create(steps), progress)) {
                foreach(var _ in Enumerable.Range(0, steps)) {
                    reporter.Report();
                }
            }
            // Steps and disposal both advances progress by one, so ignore all that come after 'steps' intervals.
            Assert.Equal(ExpectedProgress(steps), progress.Take(steps + 1));
        }
        [Fact]
        public void ReportedProgressPerformsNoBoundChecks()
        {
            const Int32 steps = 4;
            const Int32 oneStepTooMany = steps + 1;
            var progress = new ProgressRecorder<Double>();
            var expectedProgress = ExpectedProgress(steps).Append(1.25).Append(1); // The last "1" is due to disposal..
            using(var reporter = new Reporter(this.root, ProgressDriver.Create(steps), progress)) {
                foreach(var _ in Enumerable.Range(0, oneStepTooMany)) {
                    reporter.Report();
                }
            }
            Assert.Equal(expectedProgress, progress);
        }
        [Fact]
        public void ExportedProgressIsScaled()
        {
            const Double scale = 1d / steps;
            var input = new[] { 0d, 0.25, 0.5, 0.75, 1 };
            IProgress<Double> progress = this.reporter.Export();
            this.reporter.Report(); // -> 1/STEPS
            var count = Report(progress, input);
            var expectedTail = input.Select(v => scale * (1d + v)).ToList();
            Assert.Equal(expectedTail, this.progress.TakeLast(count));
        }
        [Fact]
        public void OutOfBoundExportedProgressIsIgnored()
        {
            const Double EXTRA_ZERO = 0;
            var input = new[] { -1.1, 0, 0.4, 0.1, 1.0, 1.2 };
            // The extra zero is caused by the initial zero
            // in the reporters constructor. See ProgressTest
            // to see that this wont happen in production set-ups.
            var expected = new[] { EXTRA_ZERO, 0, 0.2, 0.5 };
            Report(this.reporter.Export(), input);
            Assert.Equal(expected, this.progress);
        }
        [Fact]
        public void ExportedProgressIsStrictlyMonotonic()
        {
            const Double scale = 1d / steps;
            var input = new[] { 0d, 0.25, 0.5, 0.4, 0.5, 0.75, 1 };
            IProgress<Double> progress = this.reporter.Export();
            this.reporter.Report(); // -> 1/STEPS
            Report(progress, input);
            var expectedTail = MakeStrictlyMonotonic(input.Select(v => scale * (1d + v))).ToList();
            Assert.Equal(expectedTail, this.progress.TakeLast(expectedTail.Count));
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