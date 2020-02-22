using System;
using System.Collections.Generic;
using System.Linq;
using progress;

using static System.Linq.Enumerable;

namespace progressTest
{
    public static class Convenience
    {
        public static List<Double> ExpectedProgress(Int32 iterations) => Range(0, iterations + 1).Select(i => ((Double)i) / iterations).ToList();
        public static void GenerateProgress(Progress progress, Int32 iterations)
        {
            using(Reporter reporter = progress.Setup(iterations)) {
                foreach(var _ in Range(0, iterations)) {
                    reporter.Report();
                }
            }
        }
        public static void Recursive(Progress progressReporter, params Int32[] depthAndWidth)
        {
            void Expand(Progress progress, Queue<Int32> tree)
            {
                if(!tree.TryDequeue(out Int32 depth)) {
                    return;
                }
                Int32 width = depth % 3 + 1;
                using(progress.Setup(width)) {
                    foreach(var use in Range(0, width)) {
                        using(progress.Setup(depth)) {
                            foreach(var step in Enumerable.Range(0, depth)) {
                                Expand(progress, tree);
                            }
                        }
                    }
                }
            }
            Expand(progressReporter, new Queue<Int32>(depthAndWidth));
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