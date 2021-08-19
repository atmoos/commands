using System;

namespace progressReporting
{
    internal sealed class ObservableProgress<TProgress> : IProgress<TProgress>
    {
        private readonly Action<TProgress> _observer;
        internal ObservableProgress(Action<TProgress> observer) => this._observer = observer;
        public void Report(TProgress value) => this._observer(value);
    }
}