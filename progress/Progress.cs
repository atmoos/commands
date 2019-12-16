using System;
using System.Threading;

namespace progress
{
    public sealed class Progress
    {
        private ProgressTree _tree;
        private readonly IProgress<State> _stateReporter;

        private Progress(ProgressTree tree, IProgress<State> stateReporter)
        {
            _tree = tree;
            _stateReporter = stateReporter;
        }
        private void Pop() => _tree = _tree.Parent;
        public Reporter Setup(Int32 iterations)
        {
            _tree = ProgressTree.Chain(_tree, 1d / iterations);
            return Reporter.Create(_tree, iterations, Pop);
        }
        public Reporter Setup(String subProcess, Int32 iterations)
        {
            _tree = ProgressTree.Branch(_tree, Monotonic(subProcess, _stateReporter), 1d / iterations);
            return Reporter.Create(_tree, iterations, Pop);
        }
        public Reporter Setup(TimeSpan expectedDuration) => null; // ToDo
        public Reporter Setup(String subProcess, TimeSpan expectedDuration) => null; // ToDo
        public static Progress Create(String process, IProgress<State> progress)
        {
            return new Progress(ProgressTree.Root(Monotonic(process, progress)), progress);
        }

        private static IProgress<Double> Monotonic(String process, IProgress<State> progress) => new MonotonicProgress(new ProcessReporter(process, progress));
    }
}