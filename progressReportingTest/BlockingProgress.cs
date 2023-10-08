using System;
using System.Threading;

namespace progressReportingTest;

internal sealed class BlockingProgress<TProgress> : IProgress<TProgress>, IDisposable
{
    private readonly IProgress<TProgress> parent;
    private readonly ManualResetEventSlim block = new(initialState: false);
    public BlockingProgress(IProgress<TProgress> parent) => this.parent = parent;
    public void Report(TProgress value)
    {
        this.block.Wait();
        this.parent.Report(value);
    }

    // This seems the wrong way round, but isn't..
    // Docs: "Sets the state of the event to nonsignaled, which causes threads to block."
    public void Block() => this.block.Reset();
    public void Unblock() => this.block.Set();

    void IDisposable.Dispose() => this.block.Dispose();
}