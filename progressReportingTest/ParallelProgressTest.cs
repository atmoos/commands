using System;
using System.Collections.Generic;
using System.Linq;
using progressReporting;
using Xunit;

namespace progressReportingTest
{
    public sealed class ParallelProgressTest
    {
        private readonly IProgress<Int32> progA;
        private readonly IProgress<Int32> progB;
        private readonly IProgress<Int32> progC;

        private readonly IEnumerable<Int32> actualProgress;

        public ParallelProgressTest()
        {
            var emptyProgress = Extensions.Empty<Int32>();
            var progressRecorder = new ProgressRecorder<Int32>();
            var progress = ParallelProgress<Int32>.Intercept(progressRecorder, Enumerable.Repeat(emptyProgress, 3)).ToArray();

            this.progA = progress[0];
            this.progB = progress[1];
            this.progC = progress[2];
            this.actualProgress = progressRecorder;
        }
        [Fact]
        public void Foo()
        {
            this.progB.Report(default);
            this.progA.Report(2); // skip
            this.progC.Report(1); // skip
            this.progB.Report(1);
            this.progC.Report(4); // skip 
            this.progB.Report(2); // should be reported now!

            Assert.Equal(Enumerable.Range(0, 3), this.actualProgress);
        }
    }
}