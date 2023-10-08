using System;
using System.Linq;
using progressReporting;
using Xunit;

using static progressReporting.Extensions;

namespace progressReportingTest;

public sealed class ProgressMapTest
{
    [Fact]
    public void ProgressIsMappedToOtherType()
    {
        var actualProgress = new ProgressRecorder<Int32>();
        var mappedProgress = actualProgress.Map((UInt16 v) => (Int32)v);
        var expectedProgress = Enumerable.Range(3, 4).ToList();

        foreach(var value in expectedProgress) {
            mappedProgress.Report((UInt16)value);
        }

        Assert.Equal(expectedProgress, actualProgress);
    }

    [Fact]
    public void ProgressIsMappedBetweenValueAndReferenceTypes()
    {
        var actualProgress = new ProgressRecorder<String>();
        var mappedProgress = actualProgress.Map((Int32 v) => v.ToString());
        var expectedProgress = Enumerable.Range(3, 4).Select(v => v.ToString()).ToList();

        foreach(var value in expectedProgress) {
            mappedProgress.Report(Int32.Parse(value));
        }

        Assert.Equal(expectedProgress, actualProgress);
    }
}