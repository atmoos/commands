using System;
using Xunit;
using progressReporting;

namespace progressReportingTest
{
    public sealed class BoundedProgressTest
    {
        private static readonly Double[] Input = new[] { -2d, 0d, Math.PI, 9, Double.PositiveInfinity, 1.23d, -Math.E, 2d, 9d, 3d };
        private readonly ProgressRecorder<Double> _actualProgress = new ProgressRecorder<Double>();
        [Fact]
        public void CreatingWithEmptyRangeThrows()
        {
            Xunit.Assert.Throws<ArgumentException>(() => _actualProgress.Bounded(2, 2));
            Xunit.Assert.Throws<ArgumentException>(() => _actualProgress.Bounded(3, 1));
            Xunit.Assert.Throws<ArgumentException>(() => _actualProgress.Bounded(1, -1));
        }
        [Fact]
        public void InclusiveRangeIncludesBoundaries()
        {
            var progress = _actualProgress.Bounded(0d, Math.PI).Inclusive();
            Assert(progress, 0d, Math.PI, 1.23d, 2d, 3d);
        }
        [Fact]
        public void ExclusiveRangeExcludesBoundaries()
        {
            var progress = _actualProgress.Bounded(-2d, Math.PI).Exclusive();
            Assert(progress, 0d, 1.23d, 2d, 3d);
        }
        private void Assert(IProgress<Double> progress, params Double[] expected)
        {
            foreach(var value in Input) {
                progress.Report(value);
            }
            Xunit.Assert.Equal(expected, _actualProgress);
        }
    }

}