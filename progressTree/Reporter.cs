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
            _root = root;
            _parent = this;
            _driver = ProgressDriver.Create(1);
            Progress = _rootProgress = progress;
        }
        internal Reporter(Progress root, ProgressDriver driver, IProgress<Double> progress)
        {
            _root = root;
            _driver = driver;
            _rootProgress = progress;
            _parent = root.Exchange(this);
            progress.Report(0);
            Progress = new DriverWrapper(_driver, progress);
        }
        public void Report()
        {
            var progress = _driver.Advance();
            _rootProgress.Report(progress);
        }
        public IProgress<Double> Export() => Progress.Monotonic().Strictly.Increasing().Bounded(0, 1).Inclusive();

        public void Dispose()
        {
            _root.Exchange(_parent);
            _parent.Report();
        }
        internal static Reporter Root(Progress root, IProgress<Double> progress) => new Reporter(root, progress);
    }
}