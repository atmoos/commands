using System;

namespace progress.reporting
{
    internal sealed class DriverWrapper : IProgress<Double>
    {
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _progress;
        public DriverWrapper(ProgressDriver driver, IProgress<Double> progress)
        {
            _driver = driver;
            _progress = progress;
        }
        public void Report(Double value) => _progress.Report(_driver.Accumulate(value));
    }
}