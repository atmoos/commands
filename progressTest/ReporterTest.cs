using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progress;

namespace progressTest
{
    public sealed class ReporterTest
    {
        private IProgress<Double> _pushedProgress;
        private readonly Reporter _reporter;
        private readonly ProgressRecorder<Double> _progress;
        public ReporterTest()
        {
            _progress = new ProgressRecorder<Double>();
            _reporter = new Reporter(ProgressDriver.Create(3), _progress, Reset);
        }
        [Fact]
        public void ZeroIsReportedUponCreation()
        {
            Assert.Equal(0d, _progress.First());
        }
        [Fact]
        public void OneIsReportedUponDisposal()
        {
            _reporter.Dispose();
            Assert.Equal(1d, _progress.Last());
        }

        [Fact]
        public void ProgressIsPushedBackUponDisposal()
        {
            _reporter.Dispose();
            Assert.Same(_progress, _pushedProgress);
        }
        private void Reset(IProgress<Double> tree)
        {
            _pushedProgress = tree;
        }
    }
}