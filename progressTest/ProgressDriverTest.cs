using System;
using System.Collections.Generic;
using System.Linq;
using progress;
using Xunit;

namespace progressTest
{
    internal sealed class DriverTestHarness
    {
        private readonly Int32 _precision;
        private readonly ProgressDriver _driver;

        public DriverTestHarness(ProgressDriver driver, Int32 precision = 15)
        {
            _driver = driver;
            _precision = precision;
        }
    }
    public class IterativeDriverTest
    {
        const Int32 Iterations = 8;
        const Int32 Precision = 12;
        const Double ExpectedIncrement = 1d / Iterations;
        ProgressDriver _driver;
        public IterativeDriverTest()
        {
            _driver = ProgressDriver.Create(Iterations);
        }
        [Fact]
        public void IncrementsOnAdvance()
        {
            var expected = Enumerable.Range(1, Iterations).Select(e => e * ExpectedIncrement);
            var actual = Enumerable.Repeat(0d, Iterations).Select(_ => _driver.Advance());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AccumulateIsIdempotent()
        {
            var expected = Enumerable.Repeat(0d, Iterations);
            var actual = Enumerable.Repeat(0d, Iterations).Select(_ => _driver.Accumulate(0d));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AccumulateScalesChildProgress()
        {
            var expected = Enumerable.Repeat(0d, Iterations).Select(e => e * ExpectedIncrement);
            var actual = Enumerable.Repeat(0d, Iterations).Select(a => _driver.Accumulate(a));
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AccumulateIsBoundedByAdvance()
        {
            Double lowerAdvance = _driver.Advance();
            foreach(var _ in Enumerable.Range(1, Iterations)) {
                Double actual = _driver.Accumulate(0.5);
                Double upperAdvance = _driver.Advance();
                Assert.InRange(actual, lowerAdvance, upperAdvance);
                lowerAdvance = upperAdvance;
            }
        }
    }
}