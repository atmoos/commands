using System;
using System.Linq;
using System.Collections.Generic;
using progress;
using Xunit;
using System.Diagnostics;
using System.Threading;

namespace commandsTest
{
    public class ProgressTest
    {
        [Fact]
        public void IterativeProgress()
        {
            const Int32 iterations = 8;
            const String process = "test";
            var expected = ExpectedProgress(iterations);
            var report = new Reports();
            GenerateProgress(Progress.Create(process, report), iterations);
            Assert.Equal(expected, report.Progress[process]);
        }

        [Fact]
        public void ReportsOnChildProgress()
        {
            const Int32 childIterations = 4;
            const Int32 parentIterations = 8;
            const String childProcess = "child";
            const String parentProcess = "parent";
            var expected = ExpectedProgress(childIterations * parentIterations);
            var report = new Reports();
            var progress = Progress.Create(parentProcess, report);
            using(var parentReport = progress.Setup(parentIterations)) {
                for(Int32 p = 0; p < parentIterations; ++p) {
                    using(var childReport = progress.Setup($"{childProcess} #{p:N2}", childIterations)) {
                        for(Int32 c = 0; c < childIterations; ++c) {
                            childReport.Report();

                        }
                    }
                    parentReport.Report();
                }
            }
            Assert.Equal(expected, report.Progress[parentProcess]);
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

    internal class Reports : IProgress<State>
    {
        public Dictionary<String, List<Double>> Progress { get; }
        public Reports()
        {
            Progress = new Dictionary<String, List<Double>>();
        }
        public void Report(State value)
        {
            if(Progress.TryGetValue(value.Process, out List<Double> progress)) {
                progress.Add(value.Progress);
                return;
            }
            Progress[value.Process] = new List<Double>() { value.Progress };
        }
    }
}