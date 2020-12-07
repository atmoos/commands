using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progressReporting;

namespace progressReportingTest
{
    public sealed class IncrementalProgressTest
    {
        [Fact]
        public void RootIsIncludedInProgressReport()
        {
            var input = new[] { 0, 2, 4 };
            var expected = new[] { 0, 4 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FirstValueCanBeLargerThanIncrement()
        {
            var input = new[] { 6, 10, 14 };
            var expected = new[] { 6, 10, 14 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SmallIncrementsAreFiltered()
        {
            var input = Enumerable.Range(0, 9);
            var expected = new[] { 0, 4, 8 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ValuesSmallerThanRootAreExcludedInProgressReport()
        {
            var input = new[] { -19283, -4, -3, -2, 0, 5 };
            var expected = new[] { 0, 5 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SuccessiveValuesAreExcludedInProgressReport()
        {
            var input = new[] { 0, 4, 4, 4, 4, 9 };
            var expected = new[] { 0, 4, 9 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ProgressReportIncreasesMonotonically()
        {
            var input = new[] { 5, 4, 10, -4, 20 };
            var expected = new[] { 5, 10, 20 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void NeighbouringValuesMayBeSmallerThanIncrementAsLongAsTheyFallIntoNeighbouringBins()
        {
            // Bins: [0 - 4[, [4 - 8[, [8 - 12[
            var input = new[] { 3, 5, 8 };
            var expected = new[] { 3, 5, 8 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void OnlyOneValuePerBinIsReported()
        {
            // Bins: [0 - 4[, [4 - 8[, [8 - 12[
            var input = new[] { 1, 2, 4, 5, 6, 9, 11 };
            var expected = new[] { 1, 4, 9 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LargeDistancesAreReported()
        {
            var input = new[] { 1, 50, 51, 100 };
            var expected = new[] { 1, 50, 100 };

            var actual = TestInt32(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WorksWithDoubles()
        {
            var increment = 0.25;
            var input = new[] { -3, -Double.Epsilon, 0.5, 1.9, 2.2, 2.3, 2.4, 60, 30, 60.3 };
            var expected = new[] { 0.5, 1.9, 2.2, 2.3, 60, 60.3 };

            var actual = TestDouble(increment, input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void WorksWithUInt16()
        {
            UInt16 increment = 32;
            var input = new UInt16[] { 3, 8, 32, 34, 50, 70 };
            var expected = new UInt16[] { 3, 32, 70 };

            var actual = TestUInt16(increment, input);

            Assert.Equal(expected, actual);
        }

        private static IEnumerable<Int32> TestInt32(IEnumerable<Int32> input) => Test<Int32>(4, Addition, 0, input);
        private static IEnumerable<UInt16> TestUInt16(UInt16 increment, IEnumerable<UInt16> input) => Test<UInt16>(increment, Addition, 0, input);
        private static IEnumerable<Double> TestDouble(Double increment, IEnumerable<Double> input) => Test<Double>(increment, Addition, 0d, input);
        private static IEnumerable<TProgress> Test<TProgress>(TProgress increment, Add<TProgress> add, TProgress root, IEnumerable<TProgress> input)
         where TProgress : unmanaged, IComparable<TProgress>
        {
            var recorder = new ProgressRecorder<TProgress>();
            var incremental = new IncrementalProgress<TProgress>(recorder, increment, add, root);
            foreach(var value in input) {
                incremental.Report(value);
            }
            return recorder;
        }

        private static Double Addition(in Double left, in Double right) => left + right;
        private static UInt16 Addition(in UInt16 left, in UInt16 right) => (UInt16)(left + right);
        private static Int32 Addition(in Int32 left, in Int32 right) => left + right;
    }
}