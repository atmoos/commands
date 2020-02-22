using System;
using System.Threading;

namespace progressReporting
{
    public interface IProgressView<TProgress> : IProgress<TProgress>
    {
        TProgress Current { get; }
    }
    public sealed class ProgressView : IProgressView<Double>
    {
        private Double _current = default;
        public Double Current => _current;
        public void Report(Double value)
        {
            Interlocked.Exchange(ref _current, value);
        }
    }
    public sealed class ProgressView<TProgress> : IProgressView<TProgress>
        where TProgress : class
    {
        private TProgress _current = default;
        public TProgress Current => _current;

        public void Report(TProgress value)
        {
            Interlocked.Exchange(ref _current, value);
        }
    }
}