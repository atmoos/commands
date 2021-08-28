using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using progressReporting;
using progressTree;
using Xunit;
using static progressTreeTest.Convenience;

namespace progressTreeTest
{
    public class ProgressTest : IExportedProgressTest
    {
        private readonly ProgressRecorder<Double> actualProgress;

        public ProgressTest() => this.actualProgress = new ProgressRecorder<Double>();

        [Fact]
        public void IterativeProgress()
        {
            const Int32 iterations = 8;
            var expected = ExpectedProgress(iterations);
            GenerateProgress(Progress.Create(this.actualProgress), iterations);
            Assert.Equal(expected, this.actualProgress);
        }

        [Fact]
        public void ReportsOnSubProgressAreNotScaled()
        {
            const Int32 childIterations = 4;
            const Int32 parentIterations = 8;
            var childReports = new List<IEnumerable<Double>>();
            var progressReporter = Progress.Create(this.actualProgress);
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
            Assert.Equal(ExpectedProgress(childIterations * parentIterations), this.actualProgress);
        }
        [Fact]
        public void ScheduledReportsAreStable()
        {
            List<Double> expected = new List<Double>();
            var progressReporter = Progress.Create(this.actualProgress);
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
                        expected.Add((3d / 8) + (1d / (8 * 2 * 4)));
                        r.Report();
                        expected.Add((3d / 8) + (2d / (8 * 2 * 4)));
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
            Assert.Equal(expected, this.actualProgress);
        }
        [Fact]
        public void NestedReportsAreWellBehaved()
        {
            var progressReporter = Progress.Create(this.actualProgress);
            using(var reporter = progressReporter.Schedule(4)) {
                reporter.Report(); // 1/4
                Report(progressReporter, 2, 4); // [1/4 ... 2/4]
                reporter.Report(); // 3/4
            }
            var head = new[] { 0d, 0.25d };
            var tail = new[] { 0.5d, 0.75d, 1d };
            Assert.Equal(head, this.actualProgress.Take(2));
            Assert.Equal(tail, this.actualProgress.TakeLast(3));
            Assert.True(this.actualProgress.Count() > head.Length + tail.Length, "Sanity check that there is an absurdly long middle range");
        }
        [Fact]
        public void ExportedProgressIsScaled()
        {
            var input = new[] { 0, 0.2, 0.4, 0.6, 1 };
            var expected = new[] { 0, 0.1, 0.2, 0.3, 0.5, 1 };
            using(var r = Progress.Create(this.actualProgress).Schedule(2)) {
                Report(r.Export(), input);
            }
            Assert.Equal(expected, this.actualProgress);
        }

        [Fact]
        public void ExportedSubProgressIsScaled()
        {
            var input = new[] { 0, 0.2, 0.4, 0.8, 1 };
            var expected = new[] { 0, 0.05, 0.1, 0.2, 0.25, 1 };
            var actual = new ProgressRecorder<Double>();
            using(var r = Progress.Create(this.actualProgress).Schedule(4, actual)) {
                Report(r.Export(), input);
            }
            Assert.Equal(expected, this.actualProgress);
        }
        [Fact]
        public void ExportedProgressIsStrictlyMonotonic()
        {
            var input = new[] { 0.2, 0, 0.4, 0.3, 0.6, -1 };
            var expected = new[] { 0, 0.1, 0.2, 0.3, 1 };
            using(var r = Progress.Create(this.actualProgress).Schedule(2)) {
                IProgress<Double> progress = r.Export();
                Report(progress, input);
            }
            Assert.Equal(expected, this.actualProgress);
        }
        [Fact]
        public void OutOfBoundExportedProgressIsIgnored()
        {
            var input = new[] { -1.1, 0, 0.4, 0.1, 1.0, 1.2, 4.2 };
            var expected = new[] { 0, 0.2, 0.5, 1 };
            using(var r = Progress.Create(this.actualProgress).Schedule(2)) {
                IProgress<Double> progress = r.Export();
                Report(progress, input);
            }
            Assert.Equal(expected, this.actualProgress);
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
            Report(Progress.Create(this.actualProgress),
                                            new[] { 2 },
                                            new[] { 4, 2 },
                                            new[] { 2 },
                                            new[] { 4 }
            );
            var expected1 = ExpectedProgress(0, 2, 0.25);
            var actual1 = this.actualProgress.Section(0, 3);
            var expected2 = ExpectedProgress(0.25, 4 * 2, 0.5);
            var actual2 = this.actualProgress.Section(2, 9);
            var expected3 = ExpectedProgress(0.5, 2, 0.75);
            var actual3 = this.actualProgress.Section(10, 3);
            var expected4 = ExpectedProgress(0.75, 4, 1);
            var actual4 = this.actualProgress.Section(12, 5);
            Assert.Equal(expected1, actual1);
            Assert.Equal(expected2, actual2);
            Assert.Equal(expected3, actual3);
            Assert.Equal(expected4, actual4);
        }

