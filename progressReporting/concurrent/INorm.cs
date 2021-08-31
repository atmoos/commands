using System.Collections.Generic;

namespace progressReporting.concurrent
{
    public interface INorm<T>
    {
        void Update(in T current, IEnumerable<T> others);
    }
}