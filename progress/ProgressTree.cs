using System;

namespace progress
{
    internal abstract class ProgressTree : IProgress<Double>
    {
        public static ProgressTree Empty { get; } = new EmptyTree();
        private readonly ProgressDriver _driver;
        public ProgressTree Parent { get; }
        private ProgressTree(ProgressDriver driver)
        {
            Parent = this;
            _driver = driver;
        }
        private ProgressTree(ProgressDriver driver, ProgressTree parent)
            : this(driver)
        {
            Parent = parent;
        }
        public void Report() => ReportImpl(_driver.Advance());
        public void Report(Double value) => ReportImpl(_driver.Accumulate(value));
        protected abstract void ReportImpl(Double value);
        public ProgressTree Chain(ProgressDriver driver) => new ChainNode(driver, this);
        public ProgressTree Branch(ProgressDriver driver, IProgress<Double> progress) => new BranchNode(driver, this, progress);
        public static ProgressTree Root(IProgress<Double> progress) => new RootNode(progress);
        private sealed class RootNode : ProgressTree
        {
            private readonly IProgress<Double> _progressRoot;
            public RootNode(IProgress<Double> progressRoot)
                : base(ProgressDriver.Create(1))
            {
                _progressRoot = progressRoot;
            }
            protected override void ReportImpl(Double value) => _progressRoot.Report(value);
        }
        private sealed class BranchNode : ProgressTree
        {
            private readonly IProgress<Double> _progress;

            public BranchNode(ProgressDriver driver, ProgressTree parent, IProgress<Double> progress)
                : base(driver, parent)
            {
                _progress = progress;
            }
            protected override void ReportImpl(Double value)
            {
                _progress.Report(value);
                Parent.Report(value);
            }
        }
        private sealed class ChainNode : ProgressTree
        {
            public ChainNode(ProgressDriver driver, ProgressTree parent)
                : base(driver, parent)
            {
            }
            protected override void ReportImpl(Double value) => Parent.Report(value);
        }

        private sealed class EmptyTree : ProgressTree
        {
            public EmptyTree() : base(ProgressDriver.Empty)
            {
            }
            protected override void ReportImpl(Double _)
            {
                // empty
            }
        }
    }
}