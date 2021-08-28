using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using progressReporting;
using Xunit;

namespace progressReportingTest
{
    public sealed class ParallelProgressTest
    {
        private readonly IProgress<Int32> progA;
        private readonly IProgress<Int32> progB;
        private readonly IProgress<Int32> progC;
        private readonly IEnumerable<Int32> actualProgress;

        public ParallelProgressTest()
        {
            var targetProgress = new ProgressRecorder<Int32>();
            var progress = targetProgress.ForConcurrencyLevel(3).ToArray();

            this.progA = progress[0];
            this.progB = progress[1];
            this.progC = progress[2];
            this.actualProgress = targetProgress;
        }

        [Fact]
        public void ProgressIsReportedOnly_WhenMinimalValueIsIncreased()
        {
            this.progB.Report(default); // initial minimal value is reported
            this.progA.Report(2); // skip
            this.progC.Report(1); // skip
            this.progB.Report(1); // third time min is exceeded, it is reported
            this.progC.Report(4); // skip 
            this.progB.Report(2); // second time '2' is seen, it's now second to min -> must be reported

            Assert.Equal(Enumerable.Range(0, 3), this.actualProgress);
        }

        [Fact]
        public void DefaultValue_IsReportedEveryTime()
        {
            this.progB.Report(default); // initial minimal value is reported
            this.progC.Report(default); // every time
            this.progA.Report(default); // it occurs,
            this.progC.Report(default); // also when more than once per instance

            Assert.Equal(Enumerable.Repeat(0, 4), this.actualProgress);
        }

        [Fact]
        public void DefaultValue_DoesNotNeedToBeReported_ForProgressToIncrease()
        {
            this.progB.Report(4); // skip
            this.progC.Report(3); // skip
            this.progA.Report(1); // new smallest value -> report!,

            Assert.Equal(Enumerable.Repeat(1, 1), this.actualProgress);
        }

        [Fact]
        public void ProgressIsReported_WhenIncrementIsBetween_MinimalAndSecondToMinimalValue()
        {
            ReportAll(1); // minimal value is: 1 -> report!
            this.progC.Report(8); // skip
            this.progA.Report(6); // skip
            this.progB.Report(3); // new smallest value -> report!

            Assert.Equal(Enumerable.Repeat(1, 1).Append(3), this.actualProgress);
        }

        [Fact]
        public void ReportingTheSameValueMultipleTimes_DoesNotCauseProgressToBeReported()
        {
            ReportAll(2); // minimal value is: 2-> report!
            this.progA.Report(3); // skip
            this.progA.Report(3); // skip, as we're still reporting on A!
            this.progA.Report(4); // skip, as we're still reporting on A!
            this.progA.Report(3); // skip, as we're still reporting on A!
            this.progA.Report(5); // skip, as we're still reporting on A!
            this.progA.Report(3); // skip, as we're still reporting on A!
            // '3' has been reported multiple times on instance 'A', but 'B' and 'C' are still on '2' -> '3' is not reported!

            Assert.Equal(Enumerable.Repeat(2, 1), this.actualProgress);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ProgressIsReportedDirectlyToTarget_WhenConcurrencyLevelIsDegenerate(Int32 degenerateConcurrencyLevel)
        {
            var expectedValue = Guid.NewGuid();
            var expectedTargetProgress = new ProgressRecorder<Guid>();
            var parallelProgressInstances = expectedTargetProgress.ForConcurrencyLevel(degenerateConcurrencyLevel);

            var actualSingleWrappedParallelInstance = parallelProgressInstances.Single();
            actualSingleWrappedParallelInstance.Report(expectedValue);

            // All values will be reported no matter what, when degenerate concurrency occurs!
            Assert.Equal(new[] { expectedValue }, expectedTargetProgress);
            // implementation detail:
            Assert.Same(expectedTargetProgress, actualSingleWrappedParallelInstance);
        }

        [Fact]
        public async Task ReportsAreNotLost_WhenTargetProgressBlocks()
        {
            const Int32 reportCount = 3;
            const Int32 initialReport = 1;
            const Int32 concurrencyLevel = 2;

            static void Report(IProgress<Int32> progress, Int32 count)
            {
                foreach(var value in Enumerable.Range(initialReport, count)) {
                    progress.Report(value);
                }
            }

            var parentProgress = new ProgressRecorder<Int32>();
            var siblingRecorders = Enumerable.Range(0, concurrencyLevel).Select(_ => new ProgressRecorder<Int32>()).ToArray();
            using var blockingProgress = new BlockingProgress(parentProgress);
            var parallelReporters = blockingProgress.ForConcurrencyLevel(2).Select((p, i) => p.Zip(siblingRecorders[i])).ToArray();

            blockingProgress.Block();
            var parallelReports = parallelReporters.Select(p => Task.Run(() => Report(p, reportCount))).ToArray();

            var expectedParentProgress = Array.Empty<Int32>();
            var expectedSiblingProgress = new[] { initialReport };
            foreach(var siblingProgress in siblingRecorders) {
                Assert.Equal(expectedSiblingProgress, siblingProgress);
            }
            Assert.Equal(expectedParentProgress, parentProgress); // Sanity check
            blockingProgress.Unblock();
            await Task.WhenAll(parallelReports).ConfigureAwait(false);
        }

        private void ReportAll(Int32 value)
        {
            this.progA.Report(value);
            this.progB.Report(value);
            this.progC.Report(value);
        }

        private sealed class BlockingProgress : IProgress<Int32>, IDisposable
        {
            private readonly IProgress<Int32> parent;
            private readonly SemaphoreSlim block = new(1);
            public BlockingProgress(IProgress<Int32> parent) => this.parent = parent;
            public void Report(Int32 value)
            {
                Block();
                try {
                    this.parent.Report(value);
                } finally {
                    Unblock();
                }
            }
            public void Block() => this.block.Wait();
            public void Unblock() => this.block.Release();

            void IDisposable.Dispose() => this.block.Dispose();
        }
    }
}