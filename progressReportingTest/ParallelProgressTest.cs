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
            var emptyProgress = Extensions.Empty<Int32>();
            var targetProgress = new ProgressRecorder<Int32>();
            var progress = ParallelProgress<Int32>.Intercept(targetProgress, Enumerable.Repeat(emptyProgress, 3)).ToArray();

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
        public void ProgressIsReportedDirectlyToTarget_WhenNoChildElementsArePresent()
        {
            var expectedValue = Guid.NewGuid();
            var actualProgress = new ProgressRecorder<Guid>();
            var parallelProgress = ParallelProgress<Guid>.Intercept(actualProgress, Array.Empty<IProgress<Guid>>());

            parallelProgress.Single().Report(expectedValue);

            Assert.Equal(new[] { expectedValue }, actualProgress);
        }

        [Fact]
        public void ProgressIsReportedDirectlyToBothTargetAndChild_WhenOnlyOneChildElementsArePresent()
        {
            var expectedValue = Guid.NewGuid();
            var childProgress = new ProgressRecorder<Guid>();
            var targetProgress = new ProgressRecorder<Guid>();
            var parallelProgress = ParallelProgress<Guid>.Intercept(targetProgress, new[] { childProgress });

            parallelProgress.Single().Report(expectedValue);

            var expectedProgress = new[] { expectedValue };
            Assert.Equal(expectedProgress, targetProgress);
            Assert.Equal(expectedProgress, childProgress);
        }
    }
}