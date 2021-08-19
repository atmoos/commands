using System;

namespace progressTree.reporting
{
    internal sealed class DriverWrapper : IProgress<Double>
    {
        private readonly ProgressDriver driver;
        private readonly IProgress<Double> progress;
        public DriverWrapper(ProgressDriver driver, IProgress<Double> progress)
        {
            this.driver = driver;
            this.progress = progress;
        }
        public void Report(Double value) => this.progress.Report(this.driver.Accumulate(value));
    }
}