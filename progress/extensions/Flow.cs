using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace progress.extensions
{
    public static class Flow
    {
        public static IEnumerable<TElement> Enumerate<TElement>(this Progress progress, ICollection<TElement> collection, CancellationToken token)
        {
            return progress.Enumerate<ICollection<TElement>, TElement>(collection, token, (p, c) => p.Setup(c.Count));
        }
        public static IEnumerable<TElement> Enumerate<TElement>(this Progress progress, ICollection<TElement> collection, CancellationToken token, IProgress<Double> subProgress)
        {
            return progress.Enumerate<ICollection<TElement>, TElement>(collection, token, (p, c) => p.Setup(c.Count, subProgress));
        }
        private static IEnumerable<TElement> Enumerate<TEnumerable, TElement>(this Progress progress, TEnumerable elements, CancellationToken token, Func<Progress, TEnumerable, Reporter> create)
            where TEnumerable : IEnumerable<TElement>
        {
            token.ThrowIfCancellationRequested();
            using(Reporter reporter = create(progress, elements)) {
                foreach(TElement element in elements) {
                    token.ThrowIfCancellationRequested();
                    yield return element;
                    reporter.Report();
                }
            }
        }
        public static async IAsyncEnumerable<TimeSpan> AtIntervalls(this Progress progress, TimeSpan duration, TimeSpan interval, [EnumeratorCancellation]CancellationToken token)
        {
            using(Reporter reporter = progress.Setup(duration)) {
                await using(var enumerator = new TimerStream(interval).GetAsyncEnumerator(token))
                    while(await enumerator.MoveNextAsync() && enumerator.Current < duration) {
                        reporter.Report();
                        yield return enumerator.Current;
                    }
            }
        }

        public static async IAsyncEnumerable<TProgress> Approach<TProgress>(this Progress progress, TProgress target, INonLinearProgress<TProgress> nonLinearProgress, [EnumeratorCancellation]CancellationToken token, TimeSpan interval)
        {
            var linearTarget = nonLinearProgress.Linearize(target);
            var view = new NonLinearView<TProgress>(nonLinearProgress);
            using(Reporter reporter = progress.Setup(target, view)) {
                Double delta = 0;
                var prevDelta = Double.PositiveInfinity;
                while((delta = Math.Abs(linearTarget - view.LinearProgress)) < prevDelta) {
                    yield return view.Current;
                    await Task.Delay(interval, token).ConfigureAwait(false);
                    reporter.Report();
                    prevDelta = delta;
                }
                yield return view.Current;
            }
        }

        private sealed class NonLinearView<TProgress> : INonLinearProgress<TProgress>
        {
            private Double _linearProgress;
            private TProgress _current;
            private readonly INonLinearProgress<TProgress> _nlProgress;
            public Double LinearProgress => _linearProgress;
            public TProgress Current => _current;
            public NonLinearView(INonLinearProgress<TProgress> nlProgress)
            {
                _current = default;
                _linearProgress = 0;
                _nlProgress = nlProgress;
            }
            public Double Linearize(TProgress progress) => (_linearProgress = _nlProgress.Linearize(progress));
            public TProgress Progress() => (_current = _nlProgress.Progress());
        }
    }

    internal class Flow<TProgress> : IProgress<TProgress>, IEnumerable<TProgress>
    {
        public IEnumerator<TProgress> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Report(TProgress value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}