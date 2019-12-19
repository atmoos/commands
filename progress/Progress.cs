using System;
using System.Threading;

namespace progress
{
    public sealed class Progress
    {
        public static Progress Empty { get; } = new Progress(ProgressTree.Empty, EmptyProgress<State>.Empty);
        private ProgressTree _tree;
        private readonly IProgress<State> _stateReporter;
        private Progress(ProgressTree tree, IProgress<State> stateReporter)
        {
            _tree = tree;
            _stateReporter = stateReporter;
        }
        private void Set(ProgressTree tree) => Interlocked.Exchange(ref _tree, tree);
        private Reporter Reporter(ProgressTree tree)
        {
            Set(tree);
            return new Reporter(tree, Set);
        }
        public Reporter Setup(Int32 iterations)
        {
            var driver = ProgressDriver.Create(iterations);
            return Reporter(ProgressTree.Chain(_tree, driver));
        }
        public Reporter Setup(String subProcess, Int32 iterations)
        {
            var driver = ProgressDriver.Create(iterations);
            return Reporter(ProgressTree.Branch(_tree, driver, Monotonic(subProcess, _stateReporter)));
        }
        public Reporter Setup(TimeSpan expectedDuration)
        {
            var driver = ProgressDriver.Create(expectedDuration);
            return Reporter(ProgressTree.Chain(_tree, driver));
        }
        public Reporter Setup(String subProcess, TimeSpan expectedDuration)
        {
            var driver = ProgressDriver.Create(expectedDuration);
            return Reporter(ProgressTree.Branch(_tree, driver, Monotonic(subProcess, _stateReporter)));
        }
        public Reporter Setup<TProcess>(TProcess target, Func<TProcess> progressGetter, Func<TProcess, Double> linearization)
        {
            var driver = ProgressDriver.Create(target, progressGetter, linearization);
            return Reporter(ProgressTree.Chain(_tree, driver));
        }
        public static Progress Create(String process, IProgress<State> progress)
        {
            return new Progress(ProgressTree.Root(Monotonic(process, progress)), progress);
        }
        private static IProgress<Double> Monotonic(String process, IProgress<State> progress) => new MonotonicProgress(new ProcessReporter(process, progress));
    }
}