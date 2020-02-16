using System;
using progress.reporters;

namespace progress
{
    public sealed class Reporter : IDisposable
    {
        private readonly IDisposable _reset;
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _progress;
        internal Reporter(ProgressDriver driver, IProgress<Double> progress, IDisposable reset)
        {
            _reset = reset;
            _driver = driver;
            _progress = progress;
            _progress.Report(0);
        }
        public void Report() => _progress.Report(_driver.Advance());
        public IProgress<Double> Export() => MonotonicProgress.Strictly.Increasing(new DriverWrapper(_driver, _progress));
        public void Dispose()
        {
            _progress.Report(1d);
            _reset.Dispose();
        }
    }
}