using System;

namespace progressReporting;

internal delegate Boolean InRange<TProgress>(in Range<TProgress> range, in TProgress value) where TProgress : IComparable<TProgress>;
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
    private readonly Range<TProgress> range;
    private readonly IProgress<TProgress> progress;
    public BoundedProgressBuilder(IProgress<TProgress> progress, in Range<TProgress> range)
    {
        this.range = range;
        this.progress = progress;
    }
    public IProgress<TProgress> Inclusive() => new BoundedProgress(this.progress, this.range, (in Range<TProgress> r, in TProgress v) => r.Inclusive(v));
    public IProgress<TProgress> Exclusive() => new BoundedProgress(this.progress, this.range, (in Range<TProgress> r, in TProgress v) => r.Exclusive(v));
    private sealed class BoundedProgress : IProgress<TProgress>
    {
        private readonly Range<TProgress> range;
        private readonly IProgress<TProgress> progress;
        private readonly InRange<TProgress> withinBounds;
        public BoundedProgress(IProgress<TProgress> progress, in Range<TProgress> range, InRange<TProgress> withinBounds)
        {
            this.range = range;
            this.progress = progress;
            this.withinBounds = withinBounds;
        }
        public void Report(TProgress value)
        {
            if(this.withinBounds(in this.range, in value)) {
                this.progress.Report(value);
            }
        }
    }
}