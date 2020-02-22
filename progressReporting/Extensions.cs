using System;
using System.Threading;

namespace progressReporting
{
    public static class Extensions
    {
        public static IProgress<TProgress> Empty<TProgress>() => EmptyProgress<TProgress>.Empty;
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
    }
}