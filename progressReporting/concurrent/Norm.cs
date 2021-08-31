using System;
using System.Collections.Generic;

namespace progressReporting.concurrent
{
    public delegate INorm<T> CreateNorm<T>(IProgress<T> progress);
    public static class Norm
    {
        public static INorm<T> Max<T>(IProgress<T> progress) where T : IComparable<T> => Norm<T>.Max(progress);
        public static INorm<T> Min<T>(IProgress<T> progress) where T : IComparable<T> => Norm<T>.Min(progress);
    }
    internal sealed class Norm<T> : INorm<T>
        where T : IComparable<T>
    {
        private delegate T Compute(in T current, IEnumerable<T> previous);
        private readonly Compute computeNorm;
        private readonly IProgress<T> progress;
        private Norm(IProgress<T> progress, Compute selectNorm)
        {
            this.progress = progress;
            this.computeNorm = selectNorm;
        }

        public void Update(in T current, IEnumerable<T> others)
        {
            var norm = this.computeNorm(in current, others);
            if(norm.CompareTo(current) <= 0) {
                this.progress.Report(norm);
            }
        }
        internal static Norm<T> Max(IProgress<T> progress) => new(progress, Maximum);
        internal static Norm<T> Min(IProgress<T> progress) => new(progress, Minimum);
        private static T Maximum(in T current, IEnumerable<T> previous)
        {
            var norm = current;
            foreach(var candidate in previous) {
                if(norm.CompareTo(candidate) < 0) {
                    norm = candidate;
                }
            }
            return norm;
        }
        private static T Minimum(in T current, IEnumerable<T> previous)
        {
            var norm = current;
            foreach(var candidate in previous) {
                if(candidate.CompareTo(norm) < 0) {
                    norm = candidate;
                }
            }
            return norm;
        }
    }
}