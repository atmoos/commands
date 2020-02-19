using System;

namespace progress.reporters
{
    internal sealed class DriverAdapter : IProgress<Double>, IDisposable
    {
        private readonly DriverAdapter _parent;
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _progress;
        private readonly Stack<DriverAdapter> _stack;
        private DriverAdapter(IProgress<Double> progress)
        {
            _parent = this;
            _progress = progress;
            _driver = ProgressDriver.Empty;
            _stack = new Stack<DriverAdapter>(this);
        }
        public DriverAdapter(Stack<DriverAdapter> stack, ProgressDriver driver, IProgress<Double> progress)
        {
            _stack = stack;
            _driver = driver;
            _progress = progress;
            _parent = stack.Push(this);
        }
        public void Report(Double value) => _progress.Report(_driver.Accumulate(value));
        public void Dispose() => _stack.Push(_parent);
        public static Stack<DriverAdapter> Root(IProgress<Double> root) => new DriverAdapter(root)._stack;
    }
}