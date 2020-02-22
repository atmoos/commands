using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using progressTree;

namespace progressTreeTest
{
    public interface IDriverTest
    {
        void AdvanceIsLinear();
        void AdvanceIsStrictlyMonotonic();
        void AccumulateIsBoundedByAdvance(Double childProgress);
    }

    public interface IIdempotentAccumulate
    {
        void AccumulateIsIdempotent();
    }
    internal sealed class DriverTestHarness : IDriverTest, IIdempotentAccumulate
    {
        private readonly Int32 _precision;
        private readonly ProgressDriver _driver;
        public Int32 Granularity { get; }
        public DriverTestHarness(ProgressDriver driver, Int32 granularity = 16, Int32 precision = 15)
        {
            _driver = driver;
            _precision = precision;
            Granularity = granularity;
        }
        public void AccumulateIsIdempotent()
        {
            var expected = Enumerable.Repeat(0d, Granularity);
            var actual = Enumerable.Repeat(0d, Granularity).Select(_ => _driver.Accumulate(0d));
            Assert.Equal(expected, actual);
        }
        public void AccumulateIsBoundedByAdvance(Double childProgress)
        {
            Assert.InRange(childProgress, 0d, 1d); // sanity check of input values!
            Double lowerAdvance = _driver.Advance();
            foreach(var _ in Enumerable.Range(1, Granularity)) {
                Double actual = _driver.Accumulate(childProgress);
                Double upperAdvance = _driver.Advance();
                Assert.InRange(actual, lowerAdvance, upperAdvance);
                lowerAdvance = upperAdvance;
            }
        }
        public void AdvanceIsStrictlyMonotonic()
        {
            Double prevValue = Double.NegativeInfinity;
            foreach(var _ in Enumerable.Range(0, Granularity)) {
                Double actualValue = _driver.Advance();
                Assert.NotStrictEqual(prevValue, actualValue);
                Assert.InRange(actualValue, prevValue, Double.PositiveInfinity);
                prevValue = actualValue;
            }
        }
        public void AdvanceIsLinear() => Assert.Equal(0d, Gradient(AllDeltas()), _precision);
        private IEnumerable<Double> AllDeltas()
        {
            Double prevValue = _driver.Advance();
            while(prevValue <= 1d) {
                Double value = _driver.Advance();
                yield return value - prevValue;
                prevValue = value;
            }
        }
        static Double Gradient(IEnumerable<Double> sequence)
        {
            var values = sequence.ToList();
            Double meanY = values.Average();
            Double spread = Enumerable.Range(0, values.Count).Select<Int32, Double>(x => x * x).Sum();
            Double scaledValues = values.Select((y, x) => x * (y - meanY)).Sum();
            return scaledValues / spread;
        }
        public static IEnumerable<Object[]> ChildProgress()
        {
            yield return new Object[] { 0d };
            yield return new Object[] { 1d / 3d };
            yield return new Object[] { 0.5d };
            yield return new Object[] { 2d / 3d };
            yield return new Object[] { 1d };
        }
    }

    public sealed class IterativeDriverTest : IDriverTest, IIdempotentAccumulate
    {
        readonly DriverTestHarness _harness;
        public IterativeDriverTest()
        {
            const Int32 iterations = 16;
            var driver = ProgressDriver.Create(iterations);
            _harness = new DriverTestHarness(driver, granularity: iterations);
        }
        [Fact]
        public void AccumulateIsIdempotent() => _harness.AccumulateIsIdempotent();
        [Theory]
        [MemberData(nameof(DriverTestHarness.ChildProgress), MemberType = typeof(DriverTestHarness))]
        public void AccumulateIsBoundedByAdvance(Double childProgress) => _harness.AccumulateIsBoundedByAdvance(childProgress);
        [Fact]
        public void AdvanceIsStrictlyMonotonic() => _harness.AdvanceIsStrictlyMonotonic();
        [Fact]
        public void AdvanceIsLinear() => _harness.AdvanceIsLinear();
    }

    public sealed class TemporalDriverTest : IDriverTest
    {
        readonly DriverTestHarness _harness = new DriverTestHarness(ProgressDriver.Create(TimeSpan.FromMilliseconds(8)), precision: 9);
        [Theory]
        [MemberData(nameof(DriverTestHarness.ChildProgress), MemberType = typeof(DriverTestHarness))]
        public void AccumulateIsBoundedByAdvance(Double childProgress) => _harness.AccumulateIsBoundedByAdvance(childProgress);
        [Fact]
        public void AdvanceIsLinear() => _harness.AdvanceIsLinear();
        [Fact]
        public void AdvanceIsStrictlyMonotonic() => _harness.AdvanceIsStrictlyMonotonic();
    }

    public sealed class NonLinearDriverTest : INonLinearProgress<Decimal>, IDriverTest, IIdempotentAccumulate
    {
        Double _time;
        readonly Double _timeStep;
        readonly DriverTestHarness _harness;
        public NonLinearDriverTest()
        {
            _time = Math.E;
            _timeStep = Math.PI * Math.E / Math.ScaleB(1, 10);
            _harness = new DriverTestHarness(ProgressDriver.Create((Decimal)Math.PI, this), granularity: 32);
        }
        [Fact]
        public void AccumulateIsIdempotent() => _harness.AccumulateIsIdempotent();
        [Theory]
        [MemberData(nameof(DriverTestHarness.ChildProgress), MemberType = typeof(DriverTestHarness))]
        public void AccumulateIsBoundedByAdvance(Double childProgress) => _harness.AccumulateIsBoundedByAdvance(childProgress);
        [Fact]
        public void AdvanceIsStrictlyMonotonic() => _harness.AdvanceIsStrictlyMonotonic();
        [Fact]
        public void AdvanceIsLinear() => _harness.AdvanceIsLinear();
        Double INonLinearProgress<Decimal>.Linearise(Decimal progress) => Math.Sqrt((Double)progress);
        Decimal INonLinearProgress<Decimal>.Progress()
        {
            _time += _timeStep;
            return (Decimal)Math.Pow(_time, 2);
        }
    }
}