using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace progressReporting
{
    public sealed class ParallelProgress<T>
     where T : struct, IComparable<T>
    {
        private readonly IProgress<T> root;
        private readonly Interceptor[] interceptors;

        private ParallelProgress(IProgress<T> root, IEnumerable<IProgress<T>> progress)
        {
            this.root = root;
            this.interceptors = progress.Select(p => new Interceptor(this, p)).ToArray();
        }
        private void Report(in T value)
        {
            // ToDo: Make thread safe!!
            var currentState = this.interceptors.OrderBy(i => i.Current).ToArray();
            var minValue = currentState[0].Current;
            var secondToMin = currentState[1].Current;
            Int32 minComparison;
            if((minComparison = minValue.CompareTo(value)) <= 0) {
                if(minComparison == 0 || value.CompareTo(secondToMin) <= 0) {
                    this.root.Report(value);
                }
            }
        }

        public static IEnumerable<IProgress<T>> Intercept(IProgress<T> root, IEnumerable<IProgress<T>> progress)
        {
            return new ParallelProgress<T>(root, progress).interceptors;
        }

        private sealed class Interceptor : IProgress<T>
        {

            public IProgress<T> wrapped;
            public ParallelProgress<T> parent;
            public T Current { get; private set; }

            public Interceptor(ParallelProgress<T> parent, IProgress<T> wrapped)
            {
                this.parent = parent;
                this.wrapped = wrapped;
            }

            void IProgress<T>.Report(T value)
            {
                this.parent.Report(in value);
                Current = value;
                this.wrapped.Report(value);
            }
        }
    }
}