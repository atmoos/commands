using System;
using Xunit;
using progress.reporters;

namespace progressTest.reporters
{
    public sealed class MonotonicProgressTest
    {
        private static readonly Double[] Input = new Double[] { -1, 0, -2, 0, -3, 0, 1, -4, 2, -4, -1.5, 1, -4, -2, -5, 1.5, 3, -5, 3, 4, -6 };
        private readonly ProgressRecorder<Double> _actualSequence = new ProgressRecorder<Double>();
        [Fact]
        public void MonotonicIncreasingFiltersSequenceWeaklyIncreasing()
        {
            RunFilter(MonotonicProgress.Increasing(_actualSequence));
            Assert(-1, 0, 0, 0, 1, 2, 3, 3, 4);
        }
        [Fact]
        public void MonotonicStrictlyIncreasingFiltersSequenceStrictlyIncreasing()
        {
            RunFilter(MonotonicProgress.Strictly.Increasing(_actualSequence));
            Assert(-1, 0, 1, 2, 3, 4);
        }
        [Fact]
        public void MonotonicDecreasingFiltersSequenceWeaklyDecreasing()
        {
            RunFilter(MonotonicProgress.Decreasing(_actualSequence));
            Assert(-1, -2, -3, -4, -4, -4, -5, -5, -6);
        }
        [Fact]
        public void MonotonicStrictlyDereasingFiltersSequenceStrictlyDecreasing()
        {
            RunFilter(MonotonicProgress.Strictly.Decreasing(_actualSequence));
            Assert(-1, -2, -3, -4, -5, -6);
        }
        private void Assert(params Double[] expected) => Xunit.Assert.Equal(expected, _actualSequence);
        private static void RunFilter(IProgress<Double> progress)
        {
            foreach(Double value in Input) {
                progress.Report(value);
            }
        }

    }
}