using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace progressTree.extensions
{
    public static class Flow
    {
        public static IEnumerable<TElement> Enumerate<TElement>(this Progress progress, ICollection<TElement> collection)
        {
            return progress.Enumerate<ICollection<TElement>, TElement>(collection, CancellationToken.None, (p, c) => p.Schedule(c.Count));
        }
        public static IEnumerable<TElement> Enumerate<TElement>(this Progress progress, ICollection<TElement> collection, CancellationToken token)
        {
            return progress.Enumerate<ICollection<TElement>, TElement>(collection, token, (p, c) => p.Schedule(c.Count));
        }
        public static IEnumerable<TElement> Enumerate<TElement>(this Progress progress, ICollection<TElement> collection, CancellationToken token, IProgress<Double> subProgress)
        {
            return progress.Enumerate<ICollection<TElement>, TElement>(collection, token, (p, c) => p.Schedule(c.Count, subProgress));
        }
        private static IEnumerable<TElement> Enumerate<TEnumerable, TElement>(this Progress progress, TEnumerable elements, CancellationToken token, Func<Progress, TEnumerable, Reporter> create)
            where TEnumerable : IEnumerable<TElement>
        {
            token.ThrowIfCancellationRequested();
            using(var reporter = create(progress, elements)) {
                foreach(TElement element in elements) {
                    token.ThrowIfCancellationRequested();
                    yield return element;
                    reporter.Report();
                }
            }
        }
        public static async IAsyncEnumerable<TimeSpan> AtIntervals(this Progress progress, TimeSpan duration, TimeSpan interval, [EnumeratorCancellation] CancellationToken token)
        {
            using(var reporter = progress.Schedule(duration)) {
                await foreach(var timeStamp in new TimerStream(interval).WithCancellation(token).ConfigureAwait(false)) {
                    if(timeStamp > duration) {
                        break;
                    }
                    reporter.Report();
                    yield return timeStamp;
                }
            }
        }
        public static async IAsyncEnumerable<TProgress> Approach<TProgress>(this Progress progress, TProgress target, INonLinearProgress<TProgress> nonLinearProgress, [EnumeratorCancellation] CancellationToken token, TimeSpan interval)
        {
            var linearTarget = nonLinearProgress.Linearise(target);
            var view = new NonLinearView<TProgress>(nonLinearProgress);
            using(var reporter = progress.Schedule(target, view)) {
                Double delta = 0;
                var prevDelta = Double.PositiveInfinity;
                await foreach(var _ in new TimerStream(interval).WithCancellation(token).ConfigureAwait(false)) {
                    if((delta = Math.Abs(linearTarget - view.LinearProgress)) >= prevDelta) {
                        break;
                    }
                    reporter.Report();
                    yield return view.Current;
                    prevDelta = delta;
                }
                yield return view.Current;
            }
        }
        private sealed class NonLinearView<TProgress> : INonLinearProgress<TProgress>
        {
            private Double linearProgress;
            private TProgress current;
            private readonly INonLinearProgress<TProgress> nlProgress;
            public Double LinearProgress => this.linearProgress;
            public TProgress Current => this.current;
            public NonLinearView(INonLinearProgress<TProgress> nlProgress)
            {
                // Before current is accessed, it will be set to some value by
                // a call to Progress() below. Hence, the default will not be used.
                this.current = default!;
                this.linearProgress = 0;
                this.nlProgress = nlProgress;
            }
            public Double Linearise(TProgress progress) => this.linearProgress = this.nlProgress.Linearise(progress);
            public TProgress Progress() => this.current = this.nlProgress.Progress();
        }
    }
}
