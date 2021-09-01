using System;
using System.Collections.Generic;

namespace progressReporting.concurrent
{
    public delegate INorm<TProgress> CreateNorm<TProgress>(IProgress<TProgress> progress);
    public static class Norm
    {
        public static INorm<TProgress> Max<TProgress>(IProgress<TProgress> progress) where TProgress : IComparable<TProgress> => new MaxNorm<TProgress>(progress);
        public static INorm<TProgress> Min<TProgress>(IProgress<TProgress> progress) where TProgress : IComparable<TProgress> => new MinNorm<TProgress>(progress);
    }
    internal sealed class MinNorm<TProgress> : INorm<TProgress>
        where TProgress : IComparable<TProgress>
    {
        private readonly IProgress<TProgress> progress;
        internal MinNorm(IProgress<TProgress> progress) => this.progress = progress;
        public void Update(in TProgress current, IEnumerable<TProgress> others)
        {
            var min = Minimum(in current, in others);
            if(min.CompareTo(current) <= 0) {
                this.progress.Report(min);
            }
        }
        private static TProgress Minimum(in TProgress current, in IEnumerable<TProgress> previous)
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
    internal sealed class MaxNorm<TProgress> : INorm<TProgress>
    where TProgress : IComparable<TProgress>
    {
        private readonly IProgress<TProgress> progress;
        internal MaxNorm(IProgress<TProgress> progress) => this.progress = progress;
        public void Update(in TProgress current, IEnumerable<TProgress> others)
        {
            var max = Maximum(in current, in others);
            if(current.CompareTo(max) <= 0) {
                this.progress.Report(max);
            }
        }
        private static TProgress Maximum(in TProgress current, in IEnumerable<TProgress> previous)
        {
            var norm = current;
            foreach(var candidate in previous) {
                if(norm.CompareTo(candidate) < 0) {
                    norm = candidate;
                }
            }
            return norm;
        }
    }
}