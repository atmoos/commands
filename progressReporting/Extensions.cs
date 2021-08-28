using System;
using System.Collections.Generic;
using System.Threading;

namespace progressReporting
{
    public static class Extensions
    {
        public static IProgress<TProgress> Empty<TProgress>() => EmptyProgress<TProgress>.Empty;
        public static IProgress<TIn> Map<TIn, TOut>(this IProgress<TOut> progress, Func<TIn, TOut> map) => new ProgressMap<TIn, TOut>(progress, map);
        public static IProgress<TProgress> Zip<TProgress>(this IProgress<TProgress> progress, IProgress<TProgress> other)
        {
            return new ProgressZip<TProgress>(progress, other);
        }
        public static IProgress<TProgress> Cancellable<TProgress>(this IProgress<TProgress> progress, CancellationToken token)
        {
            return progress.Zip(new CancellationAdapter<TProgress>(token));
        }
        public static IProgress<TProgress> Observable<TProgress>(this IProgress<TProgress> progress, Action<TProgress> observer)
        {
            return progress.Zip(new ObservableProgress<TProgress>(observer));
        }
        public static IBoundedProgressBuilder<TProgress> Bounded<TProgress>(this IProgress<TProgress> progress, Range<TProgress> range)
            where TProgress : IComparable<TProgress> => new BoundedProgressBuilder<TProgress>(progress, range);
        public static IBoundedProgressBuilder<TProgress> Bounded<TProgress>(this IProgress<TProgress> progress, TProgress lower, TProgress upper)
            where TProgress : IComparable<TProgress> => Bounded(progress, new Range<TProgress>(lower, upper));
        public static IMonotonicBuilder<Double> Monotonic(this IProgress<Double> progress) => new MonotonicBuilder(progress);
        public static IMonotonicBuilder<TProgress> Monotonic<TProgress>(this IProgress<TProgress> progress)
            where TProgress : IComparable<TProgress> => new MonotonicBuilder<TProgress>(progress);
        public static IEnumerable<IProgress<TProgress>> Concurrent<TProgress>(this IProgress<TProgress> target, Int32 concurrencyLevel)
            where TProgress : struct, IComparable<TProgress> => ParallelProgress<TProgress>.FanOut(target, concurrencyLevel);
    }
}