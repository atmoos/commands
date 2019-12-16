using System;

namespace progress
{
    internal sealed class ProcessReporter : IProgress<Double>
    {
        private readonly String _process;
        private readonly IProgress<State> _progress;

        public ProcessReporter(String process, IProgress<State> progress)
        {
            _process = process;
            _progress = progress;
        }
        public void Report(Double progress) => _progress.Report(new State(_process, progress));
    }
}