using System;

namespace progress
{
    public sealed class Reporter : IDisposable
    {
        private readonly ProgressTree _tree;
        private readonly Action<ProgressTree> _reset;
        internal Reporter(ProgressTree tree, Action<ProgressTree> reset)
        {
            _tree = tree;
            _reset = reset;
            _tree.Report(0);
        }
        public void Report() => _tree.Report();
        public void Dispose()
        {
            _tree.Report(1d);
            _reset(_tree.Parent);
        }
    }
}