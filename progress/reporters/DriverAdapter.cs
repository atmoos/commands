using System;

namespace progress.reporters
{
    internal sealed class DriverAdapter : IProgress<Double>, IDisposable
    {
        private readonly ProgressDriver _driver;
        private readonly DriverAdapter _parentDriver;
        private readonly IProgress<Double> _progress;
        private readonly Stack<DriverAdapter> _stack;
        private DriverAdapter(IProgress<Double> progress)
        {
            _parentDriver = this;
            _progress = progress;
            _driver = ProgressDriver.Empty;
            _stack = new Stack<DriverAdapter>(this);
        }
        public DriverAdapter(ProgressDriver driver, Stack<DriverAdapter> stack, IProgress<Double> progress)
        {
            _stack = stack;
            _driver = driver;
            _progress = progress;
            _parentDriver = _stack.Push(this);
        }
        public void Report(Double value) => _progress.Report(_driver.Accumulate(value));
        public void Dispose() => _stack.Push(_parentDriver);

        public static Stack<DriverAdapter> Root(IProgress<Double> root)
        {
            var rootDriver = new DriverAdapter(root);
            return rootDriver._stack;
        }
    }
}