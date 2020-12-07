using System;
using System.Threading;

namespace progressReporting
{
    internal delegate TProgress Add<TProgress>(in TProgress left, in TProgress right);
    internal sealed class IncrementalProgress<TProgress> : IProgress<TProgress>
        where TProgress : unmanaged, IComparable<TProgress>
    {
        private TProgress _next;
        private readonly Add<TProgress> _add;
        private readonly TProgress _increment;
        private readonly IProgress<TProgress> _progress;
        public IncrementalProgress(IProgress<TProgress> progress, in TProgress increment, Add<TProgress> add, TProgress root)
        {
            _add = add;
            _next = root;
            _progress = progress;
            _increment = increment;
        }
        public void Report(TProgress value)
        {
            var current = _next;
            while(_next.CompareTo(value) <= 0) {
                _next = _add(_increment, _next);
            }
            if(_next.CompareTo(current) > 0) {
                _progress.Report(value);
            }
        }
    }
}