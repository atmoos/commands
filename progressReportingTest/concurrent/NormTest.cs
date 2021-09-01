using System;
using System.Linq;
using progressReporting.concurrent;
using Xunit;

namespace progressReportingTest.concurrent
{
    public sealed class NormTestTest
    {
        private static readonly Int32[] state = new[] { -12, 0, 1, -1, 282, 32, 4, -5, 7 };

        [Fact]
        public void MinNorm_UpdatesTheSmallestValue_WhenTheCurrentStateContainsTheSmallestValue()
        {
            var smallest = state.Min();

            var norm = Norm.Min(smallest + 2, state);

            Assert.Equal(smallest, norm);
        }

        [Fact]
        public void MinNorm_UpdatesTheSmallestValue_WhenTheSmallestValueIsProvidedByTheUpdate()
        {
            var smallest = state.Min() - 2;

            var norm = Norm.Min(smallest, state);

            Assert.Equal(smallest, norm);
        }

        [Fact]
        public void MaxNorm_UpdatesTheLargestValue_WhenTheCurrentStateContainsTheLargestValue()
        {
            var largest = state.Max();

            var norm = Norm.Max(largest - 3, state);

            Assert.Equal(largest, norm);
        }

        [Fact]
        public void MaxNorm_UpdatesTheLargestValue_WhenTheLargestValueIsProvidedByTheUpdate()
        {
            var largest = state.Max() + 5;

            var norm = Norm.Max(largest, state);

            Assert.Equal(largest, norm);
        }
    }
}