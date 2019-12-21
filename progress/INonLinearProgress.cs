using System;

namespace progress
{
    public interface INonLinearProgress<TProgress>
    {
        TProgress Progress();
        Double Linearize(TProgress progress);
    }
}