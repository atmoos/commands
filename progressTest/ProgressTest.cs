using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progress;
using progressReporting;

using static progressTest.Convenience;

namespace progressTest
{
    public class ProgressTest
    {
        private readonly Progress _progressReporter;
        private readonly ProgressRecorder<Double> _actualProgress;

        public ProgressTest()
        {
            _actualProgress = new ProgressRecorder<Double>();
            _progressReporter = Progress.Create(_actualProgress);
        }

        [Fact]
        public void IterativeProgress()
        {
            const Int32 iterations = 8;
            var expected = ExpectedProgress(iterations);
            GenerateProgress(_progressReporter, iterations);
            Assert.Equal(expected, _actualProgress);
        }

        [Fact]
        public void ReportsOnChildProgress()
        {
            const Int32 childIterations = 4;
            const Int32 parentIterations = 8;
            var childProgress = new List<IEnumerable<Double>>();
            using(var parentReport = _progressReporter.Setup(parentIterations)) {
                for(Int32 p = 0; p < parentIterations; ++p) {
                    var subProgress = new ProgressRecorder<Double>();
                    using(var childReport = _progressReporter.Setup(childIterations, subProgress)) {
                        childProgress.Add(subProgress);
                        for(Int32 c = 0; c < childIterations; ++c) {
                            childReport.Report();
                        }
                    }
                }
            }
            Assert.Equal(ExpectedProgress(childIterations), childProgress[0]);
            Assert.Equal(ExpectedProgress(childIterations * parentIterations), _actualProgress);
        }
        [Fact]
        public void NotReportingAnythingWithinUsingStatementsReportsSetupBoundaries()
        {
            List<Double> expected = new List<Double>();
            using(_progressReporter.Setup(4)) {
                expected.Add(0);
                using(_progressReporter.Setup(3)) {
                }
                expected.Add(1d / 4);
                using(_progressReporter.Setup(2)) {
                    using(_progressReporter.Setup(6)) { }
                    expected.Add(3d / 8);
                    using(_progressReporter.Setup(8)) { }
                    expected.Add(2d / 4);
                }
                using(_progressReporter.Setup(5)) {
                }
                expected.Add(3d / 4);
                // No fourth step
            }
            // 4d / 4 is added here, despite no fourth step above
            expected.Add(4d / 4);
            Assert.Equal(expected, _actualProgress);
        }
        [Fact]
        public void NestedReportsAreWellBehaved()
        {
            using(var reporter = _progressReporter.Setup(4)) {
                reporter.Report(); // 1/4
                Recursive(_progressReporter, 2, 4); // [1/4 ... 2/4]
                reporter.Report(); // 3/4
            }
            var head = new[] { 0d, 0.25d };
            var tail = new[] { 0.5d, 0.75d, 1d };
            Assert.Equal(head, _actualProgress.Take(2));
            Assert.Equal(tail, _actualProgress.TakeLast(3));
            Assert.True(_actualProgress.Count() > head.Length + tail.Length, "Sanity check that there is an absurdly long middle range");
        }
    }
}