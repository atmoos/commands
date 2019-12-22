using System;
using System.Linq;
using System.Collections.Generic;
using progress;
using Xunit;
using System.Diagnostics;
using System.Threading;

namespace progressTest
{
    public class ProgressTest
    {
        [Fact]
        public void IterativeProgress()
        {
            const Int32 iterations = 8;
            var actual = new ProgressRecorder<Double>();
            var expected = ExpectedProgress(iterations);
            GenerateProgress(Progress.Create(actual), iterations);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReportsOnChildProgress()
        {
            const Int32 childIterations = 4;
            const Int32 parentIterations = 8;
            var childProgress = new List<IEnumerable<Double>>();
            var actualParentProgress = new ProgressRecorder<Double>();
            var progress = Progress.Create(actualParentProgress);
            using(var parentReport = progress.Setup(parentIterations)) {
                for(Int32 p = 0; p < parentIterations; ++p) {
                    var subProgress = new ProgressRecorder<Double>();
                    using(var childReport = progress.Setup(childIterations, subProgress)) {
                        childProgress.Add(subProgress);
                        for(Int32 c = 0; c < childIterations; ++c) {
                            childReport.Report();
                        }
                    }
                    parentReport.Report();
                }
            }
            Assert.Equal(ExpectedProgress(childIterations), childProgress[0]);
            Assert.Equal(ExpectedProgress(childIterations * parentIterations), actualParentProgress);
        }

        private static void GenerateProgress(Progress progress, Int32 iterations)
        {
            using(Reporter reporter = progress.Setup(iterations)) {
                foreach(var _ in Enumerable.Range(0, iterations)) {
                    reporter.Report();
                }
            }
        }

        private static void GenerateProgress(Progress progress, TimeSpan duration)
        {
            var timer = Stopwatch.StartNew();
            var intervals = TimeSpan.FromTicks(duration.Ticks / 32);
            using(Reporter reporter = progress.Setup(duration)) {
                while(timer.Elapsed < duration) {
                    Thread.Sleep(intervals);
                }
            }
        }

        private List<Double> ExpectedProgress(Int32 iterations) => Enumerable.Range(0, iterations + 1).Select(i => ((double)i) / iterations).ToList();
    }
}