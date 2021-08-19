using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace progressTree.extensions
{
    public sealed class Interval : IAsyncEnumerable<TimeSpan>
    {
        private readonly Int32 intervals;
        private readonly TimerStream stream;
        public Interval(TimeSpan duration, Int32 intervals)
            : this(intervals, TimeSpan.FromTicks(duration.Ticks / intervals))
        {
        }
        public Interval(Int32 count, TimeSpan interval)
        {
            this.intervals = count;
            this.stream = new TimerStream(interval);
        }
        public async IAsyncEnumerator<TimeSpan> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            Int32 interval = -1;
            await foreach(TimeSpan timeStamp in this.stream.WithCancellation(cancellationToken).ConfigureAwait(false)) {
                yield return timeStamp;
                if(interval++ > this.intervals) {
                    break;
                }
            }
        }
    }
    public sealed class TimerStream : IAsyncEnumerable<TimeSpan>
    {
        private readonly Int64 interval;
        public TimerStream(TimeSpan interval)
        {
            if(interval <= TimeSpan.Zero) {
                throw new ArgumentOutOfRangeException($"Interval must be in the half open interval of ]0, ∞[. Received: {interval:g}");
            }
            this.interval = interval.Ticks;
        }
        public async IAsyncEnumerator<TimeSpan> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            const Int64 constantTimeIncrement = 1;
            Stopwatch timer = Stopwatch.StartNew();
            TimeSpan jitterFreeRelativeTime = TimeSpan.Zero;
            while(true) {
                yield return jitterFreeRelativeTime;
                Int64 delta = Math.Max(timer.Elapsed.Ticks - jitterFreeRelativeTime.Ticks, 0);
                Int64 relativeTimeIncrement = (delta / this.interval) + constantTimeIncrement;
                jitterFreeRelativeTime += TimeSpan.FromTicks(relativeTimeIncrement * this.interval);
                await Task.Delay(jitterFreeRelativeTime - timer.Elapsed, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
