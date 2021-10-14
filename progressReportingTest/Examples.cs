using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using progressReporting;

using static System.Console;
using static progressReporting.Extensions;
using static progressReporting.IncrementalProgressExtensions;

namespace progressReportingTest
{
    public static class Examples
    {
        public static IProgress<T> Filtering<T>(IProgress<T> progress)
            where T : IComparable<T>
        {
            T lower = default; // some value
            T upper = default; // some value larger than lower
            IProgress<T> bounded = progress.Bounded(lower, upper).Inclusive();
            IProgress<T> monotonicDecreasing = bounded.Monotonic().Decreasing();
            IProgress<T> monotonicStrictlyIncreasing = bounded.Monotonic().Strictly.Increasing();
            return monotonicDecreasing.Zip(monotonicStrictlyIncreasing);
        }

        public static IProgress<Double> NumericFiltering(IProgress<Double> progress)
        {
            IProgress<Double> incremental = progress.Incremental(increment: 0.1);
            return incremental;
        }

        public static IProgress<T> Mapping<T>(IProgress<String> progress)
        {
            IProgress<T> mapsToString = progress.Map((T p) => p.ToString());
            return mapsToString;
        }
        public static IProgress<T> Aggregation<T>(IProgress<T> channelA, IProgress<T> channelB)
        {
            IProgress<T> combined = channelA.Zip(channelB);
            return combined;
        }
        public static IProgress<T> Convenience<T>()
        {
            IProgress<T> progress = Empty<T>();

            IProgress<T> observable = progress.Observable((T p) => WriteLine($"Current progress: {p}"));

            ProgressRecorder<T> recorder = new();
            T progressAtPosition = recorder[3];
            T lastRecordedValue = recorder[^1];
            IEnumerable<T> filteredValues = recorder.Where(p => p is not null);

            return progress;
        }
    }
}