using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using progressTree.extensions;
using Xunit;

namespace progressTreeTest.extensions
{
    public sealed class TimerStreamTest
    {
        [Fact]
        public void ConstructorThrowsOnIntervalOfZero()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimerStream(interval: TimeSpan.Zero));
        }
        [Fact]
        public void ConstructorThrowsOnNegativeInterval()
        {
            var negativeInterval = TimeSpan.FromMilliseconds(-1);
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimerStream(interval: negativeInterval));
        }
        [Fact]
        public async Task TimeStampsGrowLinearly()
        {
            const Int32 count = 12;
            const Int32 intervalMs = 12;
            var expected = Enumerable.Range(0, count).Select(i => TimeSpan.FromMilliseconds(intervalMs * i));
            var timeStamps = await Take(new TimerStream(TimeSpan.FromMilliseconds(intervalMs)), count, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(expected, timeStamps.Select(t => t.ReportedTime));
        }
        [Fact]
        public async Task TimeStampsAreAcurate()
        {
            const Int32 count = 16;
            const Int32 intervalMs = 32;
            var timeStamps = await Take(new TimerStream(TimeSpan.FromMilliseconds(intervalMs)), count, CancellationToken.None).ConfigureAwait(false);
            Double meanError = timeStamps.Select(t => (t.RealTime - t.ReportedTime).TotalSeconds).Average();
            Assert.Equal(0d, meanError, precision: 2);
        }

        [Fact]
        public async Task TimeStampsArePrecise()
        {
            const Int32 count = 16;
            const Int32 intervalMs = 48;
            var timeStamps = await Take(new TimerStream(TimeSpan.FromMilliseconds(intervalMs)), count, CancellationToken.None).ConfigureAwait(false);
            (Double _, Double stdDev) = StdDev(timeStamps.Select(t => (t.RealTime - t.ReportedTime).TotalSeconds).ToArray());
            Assert.Equal(0, stdDev, 2);
        }

        [Fact]
        public async Task ActiveStreamCanBeCancelled()
        {
            const Int32 count = 6;
            const Int32 intervalMs = 16;
            const Int32 cancelAfterMs = (Int32)(intervalMs * ((count / 2d) - 0.5));
            using (var cts = new CancellationTokenSource(cancelAfterMs))
            {
                IAsyncEnumerable<TimeSpan> stream = new TimerStream(TimeSpan.FromMilliseconds(intervalMs));
                await Assert.ThrowsAsync<TaskCanceledException>(() => Take(stream, count, cts.Token)).ConfigureAwait(false);
            }
        }
        [Fact]
        public async Task WorkOverheadIsCompensatedByIntegerMultiplesOfTheTargetInterval()
        {
            const Int32 count = 8;
            const Int32 intervalMs = 48; // values lower than 16ms can cause flaky tests
            const Int32 extendendIntervalMs = (Int32)(1.2 * intervalMs);
            Int32 expectedOverheadFactor = (Int32)Math.Ceiling(extendendIntervalMs / (Decimal)intervalMs);
            Func<Task> workOverhead = () => Task.Delay(extendendIntervalMs);
            var expected = Enumerable.Range(0, count).Select(i => TimeSpan.FromMilliseconds(expectedOverheadFactor * intervalMs * i));
            var timeStamps = await Take(new TimerStream(TimeSpan.FromMilliseconds(intervalMs)), workOverhead, count, CancellationToken.None).ConfigureAwait(false);
            Assert.Equal(expected, timeStamps.Select(t => t.ReportedTime));
        }
        private static Task<IEnumerable<TimeStamp>> Take(IAsyncEnumerable<TimeSpan> stream, Int32 count, CancellationToken token)
        {
            return Take(stream, () => Task.CompletedTask, count, token);
        }
        private static async Task<IEnumerable<TimeStamp>> Take(IAsyncEnumerable<TimeSpan> stream, Func<Task> work, Int32 count, CancellationToken token)
        {
            var timer = Stopwatch.StartNew();
            var timeStamps = new List<TimeStamp>(count);
            await foreach (var timeStamp in stream.WithCancellation(token).ConfigureAwait(false))
            {
                var stamp = new TimeStamp(timer.Elapsed, timeStamp);
                await work().ConfigureAwait(false);
                if (timeStamps.Count == count)
                {
                    break;
                }
                timeStamps.Add(stamp);
            }
            return timeStamps;
        }
        private sealed class TimeStamp
        {
            public TimeSpan RealTime { get; }
            public TimeSpan ReportedTime { get; }
            public TimeStamp(TimeSpan realTime, TimeSpan reportedTime)
            {
                RealTime = realTime;
                ReportedTime = reportedTime;
            }
        }

        private static (Double mean, Double stdDev) StdDev(IEnumerable<Double> values)
        {
            Double mean = values.Average();
            return (mean, Math.Sqrt(values.Select(v => v - mean).Select(v => v * v).Average()));
        }
    }
}