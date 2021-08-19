using System;
using progressReporting;
using progressTree.reporting;

namespace progressTree
{
    public sealed class Reporter : IDisposable
    {
        private readonly Progress root;
        private readonly Reporter parent;
        private readonly ProgressDriver driver;
        private readonly IProgress<Double> rootProgress;
        internal IProgress<Double> Progress { get; }
        private Reporter(Progress root, IProgress<Double> progress)
        {
            this.root = root;
            this.parent = this;
            this.driver = ProgressDriver.Create(1);
            Progress = this.rootProgress = progress;
        }
        internal Reporter(Progress root, ProgressDriver driver, IProgress<Double> progress)
        {
            this.root = root;
            this.driver = driver;
            this.rootProgress = progress;
            this.parent = root.Exchange(this);
            progress.Report(0);
            Progress = new DriverWrapper(this.driver, progress);
        }
        public void Report()
        {
            var progress = this.driver.Advance();
            this.rootProgress.Report(progress);
        }
        public IProgress<Double> Export() => Progress.Bounded(0, 1).Inclusive().Monotonic().Strictly.Increasing();

        public void Dispose()
        {
            this.root.Exchange(this.parent);
            this.rootProgress.Report(1);
            this.parent.Report();
        }
        internal static Reporter Root(Progress root, IProgress<Double> progress) => new(root, progress);
    }
}