        [Fact]
        public void NoProgressIsReportedWhenEmptyProgressIsUsed()
        {
            var progress = Progress.Empty;
            using(var reporter = progress.Schedule(3, this.actualProgress)) {
                reporter.Report();
                reporter.Report();
            }
            Assert.Empty(this.actualProgress);
        }

        [Fact]
        public void TopLevelProgressIsBounded()
        {
            const Int32 expectedSteps = 3;
            const Int32 oneStepTooMany = expectedSteps + 1;
            var expectedProgress = ExpectedProgress(expectedSteps);
            var progress = Progress.Create(this.actualProgress);

            using(var reporter = progress.Schedule(expectedSteps)) {
                foreach(var _ in Enumerable.Range(0, oneStepTooMany)) {
                    reporter.Report();
                }
            }
            Assert.Equal(expectedProgress, this.actualProgress);
        }

        [Fact]
        public void SubLevelProgressIsBounded()
        {
            const Int32 expectedSteps = 4;
            const Int32 oneStepTooMany = expectedSteps + 1;
            var expectedProgress = ExpectedProgress(expectedSteps);
            var progress = Progress.Create(Extensions.Empty<Double>());

            using(var reporter = progress.Schedule(3)) {
                reporter.Report();
                using(var subReporter = progress.Schedule(expectedSteps, this.actualProgress)) {
                    foreach(var _ in Enumerable.Range(0, oneStepTooMany)) {
                        subReporter.Report();
                    }
                }
                reporter.Report();
            }
            Assert.Equal(expectedProgress, this.actualProgress);
        }

        [Fact]
        public async Task OneIsReportedUponDisposalOfTemporalReporting()
        {
            var progress = Progress.Create(this.actualProgress);
            using(progress.Schedule(TimeSpan.MaxValue)) {
                await Task.Yield();
            }
            Assert.Contains(1d, this.actualProgress);
        }

        [Fact]
        public async Task OneIsReportedUponDisposalOfTemporalReportingWhenReportComesDelayed()
        {
            var progress = Progress.Create(this.actualProgress);
            var scheduledTime = TimeSpan.FromMilliseconds(8);
            using(var r = progress.Schedule(scheduledTime)) {
                await Task.Delay(1.4 * scheduledTime).ConfigureAwait(false);
                r.Report();
            }
            Assert.Contains(1d, this.actualProgress);
        }

        [Fact]
        public async Task OneIsReportedUponDisposalOfTemporalReportingOnSubProgressWhenReportComesDelayed()
        {
            var scheduledTime = TimeSpan.FromMilliseconds(8);
            using(var r = Progress.Create(Extensions.Empty<Double>()).Schedule(scheduledTime, this.actualProgress)) {
                await Task.Delay(1.4 * scheduledTime).ConfigureAwait(false);
                r.Report();
            }
            Assert.Contains(1d, this.actualProgress);
        }

        [Fact]
        public void OneIsReportedUponDisposalOfIncrementalReportingOnSubProgressWhenReportComesDelayed()
        {
            const int steps = 2;
            using(var r = Progress.Create(Extensions.Empty<Double>()).Schedule(steps, this.actualProgress)) {
                foreach(var _ in Enumerable.Range(0, steps + 1)) {
                    r.Report();
                }
            }
            Assert.Contains(1d, this.actualProgress);
        }

        [Fact]
        public void ConcurrentProgress_IsOnlyReportedFromTheSlowestInstance_ToReportProgress()
        {
            const Int32 reportCount = 8;
            var rootProgress = Progress.Create(this.actualProgress);

            using(var concurrentProgress = rootProgress.Concurrent(2)) {
                // reports from the first progress instance that is used are ignored
                var firstInstanceToReport = concurrentProgress[1];
                GenerateProgress(firstInstanceToReport, 6);
                // because overall progress can't be faster than the minimum of the last incoming report
                // Which in this case are all from the second instance that is used for reporting.
                var secondInstanceToReport = concurrentProgress[1];
                GenerateProgress(secondInstanceToReport, reportCount);
            }

            var expectedProgress = ExpectedProgress(reportCount);
            Assert.Equal(expectedProgress, this.actualProgress);
        }

        [Fact]
        public void ConcurrentProgress_IsOnlyReportedFromTheSmallestValueThatIsReported()
        {
            const Int32 reportCount = 8;
            var rootProgress = Progress.Create(this.actualProgress);

            using(var concurrentProgress = rootProgress.Concurrent(2)) {
                // reports from the first progress instance that is used are ignored
                var firstInstanceToReport = concurrentProgress[1];
                GenerateProgress(firstInstanceToReport, 6);
                // because overall progress can't be faster than the minimum of the last incoming report
                // Which in this case are all from the second instance that is used for reporting.
                var secondInstanceToReport = concurrentProgress[1];
                GenerateProgress(secondInstanceToReport, reportCount);
            }

            var expectedProgress = ExpectedProgress(reportCount);
            Assert.Equal(expectedProgress, this.actualProgress);
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