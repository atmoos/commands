using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using Xunit;
using progressReporting;
using progressTree;

using ReporterFlow = progressTree.extensions.ReporterFlow;

using static progressTree.extensions.Flow;
using static progressTreeTest.Convenience;

namespace progressTreeTest.extensions
{
    public sealed class FlowTest
    {
        [Fact]
        public void EnumeratePreservesCollection()
        {
            var original = Enumerable.Range(0, 16).ToList();
            Assert.Equal(original, Progress.Empty.Enumerate(original, CancellationToken.None));
        }
        [Fact]
        public void EnumerateReportsExpectedProgressDatums()
        {
            const Int32 count = 4;
            var recorder = new ProgressRecorder<Double>();
            var intervals = Enumerable.Range(0, count).Select(i => i / (Double)count).ToList();
            Progress.Create(recorder).Enumerate(intervals, CancellationToken.None).ToList();
            var expected = intervals.Append(1);
            Assert.Equal(expected, recorder);
        }
        [Fact]
        public void EnumerateCanBeCancelled()
        {
            Int32 successCount = 2;
            var recorder = new ProgressRecorder<Double>();
            using(var cts = new CancellationTokenSource()) {
                var actions = Enumerable.Repeat<Action>(() => { }, successCount - 1).ToList();
                actions.Add(cts.Cancel);
                actions.AddRange(Enumerable.Repeat<Action>(() => { }, successCount));
                var expectedProgress = actions.Take(successCount + 1).Select((_, i) => i / (Double)actions.Count).Append(1);
                Assert.Throws<OperationCanceledException>(() =>
                {
                    foreach(Action action in Progress.Create(recorder).Enumerate(actions, cts.Token)) {
                        action();
                    }
                });
                Assert.Equal(expectedProgress, recorder);
            }
        }
        [Fact]
        public void FlowGeneratesTooManyReportsWhenUsedOnCollectionsOf_IReportProgress()
        {
            const Int32 count = 4;
            const Int32 subCount = 2;
            var reporters = Enumerable.Range(0, count).Select(_ => new Reporter(subCount)).ToList();
            var actual = Enumerate(reporters, (p, c) => p.Enumerate(c, CancellationToken.None));
            var expected = ExpectedProgress(count * subCount).ToList();
            Assert.NotEqual(expected, actual);
        }
        [Fact]
        public void ReporterFlowGeneratesExpectedReportsOnCollectionsOf_IReportProgress()
        {
            const Int32 count = 2;
            const Int32 subCount = 4;
            var reporters = Enumerable.Range(0, count).Select(_ => new Reporter(subCount)).ToList();
            var actual = Enumerate(reporters, (p, c) => ReporterFlow.Enumerate(p, c, CancellationToken.None));
            var expected = ExpectedProgress(count * subCount).ToList();
            Assert.Equal(expected, actual);
        }

        private IEnumerable<Double> Enumerate(ICollection<Reporter> sequence, Func<Progress, ICollection<Reporter>, IEnumerable<Reporter>> enumerate)
        {
            var recorder = new ProgressRecorder<Double>();
            Progress p = Progress.Create(recorder);
            foreach(var reporter in enumerate(p, sequence)) {
                reporter.Execute(p);
            }
            return recorder;
        }

        private sealed class Reporter : IReportProgress
        {
            private readonly Int32 _count;
            public Reporter(Int32 count) => this._count = count;
            public void Execute(Progress progress)
            {
                using(var r = progress.Schedule(this._count)) {
                    foreach(var _ in Enumerable.Range(0, this._count)) {
                        r.Report();
                    }
                }
            }
        }
    }
}