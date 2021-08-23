using System;
using System.Collections.Generic;
using System.Linq;
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
            var progress = targetProgress.InParallel(3).ToArray();

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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ProgressIsReportedDirectlyToTarget_WhenConcurrencyLevelIsDegenerate(Int32 degenerateConcurrencyLevel)
        {
            var expectedValue = Guid.NewGuid();
            var actualProgress = new ProgressRecorder<Guid>();
            var parallelProgress = actualProgress.InParallel(degenerateConcurrencyLevel);

            parallelProgress.Single().Report(expectedValue);

            Assert.Equal(new[] { expectedValue }, actualProgress);
        }
    }
}