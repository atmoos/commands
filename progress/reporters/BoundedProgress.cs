using System;

namespace progress.reporters
{
    public sealed class BoundedProgress : IProgress<Double>
    {
        private readonly Double _lower;
        private readonly Double _upper;
        private readonly IProgress<Double> _progress;
        private readonly Func<BoundedProgress, Double, Boolean> _withinBounds;
        private BoundedProgress(IProgress<Double> progress, Double lower, Double upper, Func<BoundedProgress, Double, Boolean> withinBounds)
        {
            _lower = lower;
            _upper = upper;
            _progress = progress;
            _withinBounds = withinBounds;
        }
        public void Report(Double value)
        {
            if(_withinBounds(this, value)) {
                _progress.Report(value);
            }
        }
        public static BoundedProgress Inclusive(IProgress<Double> progress, Double lower, Double upper)
        {
            GuardRange(lower, upper);
            return new BoundedProgress(progress, lower, upper, Inclusive);
        }
        public static BoundedProgress Exclusive(IProgress<Double> progress, Double lower, Double upper)
        {
            GuardRange(lower, upper);
            return new BoundedProgress(progress, lower, upper, Exclusive);
        }
        private static void GuardRange(Double lower, Double upper)
        {
            if(lower >= upper) {
                throw new ArgumentException($"The range [({nameof(lower)}={lower:g3}), ({nameof(upper)}={upper:g3})] is empty.");
            }
        }
        private static Boolean Exclusive(BoundedProgress p, Double value) => p._lower < value && value < p._upper;
        private static Boolean Inclusive(BoundedProgress p, Double value) => p._lower <= value && value <= p._upper;
    }
}