using System;
using progress.reporters;

namespace progress
{
    public sealed class Reporter : IDisposable
    {
        private readonly Reporter _parent;
        private readonly Stack<Reporter> _stack;
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _wrapper;
        private readonly IProgress<Double> _progress;
        internal IProgress<Double> Progress => _wrapper;
        private Reporter(IProgress<Double> progress)
        {
            _parent = this;
            _driver = ProgressDriver.Empty;
            _wrapper = _progress = progress;
            _stack = new Stack<Reporter>(this);
        }

        internal Reporter(Stack<Reporter> stack, ProgressDriver driver, IProgress<Double> progress)
        {
            _stack = stack;
            _driver = driver;
            progress.Report(0);
            _progress = progress;
            _parent = stack.Push(this);
            _wrapper = new DriverWrapper(_driver, progress);
        }
        public void Report() => _progress.Report(_driver.Advance());
        public IProgress<Double> Export() => MonotonicProgress.Strictly.Increasing(_wrapper);
        public void Dispose()
        {
            _stack.Push(_parent);
            _progress.Report(1d);
            _parent.Report();
        }
        internal static Stack<Reporter> Root(IProgress<Double> root) => new Reporter(root)._stack;
    }
}