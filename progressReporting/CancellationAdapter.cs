using System;
using System.Threading;

namespace progressReporting;

internal sealed class CancellationAdapter<TProgress> : IProgress<TProgress>
{
    private readonly CancellationToken token;
    public CancellationAdapter(in CancellationToken token) => this.token = token;
    public void Report(TProgress _) => this.token.ThrowIfCancellationRequested();
}