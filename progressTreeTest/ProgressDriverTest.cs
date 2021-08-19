using System;
using System.Collections.Generic;
using System.Linq;
using progressTree;
using Xunit;

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
        private readonly Int32 precision;
        private readonly ProgressDriver driver;
        public Int32 Granularity { get; }
        public DriverTestHarness(ProgressDriver driver, Int32 granularity = 16, Int32 precision = 15)
        {
            this.driver = driver;
            this.precision = precision;
            Granularity = granularity;
        }
        public void AccumulateIsIdempotent()
        {
            var expected = Enumerable.Repeat(0d, Granularity);
            var actual = Enumerable.Repeat(0d, Granularity).Select(_ => this.driver.Accumulate(0d));
            Assert.Equal(expected, actual);
        }
        public void AccumulateIsBoundedByAdvance(Double childProgress)
        {
            Assert.InRange(childProgress, 0d, 1d); // sanity check of input values!
            Double lowerAdvance = this.driver.Advance();
            foreach(var _ in Enumerable.Range(1, Granularity)) {
                Double actual = this.driver.Accumulate(childProgress);
                Double upperAdvance = this.driver.Advance();
                Assert.InRange(actual, lowerAdvance, upperAdvance);
                lowerAdvance = upperAdvance;
            }
        }
        public void AdvanceIsStrictlyMonotonic()
        {
            Double prevValue = Double.NegativeInfinity;
            foreach(var _ in Enumerable.Range(0, Granularity)) {
                Double actualValue = this.driver.Advance();
                Assert.NotStrictEqual(prevValue, actualValue);
                Assert.InRange(actualValue, prevValue, Double.PositiveInfinity);
                prevValue = actualValue;
            }
        }
        public void AdvanceIsLinear() => Assert.Equal(0d, Gradient(AllDeltas()), this.precision);
        private IEnumerable<Double> AllDeltas()
        {
            Double prevValue = this.driver.Advance();
            while(prevValue <= 1d) {
                Double value = this.driver.Advance();
                yield return value - prevValue;
                prevValue = value;
            }
        }
        private static Double Gradient(IEnumerable<Double> sequence)
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
        private readonly DriverTestHarness harness;
        public IterativeDriverTest()
        {
            const Int32 iterations = 16;
            var driver = ProgressDriver.Create(iterations);
            this.harness = new DriverTestHarness(driver, granularity: iterations);
        }
        [Fact]
        public void AccumulateIsIdempotent() => this.harness.AccumulateIsIdempotent();
        [Theory]
        [MemberData(nameof(DriverTestHarness.ChildProgress), MemberType = typeof(DriverTestHarness))]
        public void AccumulateIsBoundedByAdvance(Double childProgress) => this.harness.AccumulateIsBoundedByAdvance(childProgress);
        [Fact]
        public void AdvanceIsStrictlyMonotonic() => this.harness.AdvanceIsStrictlyMonotonic();
        [Fact]
        public void AdvanceIsLinear() => this.harness.AdvanceIsLinear();
    }

    public sealed class TemporalDriverTest : IDriverTest
    {
        private readonly DriverTestHarness harness = new DriverTestHarness(ProgressDriver.Create(TimeSpan.FromMilliseconds(8)), precision: 9);
        [Theory]
        [MemberData(nameof(DriverTestHarness.ChildProgress), MemberType = typeof(DriverTestHarness))]
        public void AccumulateIsBoundedByAdvance(Double childProgress) => this.harness.AccumulateIsBoundedByAdvance(childProgress);
        [Fact]
        public void AdvanceIsLinear() => this.harness.AdvanceIsLinear();
        [Fact]
        public void AdvanceIsStrictlyMonotonic() => this.harness.AdvanceIsStrictlyMonotonic();
    }

    public sealed class NonLinearDriverTest : INonLinearProgress<Decimal>, IDriverTest, IIdempotentAccumulate
    {
        private Double time;
        private readonly Double timeStep;
        private readonly DriverTestHarness harness;
        public NonLinearDriverTest()
        {
            this.time = Math.E;
            this.timeStep = Math.PI * Math.E / Math.ScaleB(1, 10);
            this.harness = new DriverTestHarness(ProgressDriver.Create((Decimal)Math.PI, this), granularity: 32);
        }
        [Fact]
        public void AccumulateIsIdempotent() => this.harness.AccumulateIsIdempotent();
        [Theory]
        [MemberData(nameof(DriverTestHarness.ChildProgress), MemberType = typeof(DriverTestHarness))]
        public void AccumulateIsBoundedByAdvance(Double childProgress) => this.harness.AccumulateIsBoundedByAdvance(childProgress);
        [Fact]
        public void AdvanceIsStrictlyMonotonic() => this.harness.AdvanceIsStrictlyMonotonic();
        [Fact]
        public void AdvanceIsLinear() => this.harness.AdvanceIsLinear();
        Double INonLinearProgress<Decimal>.Linearise(Decimal progress) => Math.Sqrt((Double)progress);
        Decimal INonLinearProgress<Decimal>.Progress()
        {
            this.time += this.timeStep;
            return (Decimal)Math.Pow(this.time, 2);
        }
    }
}