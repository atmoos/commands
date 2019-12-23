using System;

namespace progress.reporters
{
    internal sealed class DriverAdapter : IProgress<Double>
    {
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _progress;
        public DriverAdapter(ProgressDriver driver, IProgress<Double> progress)
        {
            _driver = driver;
            _progress = progress;
        }
        public void Report(Double value)
        {
            _progress.Report(_driver.Accumulate(value));
        }
    }
}