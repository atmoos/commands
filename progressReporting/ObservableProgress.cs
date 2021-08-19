using System;

namespace progressReporting
{
    internal sealed class ObservableProgress<TProgress> : IProgress<TProgress>
    {
        private readonly Action<TProgress> observer;
        internal ObservableProgress(Action<TProgress> observer) => this.observer = observer;
        public void Report(TProgress value) => this.observer(value);
    }
}