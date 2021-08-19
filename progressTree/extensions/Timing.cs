using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace progressTree.extensions
{
    public sealed class Interval : IAsyncEnumerable<TimeSpan>
    {
        private readonly Int32 _intervalls;
        private readonly TimerStream _stream;
        public Interval(TimeSpan duration, Int32 intervalls)
            : this(intervalls, TimeSpan.FromTicks(duration.Ticks / intervalls))
        {
        }
        public Interval(Int32 count, TimeSpan interval)
        {
            this._intervalls = count;
            this._stream = new TimerStream(interval);
        }
        public async IAsyncEnumerator<TimeSpan> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            Int32 interval = -1;
            await foreach(TimeSpan timeStamp in this._stream.WithCancellation(cancellationToken).ConfigureAwait(false)) {
                yield return timeStamp;
                if(interval++ > this._intervalls) {
                    break;
                }
            }
        }
    }
    public sealed class TimerStream : IAsyncEnumerable<TimeSpan>
    {
        private readonly Int64 _interval;
        public TimerStream(TimeSpan interval)
        {
            if(interval <= TimeSpan.Zero) {
                throw new ArgumentOutOfRangeException($"Interval must be in the half open interval of ]0, ∞[. Received: {interval:g}");
            }
            this._interval = interval.Ticks;
        }
        public async IAsyncEnumerator<TimeSpan> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            const Int64 constantTimeIncrement = 1;
            Stopwatch timer = Stopwatch.StartNew();
            TimeSpan jitterFreeRelativeTime = TimeSpan.Zero;
            while(true) {
                yield return jitterFreeRelativeTime;
                Int64 delta = Math.Max(timer.Elapsed.Ticks - jitterFreeRelativeTime.Ticks, 0);
                Int64 relativeTimeIncrement = delta / this._interval + constantTimeIncrement;
                jitterFreeRelativeTime += TimeSpan.FromTicks(relativeTimeIncrement * this._interval);
                await Task.Delay(jitterFreeRelativeTime - timer.Elapsed, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
