using System;

namespace progress
{
    public class ProgressTree : IProgress<Double>
    {
        private Double _current;
        private readonly Double _weight;
        private readonly IProgress<Double> _progress;

        public ProgressTree Parent { get; }

        private ProgressTree(IProgress<Double> progressRoot)
        {
            _weight = 1d;
            _current = 0d;
            Parent = this;
            _progress = progressRoot;
        }
        private ProgressTree(ProgressTree parent, IProgress<Double> progress, Double weight)
        : this(progress)
        {
            if(weight < 0d || weight > 1d) {
                var msg = $"Acepted range [0, 1]. Recieved: {weight:G3}";
                throw new ArgumentOutOfRangeException(nameof(weight), weight, msg);
            }
            _weight = weight;
            _progress = progress;
            Parent = parent;
        }
        public void Report(Double progress)
        {
            _current = progress;
            UdateUpstream(progress);
        }
        private void UdateUpstream(Double increment)
        {
            // Root
            if(Parent == this) {
                _progress.Report(increment);
                return;
            }
            // Chain
            if(_progress == Parent) {
                Parent.Increment(increment);
                return;
            }
            // Branch!
            _progress.Report(increment);
            Parent.Increment(increment);
        }
        private void Increment(Double increment) => UdateUpstream(_current + _weight * increment);
        public static ProgressTree Root(IProgress<Double> progress) => new ProgressTree(progress);
        public static ProgressTree Chain(ProgressTree parent, Double weight) => new ProgressTree(parent, parent, weight);
        public static ProgressTree Branch(ProgressTree parent, IProgress<Double> progress, Double weight) => new ProgressTree(parent, progress, weight);
    }
}