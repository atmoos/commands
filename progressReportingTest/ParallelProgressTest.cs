using System;
using System.Collections.Generic;
using System.Linq;
using progressReporting;
using Xunit;

namespace progressReportingTest
{
    public sealed class ParallelProgressTest
    {
        private readonly IProgress<Int32> progA;
        private readonly IProgress<Int32> progB;
        private readonly IProgress<Int32> progC;
        private readonly IEnumerable<Int32> actualProgress;

        public ParallelProgressTest()
        {
            var targetProgress = new ProgressRecorder<Int32>();
            // The tests are easier to understand, when we limit the expected values
            // to monotonically increasing ones.
            var progress = targetProgress.Monotonic().Strictly.Increasing().Concurrent(3).ToArray();

            this.progA = progress[0];
            this.progB = progress[1];
            this.progC = progress[2];
            this.actualProgress = targetProgress;
        }

        [Fact]
        public void ProgressIsReportedOnly_WhenMinimalValueIsIncreased()
        {
            var expectation = new Expectation<Int32>();
            this.progB.Report(expectation.Add(default)); // initial minimal value is reported
            this.progA.Report(2);
            this.progC.Report(1);
            this.progB.Report(expectation.Add(1));
            this.progC.Report(4);
            this.progB.Report(expectation.Add(2));

            Assert.Equal(expectation, this.actualProgress);
        }

        [Fact]
        public void DefaultValue_IsReportedEveryTime()
        {
            var expectation = new Expectation<Int32>();
            this.progB.Report(expectation.Add(default)); // initial minimal value is reported
            this.progC.Report(default);
            this.progA.Report(default);
            this.progC.Report(default);

            Assert.Equal(expectation, this.actualProgress);
        }

        [Fact]
        public void DefaultValue_DoesNotNeedToBeReported_ForProgressToIncrease()
        {
            var expectation = new Expectation<Int32>();
            this.progB.Report(expectation.Delayed(4, 0));
            this.progC.Report(3);
            this.progA.Report(expectation.Add(1)); // new smallest value -> report!,

            Assert.Equal(expectation, this.actualProgress);
        }

        [Fact]
        public void MinimalValueFromAllInstancesIsReported_WhenOtherReportsOneHigher()
        {
            var expectation = new Expectation<Int32>();
            this.ReportAll(expectation.Add(default));
            this.progC.Report(23); // ignore progC in this test...
            this.progA.Report(4);
            this.progB.Report(expectation.Add(1));
            this.progB.Report(3);
            this.progA.Report(expectation.Delayed(5, 3)); // --> 3 is now smallest, report!

            Assert.Equal(expectation, this.actualProgress);
        }

        [Fact]
        public void ProgressIsReported_WhenIncrementIsBetween_MinimalAndSecondToMinimalValue()
        {
            var expectation = new Expectation<Int32> { 0 };
            ReportAll(expectation.Add(1));
            this.progC.Report(8);
            this.progA.Report(6);
            this.progB.Report(expectation.Add(3)); // new smallest value -> report!

            Assert.Equal(expectation, this.actualProgress);
        }

        [Fact]
        public void ReportingTheSameValueMultipleTimes_DoesNotCauseProgressToBeReported()
        {
            var expectation = new Expectation<Int32> { 0 };
            ReportAll(expectation.Add(2));
            this.progA.Report(3); // skip
            this.progA.Report(3); // skip, as we're still reporting on A!
            this.progA.Report(4); // skip, as we're still reporting on A!
            this.progA.Report(3); // skip, as we're still reporting on A!
            this.progA.Report(5); // skip, as we're still reporting on A!
            this.progA.Report(3); // skip, as we're still reporting on A!
            // '3' has been reported multiple times on instance 'A', but 'B' and 'C' are still on '2' -> '3' is not reported!

            Assert.Equal(expectation, this.actualProgress);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void ProgressIsReportedDirectlyToTarget_WhenConcurrencyLevelIsDegenerate(Int32 degenerateConcurrencyLevel)
        {
            var expectedValue = Guid.NewGuid();
            var expectedTargetProgress = new ProgressRecorder<Guid>();
            var parallelProgressInstances = expectedTargetProgress.Concurrent(degenerateConcurrencyLevel);

            var actualSingleWrappedParallelInstance = parallelProgressInstances.Single();
            actualSingleWrappedParallelInstance.Report(expectedValue);

            // All values will be reported no matter what, when degenerate concurrency occurs!
            Assert.Equal(new[] { expectedValue }, expectedTargetProgress);
            // implementation detail:
            Assert.Same(expectedTargetProgress, actualSingleWrappedParallelInstance);
        }

        private void ReportAll(Int32 value)
        {
            this.progA.Report(value);
            this.progB.Report(value);
            this.progC.Report(value);
        }

        private sealed class Expectation<T> : IEnumerable<T>
        {
            private readonly List<T> expectation = new();
            public T Add(T value)
            {
                this.expectation.Add(value);
                return value;
            }
            public T Delayed(T notExpected, T delayed)
            {
                this.expectation.Add(delayed);
                return notExpected;
            }

            public IEnumerator<T> GetEnumerator() => this.expectation.GetEnumerator();

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.expectation.GetEnumerator();
        }
    }
}