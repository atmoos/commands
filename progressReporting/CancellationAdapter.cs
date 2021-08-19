using System;
using System.Threading;

namespace progressReporting
{
    internal sealed class CancellationAdapter<TProgress> : IProgress<TProgress>
    {
        private readonly CancellationToken _token;
        public CancellationAdapter(in CancellationToken token) => this._token = token;
        public void Report(TProgress _) => this._token.ThrowIfCancellationRequested();
    }
}