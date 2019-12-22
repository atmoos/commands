using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progress;

namespace progressTest
{
    public sealed class ReporterTest
    {
        private ProgressTree _pushedTree;
        private readonly Reporter _reporter;
        private readonly ProgressTree _root;
        private readonly ProgressRecorder<Double> _progress;
        public ReporterTest()
        {
            _progress = new ProgressRecorder<Double>();
            _root = ProgressTree.Root(_progress);
            _reporter = new Reporter(_root, Reset);
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
        private void Reset(ProgressTree tree)
        {
            _pushedTree = tree;
        }
    }
}