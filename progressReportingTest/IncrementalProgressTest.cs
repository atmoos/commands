using System;
using System.Collections.Generic;
using System.Linq;
using progressReporting;
using Xunit;

namespace progressReportingTest;

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
    public void SuccessiveValuesAreExcludedInProgressReport()
    {
        var input = new[] { 0, 4, 4, 4, 4, 9 };
        var expected = new[] { 0, 4, 9 };

        var actual = TestInt32(input);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void ProgressReportIsOrderInsensitive()
    {
        var input = new[] { 0, 5, 4, 10, -4, 20, 10, 12, 16, 4 };
        var expected = new[] { 0, 5, 10, -4, 20, 10, 16, 4 };

        var actual = TestInt32(input);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void NeighbouringValuesMayBeSmallerThanIncrementAsLongAsTheyFallIntoNeighbouringBins()
    {
        // Bins: [0 - 4[, [4 - 8[, [8 - 12[
        var input = new[] { 0, 3, 5, 8 };
        var expected = new[] { 0, 5, 8 };

        var actual = TestInt32(input);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void OnlyOneValuePerBinIsReported()
    {
        // Bins: [0 - 4[, [4 - 8[, [8 - 12[
        var input = new[] { 0, 1, 2, 4, 5, 6, 9, 11 };
        var expected = new[] { 0, 4, 9 };

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
        const Double increment = 0.25;
        var input = new[] { 0.5, 0.55, 1.9, 2.2, 2.3, 2.4, 60, 60.3 };
        var expected = new[] { 0.5, 1.9, 2.2, 2.3, 60, 60.3 };

        var actual = TestDouble(increment, input);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WorksWithUInt16()
    {
        const UInt16 increment = 32;
        var input = new UInt16[] { 0, 3, 8, 32, 34, 50, 70, 4 };
        var expected = new UInt16[] { 0, 32, 70, 4 };

        var actual = TestUInt16(increment, input);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void WorksWithTimeSpan()
    {
        TimeSpan increment = TimeSpan.FromSeconds(32);
        var input = new[] { 0, 3, 8, 32, 34, 50, 70, 4 }.Select(f => TimeSpan.FromSeconds(f));
        var expected = new[] { 0, 32, 70, 4 }.Select(f => TimeSpan.FromSeconds(f));

        var actual = TestTimeSpan(increment, input);

        Assert.Equal(expected, actual);
    }

    private static IEnumerable<Int32> TestInt32(IEnumerable<Int32> input) => Test(p => p.Incremental(4), input);
    private static IEnumerable<UInt16> TestUInt16(UInt16 increment, IEnumerable<UInt16> input) => Test(p => p.Incremental(increment), input);
    private static IEnumerable<TimeSpan> TestTimeSpan(TimeSpan increment, IEnumerable<TimeSpan> input) => Test(p => p.Incremental(increment), input);
    private static IEnumerable<Double> TestDouble(Double increment, IEnumerable<Double> input) => Test(p => p.Incremental(increment), input);
    private static IEnumerable<TProgress> Test<TProgress>(Func<IProgress<TProgress>, IProgress<TProgress>> getIncrementalProgress, IEnumerable<TProgress> input)
    {
        var recorder = new ProgressRecorder<TProgress>();
        var incremental = getIncrementalProgress(recorder);
        foreach(var value in input) {
            incremental.Report(value);
        }
        return recorder;
    }
}