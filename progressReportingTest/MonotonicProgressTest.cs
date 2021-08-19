using System;
using System.Collections.Generic;
using progressReporting;
using Xunit;

namespace progressReportingTest
{
    public sealed class MonotonicProgressTest
    {
        private static readonly Double[] Input = new Double[] { -1, 0, -2, 0, -3, 0, 1, -4, 2, -4, -1.5, 1, -4, -2, -5, 1.5, 3, -5, 3, 4, -6 };

        public interface IMonotonicTest
        {
            void MonotonicDecreasingFiltersSequenceWeaklyDecreasing();
            void MonotonicIncreasingFiltersSequenceWeaklyIncreasing();
            void MonotonicStrictlyDereasingFiltersSequenceStrictlyDecreasing();
            void MonotonicStrictlyIncreasingFiltersSequenceStrictlyIncreasing();
        }

        public sealed class OnDouble : IMonotonicTest
        {
            private readonly ProgressRecorder<Double> actualSequence = new ProgressRecorder<Double>();

            [Fact]
            public void MonotonicIncreasingFiltersSequenceWeaklyIncreasing()
            {
                RunFilter(this.actualSequence.Monotonic().Increasing());
                Assert(this.actualSequence, -1, 0, 0, 0, 1, 2, 3, 3, 4);
            }
            [Fact]
            public void MonotonicStrictlyIncreasingFiltersSequenceStrictlyIncreasing()
            {
                RunFilter(this.actualSequence.Monotonic().Strictly.Increasing());
                Assert(this.actualSequence, -1, 0, 1, 2, 3, 4);
            }
            [Fact]
            public void MonotonicDecreasingFiltersSequenceWeaklyDecreasing()
            {
                RunFilter(this.actualSequence.Monotonic().Decreasing());
                Assert(this.actualSequence, -1, -2, -3, -4, -4, -4, -5, -5, -6);
            }
            [Fact]
            public void MonotonicStrictlyDereasingFiltersSequenceStrictlyDecreasing()
            {
                RunFilter(this.actualSequence.Monotonic().Strictly.Decreasing());
                Assert(this.actualSequence, -1, -2, -3, -4, -5, -6);
            }
        }
        public sealed class OnGeneric : IMonotonicTest
        {
            private readonly ProgressRecorder<Double> actualSequence = new ProgressRecorder<Double>();

            [Fact]
            public void MonotonicIncreasingFiltersSequenceWeaklyIncreasing()
            {
                RunFilter(this.actualSequence.Monotonic<Double>().Increasing());
                Assert(this.actualSequence, -1, 0, 0, 0, 1, 2, 3, 3, 4);
            }
            [Fact]
            public void MonotonicStrictlyIncreasingFiltersSequenceStrictlyIncreasing()
            {
                RunFilter(this.actualSequence.Monotonic<Double>().Strictly.Increasing());
                Assert(this.actualSequence, -1, 0, 1, 2, 3, 4);
            }
            [Fact]
            public void MonotonicDecreasingFiltersSequenceWeaklyDecreasing()
            {
                RunFilter(this.actualSequence.Monotonic<Double>().Decreasing());
                Assert(this.actualSequence, -1, -2, -3, -4, -4, -4, -5, -5, -6);
            }
            [Fact]
            public void MonotonicStrictlyDereasingFiltersSequenceStrictlyDecreasing()
            {
                RunFilter(this.actualSequence.Monotonic<Double>().Strictly.Decreasing());
                Assert(this.actualSequence, -1, -2, -3, -4, -5, -6);
            }
        }
        private static void Assert(IEnumerable<Double> actual, params Double[] expected) => Xunit.Assert.Equal(expected, actual);
        private static void RunFilter(IProgress<Double> progress)
        {
            foreach(Double value in Input) {
                progress.Report(value);
            }
        }
    }
}