using System;
using System.Linq;
using System.Collections.Generic;
using progress;
using Xunit;
using System.Diagnostics;
using System.Threading;

namespace progressTest
{
    public class ProgressTest
    {
        private readonly Progress _progressReporter;
        private readonly ProgressRecorder<Double> _actualProgress;

        public ProgressTest()
        {
            _actualProgress = new ProgressRecorder<Double>();
            _progressReporter = Progress.Create(_actualProgress);
        }

        [Fact]
        public void IterativeProgress()
        {
            const Int32 iterations = 8;
            var expected = ExpectedProgress(iterations);
            GenerateProgress(_progressReporter, iterations);
            Assert.Equal(expected, _actualProgress);
        }

        [Fact]
        public void ReportsOnChildProgress()
        {
            const Int32 childIterations = 4;
            const Int32 parentIterations = 8;
            var childProgress = new List<IEnumerable<Double>>();
            using(var parentReport = _progressReporter.Setup(parentIterations)) {
                for(Int32 p = 0; p < parentIterations; ++p) {
                    var subProgress = new ProgressRecorder<Double>();
                    using(var childReport = _progressReporter.Setup(childIterations, subProgress)) {
                        childProgress.Add(subProgress);
                        for(Int32 c = 0; c < childIterations; ++c) {
                            childReport.Report();
                        }
                    }
                    parentReport.Report();
                }
            }
            Assert.Equal(ExpectedProgress(childIterations), childProgress[0]);
            Assert.Equal(ExpectedProgress(childIterations * parentIterations), _actualProgress);
        }
        //[Fact]
        [Trait("Failing test", "Failure as reporting implementation is not complete.")]
        public void NotReportingAnythingWithinUsingStatementsReportsSetupBoundaries()
        {
            List<Double> expected = new List<Double>();
            using(_progressReporter.Setup(4)) {
                expected.Add(0);
                using(_progressReporter.Setup(3)) {
                }
                expected.Add(1d / 4);
                using(_progressReporter.Setup(2)) {
                    using(_progressReporter.Setup(6)) { }
                    using(_progressReporter.Setup(8)) { }
                }
                expected.Add(2d / 4);
                using(_progressReporter.Setup(5)) {
                }
                expected.Add(3d / 4);
                // No fourth step
            }
            // 4d / 4 is added here, despite no fourth step above
            expected.Add(4d / 4);
            Assert.Equal(expected, _actualProgress);
        }
        [Fact]
        public void NestedReportsAreWellBehaved()
        {
            using(var reporter = _progressReporter.Setup(4)) {
                reporter.Report(); // 1/4
                Recursive(_progressReporter, 2, 4); // [1/4 ... 2/4]
                reporter.Report(); // 2/4
                reporter.Report(); // 3/4
                reporter.Report(); // 4/4
            }
            var head = new[] { 0d, 0.25d };
            var tail = new[] { 0.5d, 0.75d, 1d };
            Assert.Equal(head, _actualProgress.Take(2));
            Assert.Equal(tail, _actualProgress.TakeLast(3));
            Assert.True(_actualProgress.Count() > head.Length + tail.Length, "Sanity check that there is an absurdly long middle range");
        }
        private static void GenerateProgress(Progress progress, Int32 iterations)
        {
            using(Reporter reporter = progress.Setup(iterations)) {
                foreach(var _ in Enumerable.Range(0, iterations)) {
                    reporter.Report();
                }
            }
        }
        private static void Recursive(Progress progressReporter, params Int32[] depthAndWidth)
        {
            void Recurse(Progress progress, Queue<Int32> tree)
            {
                if(!tree.TryDequeue(out Int32 steps)) {
                    return;
                }
                Int32 usings = steps % 3 + 1;
                using(Reporter r0 = progress.Setup(usings)) {
                    foreach(var use in Enumerable.Range(0, usings)) {
                        using(var r1 = progress.Setup(steps)) {
                            foreach(var step in Enumerable.Range(0, steps)) {
                                Recurse(progress, tree);
                                r1.Report();
                            }
                        }
                        r0.Report();
                    }
                }
            }
            Recurse(progressReporter, new Queue<Int32>(depthAndWidth));
        }
        private List<Double> ExpectedProgress(Int32 iterations) => Enumerable.Range(0, iterations + 1).Select(i => ((Double)i) / iterations).ToList();
    }
}