using System;
using progressReporting;
using progressTree.reporting;

namespace progressTree
{
    public sealed class Reporter : IDisposable
    {
        private readonly Reporter _parent;
        private readonly Stack<Reporter> _stack;
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _rootProgress;
        internal IProgress<Double> Progress { get; }
        private Reporter(IProgress<Double> progress)
        {
            _parent = this;
            _driver = ProgressDriver.Create(1);
            _stack = new Stack<Reporter>(this);
            Progress = _rootProgress = progress;
        }
        internal Reporter(Stack<Reporter> stack, ProgressDriver driver, IProgress<Double> progress)
        {
            _stack = stack;
            _driver = driver;
            progress.Report(0);
            _rootProgress = progress;
            _parent = stack.Push(this);
            Progress = new DriverWrapper(_driver, progress);
        }
        public void Report()
        {
            Double progress = _driver.Advance();
            if(progress <= 1d) {
                _rootProgress.Report(progress);
            }
        }
        public IProgress<Double> Export() => MonotonicProgress.Strictly.Increasing(Progress);
        public void Dispose()
        {
            _stack.Push(_parent);
            _parent.Report();
        }
        internal static Stack<Reporter> Root(IProgress<Double> root) => new Reporter(root)._stack;
    }
}