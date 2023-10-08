using System;
using System.Collections.Generic;
using progressReporting.concurrent;

namespace progressTree;

public static class ProgressExtensions
{
    public static ConcurrentReporter Concurrent(this Progress progress, Norm<Double> norm, Int32 concurrencyLevel) => new(progress.Schedule(1), norm, in concurrencyLevel);
    public static ConcurrentReporter<TItem> Concurrent<TItem>(this Progress progress, Norm<Double> norm, IEnumerable<TItem> items) => new(progress.Schedule(1), norm, items);
    public static ConcurrentReporter Concurrent(this Progress progress, Int32 concurrencyLevel, Norm<Double> norm, IProgress<Double> subProgress) => new(progress.Schedule(1, subProgress), norm, in concurrencyLevel);
    public static ConcurrentReporter<TItem> Concurrent<TItem>(this Progress progress, Norm<Double> norm, IEnumerable<TItem> items, IProgress<Double> subProgress) => new(progress.Schedule(1, subProgress), norm, items);
}