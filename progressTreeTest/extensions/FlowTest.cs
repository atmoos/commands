using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using progressReporting;
using progressTree;

using static progressTree.extensions.Flow;


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
        private static List<Double> ExpectedProgress(Int32 count)
        {
            Double max = count;
            return Enumerable.Range(0, count).Select(i => i / max).ToList();
        }
    }
}