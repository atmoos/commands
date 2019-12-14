using System;
using System.Linq;
using System.Collections.Generic;
using commands.progress;
using Xunit;

namespace commandsTest
{
    public class ProgressTest
    {
        [Fact]
        public void Foo()
        {
            const String process = "test";
            const Int32 iterations = 8;
            var expected = Enumerable.Range(0, iterations + 1).Select(i => ((double)i)/iterations).ToList();
            var report = new Reports();
            Execute(Progress.Create(process, report), iterations);
            Assert.Equal(expected, report.Progress[process]);
        }

        private static void Execute(Progress progress, Int32 iterations)
        {
            using(Reporter reporter = progress.Setup(iterations))
            {
                foreach(var _ in Enumerable.Range(0, iterations)){
                    reporter.Report();
                }
            }
        }
    }

    internal class Reports : IProgress<State>
    {
        public Dictionary<String, List<Double>> Progress {get;}
        public Reports()
        {
            Progress = new Dictionary<String, List<Double>>();
        }
        public void Report(State value)
        {
            if(Progress.TryGetValue(value.Process, out List<Double> progress))
            {
                progress.Add(value.Progress);
                return;
            }
            Progress[value.Process] = new List<Double>(){value.Progress};
        }
    }
}