using System;

namespace progress
{
    public sealed class Progress
    {
        private Double _current;
        private readonly String _process;
        private readonly IProgress<State> _progress;
        private Progress(String process, IProgress<State> progress){
            _current = -1d;
            _process = process;
            _progress = progress;
        }

        private Progress Chain() => this; // ToDo!
        public Reporter Setup(Int32 iterations) => Reporter.Create(Chain(), iterations);
        public Reporter Setup(String subProcess, Int32 iterations) => Reporter.Create(Create(subProcess, _progress), iterations);
        public Reporter Setup(TimeSpan expectedDuration) => Reporter.Create(Chain(), expectedDuration);
        public Reporter Setup(String subProcess, TimeSpan expectedDuration) => Reporter.Create(Create(subProcess, _progress), expectedDuration);
        internal void Report(Double progress)
        {
            if(progress > _current && 0d <= progress && progress <= 1d){
                _current = progress;
                _progress.Report(new State(_process, progress));
            }
        }
        public static Progress Create(String process, IProgress<State> progress) => new Progress(process, progress);
    }
}