using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

namespace progress.extensions
{
    public sealed class TimerStream : IAsyncEnumerable<TimeSpan>
    {
        private readonly Int64 _interval;
        public TimerStream(TimeSpan interval)
        {
            if(interval <= TimeSpan.Zero) {
                throw new ArgumentOutOfRangeException($"Interval must be in the half open interval of ]0, ∞[. Revceived: {interval:g}");
            }
            _interval = interval.Ticks;
        }
        public async IAsyncEnumerator<TimeSpan> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            const Int64 constantTimeIncrement = 1;
            Stopwatch timer = Stopwatch.StartNew();
            TimeSpan jiterFreeRelativeTime = TimeSpan.Zero;
            while(true) {
                yield return jiterFreeRelativeTime;
                Int64 delta = Math.Max(timer.Elapsed.Ticks - jiterFreeRelativeTime.Ticks, 0);
                Int64 relativeTimeIncrement = delta / _interval + constantTimeIncrement;
                jiterFreeRelativeTime += TimeSpan.FromTicks(relativeTimeIncrement * _interval);
                await Task.Delay(jiterFreeRelativeTime - timer.Elapsed, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}