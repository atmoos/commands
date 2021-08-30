using System;
using System.Collections.Generic;

namespace progressTree
{
    public static class ProgressExtensions
    {
        public static ConcurrentReporter Concurrent(this Progress progress, Int32 concurrencyLevel) => new(progress.Schedule(1), in concurrencyLevel);
        public static ConcurrentReporter<TItem> Concurrent<TItem>(this Progress progress, IEnumerable<TItem> items) => new(progress.Schedule(1), items);
        public static ConcurrentReporter Concurrent(this Progress progress, Int32 concurrencyLevel, IProgress<Double> subProgress) => new(progress.Schedule(1, subProgress), in concurrencyLevel);
        public static ConcurrentReporter<TItem> Concurrent<TItem>(this Progress progress, IEnumerable<TItem> items, IProgress<Double> subProgress) => new(progress.Schedule(1, subProgress), items);
    }
}