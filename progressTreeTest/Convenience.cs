using System;
using System.Collections.Generic;
using System.Linq;
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
        public static void Recursive(Progress progress, params Int32[] tree)
        {
            if(tree == null || tree.Length == 0) {
                return;
            }
            void Report(Int32 depth)
            {
                Int32 width = tree[depth];
                if(depth + 1 == tree.Length) {
                    using(var r = progress.Schedule(width)) {
                        foreach(var _ in Range(0, width)) {
                            r.Report();
                        }
                    }
                    return;
                }
                using(progress.Schedule(width)) {
                    foreach(var _ in Range(0, width)) {
                        Report(depth + 1);
                    }
                }
            }
            Report(0);
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