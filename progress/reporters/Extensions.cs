using System;

namespace progress.reporters
{
    public static class Extensions
    {
        public static IProgress<TProgress> Zip<TProgress>(this IProgress<TProgress> progress, IProgress<TProgress> other)
        {
            return new ProgressZip<TProgress>(progress, other);
        }
    }
}