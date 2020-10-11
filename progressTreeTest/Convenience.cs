using System;
using System.Collections.Generic;
using System.Linq;
using progressReporting;
using progressTree;

using static System.Linq.Enumerable;

namespace progressTreeTest
{
    public static class Convenience
    {
        public static IEnumerable<Double> ExpectedProgress(Int32 iterations) => ExpectedProgress(0d, iterations, 1d);
        public static IEnumerable<Double> ExpectedProgress(Double start, Int32 intervals, Double end)
        {
            var interval = (end - start) / intervals;
            return Range(0, intervals + 1).Select(index => start + interval * index);
        }
        public static void GenerateProgress(Progress progress, Int32 iterations)
        {
            using(Reporter reporter = progress.Schedule(iterations)) {
                foreach(var _ in Range(0, iterations)) {
                    reporter.Report();
                }
            }
        }
        public static void Report(Progress progress, params Int32[] tree) => Report(progress, Extensions.Empty<Double>(), tree);
        public static void Report(Progress progress, params Int32[][] tree)
        {
            using(progress.Schedule(tree.Length)) {
                foreach(var subtree in tree) {
                    Report(progress, subtree);
                }
            }
        }
        public static void Report(Progress progress, IProgress<Double> expected, params Int32[] tree)
        {
            if(tree == null || tree.Length == 0) {
                return;
            }
            Double Recursive(Int32 depth, Double state, Int32 denominator)
            {
                Int32 width = tree[depth];
                if(++depth < tree.Length) {
                    using(progress.Schedule(width)) {
                        denominator *= width;
                        foreach(var _ in Range(0, width)) {
                            state = Recursive(depth, state, denominator);
                        }
                    }
                    return state;
                }
                using(var r = progress.Schedule(width)) {
                    var div = (Double)denominator * width;
                    foreach(var pos in Range(0, width)) {
                        r.Report();
                        expected.Report(state + pos / div);
                    }
                }
                return state + 1d / denominator;
            }
            expected.Report(Recursive(0, 0, 1));
            return;
        }
        public static Int32 Report(IProgress<Double> progress, params Double[] range)
        {
            foreach(var value in range) {
                progress.Report(value);
            }
            return range.Length;
        }
        public static IEnumerable<T> Section<T>(this IEnumerable<T> source, Int32 start, Int32 take) => source.Skip(start).Take(take);
    }
}