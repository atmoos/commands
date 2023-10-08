using System;

namespace progressReporting;

internal sealed class ProgressMap<TIn, TOut> : IProgress<TIn>
{
    private readonly Func<TIn, TOut> map;
    private readonly IProgress<TOut> target;
    public ProgressMap(IProgress<TOut> target, Func<TIn, TOut> map)
    {
        this.target = target;
        this.map = map;
    }

    public void Report(TIn value) => this.target.Report(this.map(value));
}