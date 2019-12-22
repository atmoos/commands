using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using progress;

namespace progressTest
{
    public sealed class ProgressTreeTest
    {
        private readonly ProgressTree _root;
        private readonly ProgressRecorder<Double> _progress;

        public ProgressTreeTest()
        {
            _progress = new ProgressRecorder<Double>();
            _root = ProgressTree.Root(_progress);
        }
        [Fact]
        public void RootsParentIsSelf()
        {
            Assert.Same(_root, _root.Parent);
        }
        [Fact]
        public void ChainDoesNotReturnSelf()
        {
            Assert.NotSame(_root, _root.Chain(ProgressDriver.Empty));
        }
        [Fact]
        public void ChainedParentIsSelf()
        {
            var child = _root.Chain(ProgressDriver.Empty);
            Assert.Same(_root, child.Parent);
        }

        [Fact]
        public void BranchDoesNotReturnSelf()
        {
            Assert.NotSame(_root, _root.Branch(ProgressDriver.Empty, _progress));
        }
        [Fact]
        public void BranchedParentIsSelf()
        {
            var child = _root.Branch(ProgressDriver.Empty, _progress);
            Assert.Same(_root, child.Parent);
        }
        [Fact]
        public void ProgressOnRootIsUnfiltered()
        {
            var arguments = Enumerable.Range(0, 12).Select(arg => (Double)arg).ToList();
            Run(_root, arguments);
            Assert.Equal(arguments, _progress);
        }

        [Fact]
        public void ProgressOnChainIsUnfiltered()
        {
            var arguments = Enumerable.Range(0, 12).Select(arg => (Double)arg).ToList();
            Run(_root.Chain(ProgressDriver.Empty), arguments);
            Assert.Equal(arguments, _progress);
        }
        [Fact]
        public void ProgressOnBranchIsScaledByDriverUpToRootProgressReporter()
        {
            const Int32 iterations = 16;
            const Double scaleFactor = 1d / iterations;
            var arguments = Enumerable.Range(0, iterations).Select(arg => (Double)arg).ToList();
            var expectedReports = arguments.Select(arg => scaleFactor * arg).ToList();
            Run(_root.Branch(ProgressDriver.Create(iterations), EmptyProgress<Double>.Empty), arguments);
            Assert.Equal(expectedReports, _progress);
        }
        [Fact]
        public void SubProgressOnBranchIsUnchanged()
        {
            const Int32 iterations = 16;
            var arguments = Enumerable.Range(0, iterations).Select(arg => (Double)arg).ToList();
            var subProgress = new ProgressRecorder<Double>();
            Run(_root.Branch(ProgressDriver.Create(iterations), subProgress), arguments);
            Assert.Equal(arguments, subProgress);
        }
        private static void Run(IProgress<Double> progress, IEnumerable<Double> parameters)
        {
            foreach(var parameter in parameters) {
                progress.Report(parameter);
            }
        }
    }
}