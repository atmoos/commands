using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progressTree;
using progressReporting;

using static progressTreeTest.Convenience;

namespace progressTreeTest
{
    public class ProgressTest : IExportedProgressTest
    {
        private readonly ProgressRecorder<Double> _actualProgress;

        public ProgressTest() => _actualProgress = new ProgressRecorder<Double>();

        [Fact]
        public void IterativeProgress()
        {
            const Int32 iterations = 8;
            var expected = ExpectedProgress(iterations);
            GenerateProgress(Progress.Create(_actualProgress), iterations);
            Assert.Equal(expected, _actualProgress);
        }

        [Fact]
        public void ReportsOnSubProgressAreNotScaled()
        {
            const Int32 childIterations = 4;
            const Int32 parentIterations = 8;
            var childReports = new List<IEnumerable<Double>>();
            var progressReporter = Progress.Create(_actualProgress);
            using(var parentReport = progressReporter.Schedule(parentIterations)) {
                for(Int32 p = 0; p < parentIterations; ++p) {
                    var subProgress = new ProgressRecorder<Double>();
                    using(var childReport = progressReporter.Schedule(childIterations, subProgress)) {
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
            var progressReporter = Progress.Create(_actualProgress);
            using(progressReporter.Schedule(4)) {
                expected.Add(0);
                using(var r = progressReporter.Schedule(3)) {
                    r.Report();
                    expected.Add(1d / (3 * 4));
                    // but forget 2/(3*4) etc...
                }
                expected.Add(1d / 4); // upon disposal
                using(progressReporter.Schedule(2)) {
                    using(progressReporter.Schedule(6)) {
                        // forget!
                    }
                    expected.Add(3d / 8); // upon disposal
                    using(var r = progressReporter.Schedule(8)) {
                        r.Report();
                        expected.Add(3d / 8 + 1d / (8 * 2 * 4));
                        r.Report();
                        expected.Add(3d / 8 + 2d / (8 * 2 * 4));
                        // but forget 3/(8*2*4) etc...
                    }
                    expected.Add(2d / 4); // upon disposal
                }
                using(progressReporter.Schedule(5)) {
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
            var progressReporter = Progress.Create(_actualProgress);
            using(var reporter = progressReporter.Schedule(4)) {
                reporter.Report(); // 1/4
                Report(progressReporter, 2, 4); // [1/4 ... 2/4]
                reporter.Report(); // 3/4
            }
            var head = new[] { 0d, 0.25d };
            var tail = new[] { 0.5d, 0.75d, 1d };
            Assert.Equal(head, _actualProgress.Take(2));
            Assert.Equal(tail, _actualProgress.TakeLast(3));
            Assert.True(_actualProgress.Count() > head.Length + tail.Length, "Sanity check that there is an absurdly long middle range");
        }
        [Fact]
        public void ExportedProgressIsScaled()
        {
            var input = new[] { 0, 0.2, 0.4, 0.6, 1 };
            var expected = new[] { 0, 0.1, 0.2, 0.3, 0.5, 1 };
            using(var r = Progress.Create(_actualProgress).Schedule(2)) {
                Report(r.Export(), input);
            }
            Assert.Equal(expected, _actualProgress);
        }

        [Fact]
        public void ExportedSubProgressIsScaled()
        {
            var input = new[] { 0, 0.2, 0.4, 0.8, 1 };
            var expected = new[] { 0, 0.05, 0.1, 0.2, 0.25, 1 };
            var actual = new ProgressRecorder<Double>();
            using(var r = Progress.Create(_actualProgress).Schedule(4, actual)) {
                Report(r.Export(), input);
            }
            Assert.Equal(expected, _actualProgress);
        }
        [Fact]
        public void ExportedProgressIsStrictlyMonotonic()
        {
            var input = new[] { 0.2, 0, 0.4, 0.3, 0.6, -1 };
            var expected = new[] { 0, 0.1, 0.2, 0.3, 1 };
            using(var r = Progress.Create(_actualProgress).Schedule(2)) {
                IProgress<Double> progress = r.Export();
                Report(progress, input);
            }
            Assert.Equal(expected, _actualProgress);
        }
        [Fact]
        public void OutOfBoundExportedProgressIsIgnored()
        {
            var input = new[] { -1.1, 0, 0.4, 0.1, 1.0, 1.2, 4.2 };
            var expected = new[] { 0, 0.2, 0.5, 1 };
            using(var r = Progress.Create(_actualProgress).Schedule(2)) {
                IProgress<Double> progress = r.Export();
                Report(progress, input);
            }
            Assert.Equal(expected, _actualProgress);
        }
        [Fact]
        public void LargeSymetricProgressTreesReportExpectedProgress()
        {
            // Powers of two prevent rounding errors.
            RunTreeComparison(64, 16, 2);
            RunTreeComparison(2, 4, 8, 16, 32, 8);
            RunTreeComparison(128, 128, 64);
        }
        [Fact]
        public void AsymmetricTrees()
        {
            Report(Progress.Create(_actualProgress),
                                            new[] { 2 },
                                            new[] { 4, 2 },
                                            new[] { 2 },
                                            new[] { 4 }
            );
            var expected1 = ExpectedProgress(0, 2, 0.25);
            var actual1 = _actualProgress.Section(0, 3);
            var expected2 = ExpectedProgress(0.25, 4 * 2, 0.5);
            var actual2 = _actualProgress.Section(2, 9);
            var expected3 = ExpectedProgress(0.5, 2, 0.75);
            var actual3 = _actualProgress.Section(10, 3);
            var expected4 = ExpectedProgress(0.75, 4, 1);
            var actual4 = _actualProgress.Section(12, 5);
            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
            Assert.Equal(expected4, actual4);
        }

        private static void RunTreeComparison(params Int32[] tree)
        {
            var actual = new ProgressRecorder<Double>();
            var expected = new ProgressRecorder<Double>();
            Report(Progress.Create(actual), expected, tree);
            Assert.Equal(expected, actual);
        }
    }
}