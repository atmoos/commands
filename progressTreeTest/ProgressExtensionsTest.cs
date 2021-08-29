using System;
using progressReporting;
using progressTree;
using Xunit;

using static progressTreeTest.Convenience;

namespace progressTreeTest
{
    public sealed class ProgressExtensionsTest
    {
        private readonly ProgressRecorder<Double> actualProgress = new();

        [Fact]
        public void ConcurrentProgress_IsOnlyReportedFromTheSlowestInstance_ToReportProgress()
        {
            const Int32 reportCount = 8;
            var rootProgress = Progress.Create(this.actualProgress);

            using(var concurrentProgress = rootProgress.Concurrent(2)) {
                // reports from the first progress instance that is used are ignored
                var firstInstanceToReport = concurrentProgress[1];
                GenerateProgress(firstInstanceToReport, 2 * reportCount);
                // because overall progress can't be faster than the minimum of the last incoming report
                // Which in this case are all from the second instance that is used for reporting.
                var secondInstanceToReport = concurrentProgress[0];
                GenerateProgress(secondInstanceToReport, reportCount);
            }

            var expectedProgress = ExpectedProgress(reportCount);
            Assert.Equal(expectedProgress, this.actualProgress);
        }

        [Fact]
        public void ConcurrentProgress_IsOnlyReportedFromTheSmallestValueThatIsReported()
        {
            var rootProgress = Progress.Create(this.actualProgress);

            using(var concurrentProgress = rootProgress.Concurrent(2)) {
                var progressA = concurrentProgress[1]; // The order of
                var progressB = concurrentProgress[0]; // progress instances is irrelevant
                using(var reportFive = progressA.Schedule(5)) {
                    using(var reporterFour = progressB.Schedule(4)) {
                        reporterFour.Report(); // 0.25
                        reportFive.Report(); // 0.2!
                        reportFive.Report(); // 0.4, but 0.25!
                        reporterFour.Report(); // 0.5 skip, but 0.4 is now smallest!
                    }
                }
            }

            var expectedProgress = ExpectedProgress(0.2, 0.25, 0.4);
            Assert.Equal(expectedProgress, this.actualProgress);
        }
    }
}