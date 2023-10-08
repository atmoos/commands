using System;

namespace progressReporting;

internal sealed class EmptyProgress<TProgress> : IProgress<TProgress>
{
    internal static IProgress<TProgress> Empty { get; } = new EmptyProgress<TProgress>();
    private EmptyProgress() { }
    public void Report(TProgress _)
    {
        // empty
    }
}