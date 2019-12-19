using System;
using System.Threading;

namespace progress
{
    internal sealed class CancellationAdapter<TProgress> : IProgress<TProgress>
    {
        private readonly CancellationToken _token;
        public CancellationAdapter(CancellationToken token) => _token = token;
        public void Report(TProgress _) => _token.ThrowIfCancellationRequested();
    }
}