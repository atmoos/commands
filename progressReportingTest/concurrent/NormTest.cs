using System;
using System.Linq;
using progressReporting;
using progressReporting.concurrent;
using Xunit;

namespace progressReportingTest.concurrent
{
    public sealed class NormTestTest
    {
        private static readonly Int32[] state = new[] { -12, 0, 1, -1, 282, 32, 4, -5, 7 };
        private readonly ProgressRecorder<Int32> actualProgress = new();

        [Fact]
        public void MinNorm_UpdatesTheSmallestValue_WhenTheCurrentStateContainsTheSmallestValue()
        {
            var smallest = state.Min();
            var minNorm = Norm.Min(this.actualProgress);

            minNorm.Update(smallest + 2, state);

            Assert.Single(this.actualProgress, v => v == smallest);
        }

        [Fact]
        public void MinNorm_UpdatesTheSmallestValue_WhenTheSmallestValueIsProvidedByTheUpdate()
        {
            var smallest = state.Min() - 2;
            var minNorm = Norm.Min(this.actualProgress);

            minNorm.Update(smallest, state);

            Assert.Single(this.actualProgress, v => v == smallest);
        }

        [Fact]
        public void MaxNorm_UpdatesTheLargestValue_WhenTheCurrentStateContainsTheLargestValue()
        {
            var largest = state.Max();
            var minNorm = Norm.Max(this.actualProgress);

            minNorm.Update(largest - 3, state);

            Assert.Single(this.actualProgress, v => v == largest);
        }

        [Fact]
        public void MaxNorm_UpdatesTheLargestValue_WhenTheLargestValueIsProvidedByTheUpdate()
        {
            var largest = state.Max() + 5;
            var minNorm = Norm.Max(this.actualProgress);

            minNorm.Update(largest, state);

            Assert.Single(this.actualProgress, v => v == largest);
        }
    }
}