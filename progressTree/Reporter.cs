using System;
using progressReporting;
using progressTree.reporting;

namespace progressTree
{
    public sealed class Reporter : IDisposable
    {
        private readonly Progress _root;
        private readonly Reporter _parent;
        private readonly ProgressDriver _driver;
        private readonly IProgress<Double> _rootProgress;
        internal IProgress<Double> Progress { get; }
        private Reporter(Progress root, IProgress<Double> progress)
        {
            this._root = root;
            this._parent = this;
            this._driver = ProgressDriver.Create(1);
            Progress = this._rootProgress = progress;
        }
        internal Reporter(Progress root, ProgressDriver driver, IProgress<Double> progress)
        {
            this._root = root;
            this._driver = driver;
            this._rootProgress = progress;
            this._parent = root.Exchange(this);
            progress.Report(0);
            Progress = new DriverWrapper(this._driver, progress);
        }
        public void Report()
        {
            var progress = this._driver.Advance();
            this._rootProgress.Report(progress);
        }
        public IProgress<Double> Export() => Progress.Bounded(0, 1).Inclusive().Monotonic().Strictly.Increasing();

        public void Dispose()
        {
            this._root.Exchange(this._parent);
            this._rootProgress.Report(1);
            this._parent.Report();
        }
        internal static Reporter Root(Progress root, IProgress<Double> progress) => new(root, progress);
    }
}