using System;

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
        public IncrementalProgress(IProgress<TProgress> progress, in TProgress increment, Add<TProgress> add, in TProgress root)
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

    public static class IncrementalProgressExtensions
    {
        public static IProgress<Double> Incremental(this IProgress<Double> progress, in Double increment, in Double root = default)
        {
            return new IncrementalProgress<Double>(progress, in increment, (in Double l, in Double r) => l + r, in root);
        }
        public static IProgress<Single> Incremental(this IProgress<Single> progress, in Single increment, in Single root = default)
        {
            return new IncrementalProgress<Single>(progress, in increment, (in Single l, in Single r) => l + r, in root);
        }
        public static IProgress<Byte> Incremental(this IProgress<Byte> progress, in Byte increment, in Byte root = default)
        {
            return new IncrementalProgress<Byte>(progress, in increment, (in Byte l, in Byte r) => (Byte)(l + r), in root);
        }
        public static IProgress<UInt64> Incremental(this IProgress<UInt64> progress, in UInt64 increment, in UInt64 root = default)
        {
            return new IncrementalProgress<UInt64>(progress, in increment, (in UInt64 l, in UInt64 r) => l + r, in root);
        }
        public static IProgress<UInt32> Incremental(this IProgress<UInt32> progress, in UInt32 increment, in UInt32 root = default)
        {
            return new IncrementalProgress<UInt32>(progress, in increment, (in UInt32 l, in UInt32 r) => l + r, in root);
        }
        public static IProgress<UInt16> Incremental(this IProgress<UInt16> progress, in UInt16 increment, in UInt16 root = default)
        {
            return new IncrementalProgress<UInt16>(progress, in increment, (in UInt16 l, in UInt16 r) => (UInt16)(l + r), in root);
        }
        public static IProgress<SByte> Incremental(this IProgress<SByte> progress, in SByte increment, in SByte root = default)
        {
            return new IncrementalProgress<SByte>(progress, in increment, (in SByte l, in SByte r) => (SByte)(l + r), in root);
        }
    }
}