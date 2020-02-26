using System;
using progressReporting;

namespace progressTree.reporting
{
    internal sealed class DriverWrapper : IProgress<Double>
    {
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _progress;
        public DriverWrapper(ProgressDriver driver, IProgress<Double> progress)
        {
            _driver = driver;
            _progress = progress.Bounded(0, 1).Inclusive();
        }
        public void Report(Double value) => _progress.Report(_driver.Accumulate(value));
    }
}