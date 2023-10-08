using System;

namespace progressTree;

public interface INonLinearProgress<TProgress>
{
    TProgress Progress();
    Double Linearise(TProgress progress);
}