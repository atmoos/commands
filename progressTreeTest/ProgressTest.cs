using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progressTree;
using progressReporting;

using static progressTreeTest.Convenience;

namespace progressTreeTest
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
        public void ReportsOnSubProgressAreNotScaled()
        {
            const Int32 childIterations = 4;
            const Int32 parentIterations = 8;
            var childReports = new List<IEnumerable<Double>>();
            using(var parentReport = _progressReporter.Schedule(parentIterations)) {
                for(Int32 p = 0; p < parentIterations; ++p) {
                    var subProgress = new ProgressRecorder<Double>();
                    using(var childReport = _progressReporter.Schedule(childIterations, subProgress)) {
                        childReports.Add(subProgress);
                        for(Int32 c = 0; c < childIterations; ++c) {
                            childReport.Report();
                        }
                    }
                }
            }
            var expectedSubReport = ExpectedProgress(childIterations).ToList();
            foreach(var subReport in childReports) {
                Assert.Equal(expectedSubReport, subReport);
            }
            Assert.Equal(ExpectedProgress(childIterations * parentIterations), _actualProgress);
        }
        [Fact]
        public void ScheduledReportsAreStable()
        {
            List<Double> expected = new List<Double>();
            using(_progressReporter.Schedule(4)) {
                expected.Add(0);
                using(var r = _progressReporter.Schedule(3)) {
                    r.Report();
                    expected.Add(1d / (3 * 4));
                    // but forget 2/(3*4) etc...
                }
                expected.Add(1d / 4); // upon disposal
                using(_progressReporter.Schedule(2)) {
                    using(_progressReporter.Schedule(6)) {
                        // forget!
                    }
                    expected.Add(3d / 8); // upon disposal
                    using(var r = _progressReporter.Schedule(8)) {
                        r.Report();
                        expected.Add(3d / 8 + 1d / (8 * 2 * 4));
                        r.Report();
                        expected.Add(3d / 8 + 2d / (8 * 2 * 4));
                        // but forget 3/(8*2*4) etc...
                    }
                    expected.Add(2d / 4); // upon disposal
                }
                using(_progressReporter.Schedule(5)) {
                    // forget!
                }
                expected.Add(3d / 4); // upon disposal
                // forget reporting 4/4
            }
            expected.Add(4d / 4); // upon disposal
            Assert.Equal(expected, _actualProgress);
        }
        [Fact]
        public void NestedReportsAreWellBehaved()
        {
            using(var reporter = _progressReporter.Schedule(4)) {
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