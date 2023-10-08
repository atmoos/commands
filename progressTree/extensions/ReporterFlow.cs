using System;
using System.Collections.Generic;
using System.Threading;

namespace progressTree.extensions;

public static class ReporterFlow
{
    public static IEnumerable<TElement> Enumerate<TElement>(this Progress progress, ICollection<TElement> collection, CancellationToken token)
        where TElement : IReportProgress
    {
        return progress.Enumerate<ICollection<TElement>, TElement>(collection, token, (p, c) => p.Schedule(c.Count));
    }
    public static IEnumerable<TElement> Enumerate<TElement>(this Progress progress, ICollection<TElement> collection, CancellationToken token, IProgress<Double> subProgress)
        where TElement : IReportProgress
    {
        return progress.Enumerate<ICollection<TElement>, TElement>(collection, token, (p, c) => p.Schedule(c.Count, subProgress));
    }
    private static IEnumerable<TElement> Enumerate<TEnumerable, TElement>(this Progress progress, TEnumerable elements, CancellationToken token, Func<Progress, TEnumerable, Reporter> create)
        where TEnumerable : IEnumerable<TElement>
        where TElement : IReportProgress
    {
        token.ThrowIfCancellationRequested();
        using(Reporter reporter = create(progress, elements)) {
            foreach(TElement element in elements) {
                token.ThrowIfCancellationRequested();
                yield return element;
            }
        }
    }
}