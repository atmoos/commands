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
        public static IEnumerable<Double> ExpectedProgress(Int32 iterations) => ExpectedProgress(iterations, 1d, 0d);
        public static IEnumerable<Double> ExpectedProgress(Int32 iterations, Double scale, Double offset)
        {
            scale /= iterations;
            return Range(0, iterations + 1).Select(i => scale * i + offset);
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
        public static void Report(Progress progress, IProgress<Double> expected, params Int32[] tree)
        {
            if(tree == null || tree.Length == 0) {
                return;
            }
            Double Recursive(Int32 depth, Double state, Int32 denominator)
            {
                Int32 width = tree[depth];
                if(depth + 1 == tree.Length) {
                    var div = (Double)denominator * width;
                    using(var r = progress.Schedule(width)) {
                        foreach(var pos in Range(0, width)) {
                            r.Report();
                            expected.Report(state + pos / div);
                        }
                    }
                    return state + 1d / denominator;
                }
                denominator *= width;
                using(progress.Schedule(width)) {
                    foreach(var _ in Range(0, width)) {
                        state = Recursive(depth + 1, state, denominator);
                    }
                }
                return state;
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

    }
}