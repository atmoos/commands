using System;

namespace progress
{
    public sealed class Reporter : IDisposable
    {
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _parent;
        private readonly IProgress<Double> _progress;
        private readonly Action<IProgress<Double>> _reset;
        internal Reporter(ProgressDriver driver, IProgress<Double> progress, Action<IProgress<Double>> reset)
            : this(driver, progress, progress, reset)
        {

        }
        internal Reporter(ProgressDriver driver, IProgress<Double> progress, IProgress<Double> parent, Action<IProgress<Double>> reset)
        {
            _reset = reset;
            _driver = driver;
            _parent = parent;
            _progress = progress;
            _progress.Report(0);
        }
        public void Report() => _progress.Report(_driver.Advance());
        public void Dispose()
        {
            _progress.Report(1d);
            _reset(_parent);
        }
    }
}