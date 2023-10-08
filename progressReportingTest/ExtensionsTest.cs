using System;
using System.Linq;
using System.Threading;
using progressReporting;
using Xunit;
using static progressReportingTest.Convenience;

namespace progressReportingTest;

public sealed class ExtensionsTest
{
    [Fact]
    public void ZipReportsEqualReportsToBothProgressInstances()
    {
        var progressA = new ProgressRecorder<Int32>();
        var progressB = new ProgressRecorder<Int32>();
        Report(((IProgress<Int32>)progressA).Zip(progressB), RandomIntegers());
        Assert.Equal(progressA, progressB);
    }
    [Fact]
    public void CallingCancellableCancelsCancellableProgress()
    {
        using(var cts = new CancellationTokenSource()) {
            cts.Cancel();
            var cancellableProgress = Extensions.Empty<String>().Cancellable(cts.Token);
            Assert.Throws<OperationCanceledException>(() => cancellableProgress.Report("Hi!"));
        }
    }
    [Fact]
    public void NotCancellingCancellableProgressLeavesPrimaryReportingUnchanged()
    {
        var recorder = new ProgressRecorder<Int32>();
        var cancellable = recorder.Cancellable(CancellationToken.None);
        Assert.Equal(Report(cancellable, RandomIntegers()).Take(12).ToList(), recorder);
    }
    [Fact]
    public void ObservableProgressPreservesReports()
    {
        const String expected = "Observable!";
        var recorder = new ProgressRecorder<Char>();
        var observable = Extensions.Empty<Char>().Observable(recorder.Report);
        Assert.Equal(Report(observable, expected).ToArray(), recorder);
    }
    [Fact]
    public void EmptyProgressAlwaysReturnsTheSameInstance()
    {
        Assert.Same(Extensions.Empty<Byte>(), Extensions.Empty<Byte>());
    }
    [Fact]
    public void EmptyProgressOfDifferenctTypeReturnsSeparateInstances()
    {
        // This test is mostly fun to see it work.
        // After all, it's simply a feature of .Net's generics...
        Assert.NotSame(Extensions.Empty<Char>(), Extensions.Empty<Uri>());
    }
}