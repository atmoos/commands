using System;

namespace progressReporting
{
    public readonly struct Range<TProgress>
        where TProgress : IComparable<TProgress>
    {
        public TProgress Lower { get; }
        public TProgress Upper { get; }
        public Range(in TProgress lower, in TProgress upper)
        {
            if(lower.CompareTo(upper) >= 0) {
                throw new ArgumentException($"The range [({nameof(lower)}={lower:g3}), ({nameof(upper)}={upper:g3})] is empty.");
            }
            (Lower, Upper) = (lower, upper);
        }
        internal Boolean Inclusive(in TProgress value) => Lower.CompareTo(value) <= 0 && value.CompareTo(Upper) <= 0;
        internal Boolean Exclusive(in TProgress value) => Lower.CompareTo(value) < 0 && value.CompareTo(Upper) < 0;
    }
    public interface IBoundedProgressBuilder<in TProgress>
    {
        IProgress<TProgress> Inclusive();
        IProgress<TProgress> Exclusive();
    }
    internal sealed class BoundedProgressBuilder<TProgress> : IBoundedProgressBuilder<TProgress>
        where TProgress : IComparable<TProgress>
    {
        private readonly Range<TProgress> _range;
        private readonly IProgress<TProgress> _progress;
        public BoundedProgressBuilder(IProgress<TProgress> progress, in Range<TProgress> range)
        {
            _range = range;
            _progress = progress;
        }
        public IProgress<TProgress> Inclusive() => new BoundedProgress(_progress, _range, (r, v) => r.Inclusive(v));
        public IProgress<TProgress> Exclusive() => new BoundedProgress(_progress, _range, (r, v) => r.Exclusive(v));
        private sealed class BoundedProgress : IProgress<TProgress>
        {
            private readonly Range<TProgress> _range;
            private readonly IProgress<TProgress> _progress;
            private readonly Func<Range<TProgress>, TProgress, Boolean> _withinBounds;
            public BoundedProgress(IProgress<TProgress> progress, in Range<TProgress> range, Func<Range<TProgress>, TProgress, Boolean> withinBounds)
            {
                _range = range;
                _progress = progress;
                _withinBounds = withinBounds;
            }
            public void Report(TProgress value)
            {
                if(_withinBounds(_range, value)) {
                    _progress.Report(value);
                }
            }
        }
    }
}