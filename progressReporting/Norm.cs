using System;
using System.Collections.Generic;

namespace progressReporting
{
    public sealed class Norm<T>
        where T : IComparable<T>
    {
        private delegate T Select(in T current, IEnumerable<T> previous);
        private readonly Select selectNorm;
        private readonly IProgress<T> progress;

        private Norm(IProgress<T> progress, Select selectNorm)
        {
            this.progress = progress;
            this.selectNorm = selectNorm;
        }

        internal void Update(in T current, IEnumerable<T> others)
        {
            var norm = this.selectNorm(in current, others);
            if(norm.CompareTo(current) <= 0) {
                this.progress.Report(norm);
            }
        }
        public static Norm<T> Max(IProgress<T> progress) => new(progress, Maximum);
        public static Norm<T> Min(IProgress<T> progress) => new(progress, Minimum);
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