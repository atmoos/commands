using System;

namespace progress.reporters
{
    public sealed class EmptyProgress<TProgress> : IProgress<TProgress>
    {
        public static IProgress<TProgress> Empty { get; } = new EmptyProgress<TProgress>();
        private EmptyProgress() { }
        public void Report(TProgress value)
        {
            // empty
        }
    }
}