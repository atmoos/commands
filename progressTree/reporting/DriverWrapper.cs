using System;

namespace progressTree.reporting
{
    internal sealed class DriverWrapper : IProgress<Double>
    {
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _progress;
        public DriverWrapper(ProgressDriver driver, IProgress<Double> progress)
        {
            this._driver = driver;
            this._progress = progress;
        }
        public void Report(Double value) => this._progress.Report(this._driver.Accumulate(value));
    }
}