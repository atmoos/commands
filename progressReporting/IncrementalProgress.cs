using System;

namespace progressReporting
{
    internal delegate void ReportFunc<T>(in T value);
    internal delegate T NextFunc<T>(in T current, in T value);
    internal sealed class IncrementalProgress<T> : IProgress<T>
    {
        private ReportFunc<T> report;
        internal IncrementalProgress(IProgress<T> progress, Func<T, ReportFunc<T>> createReportFunc)
        {
            this.report = (in T value) => { this.report = createReportFunc(value); progress.Report(value); };
        }
        public void Report(T value) => this.report(in value);
    }

    internal sealed class Incremental<T>
        where T : IEquatable<T>
    {
        private T next;
        private readonly NextFunc<T> computeNext;
        private readonly IProgress<T> progress;

        public Incremental(T seed, NextFunc<T> math, IProgress<T> progress)
        {
            this.next = seed;
            this.computeNext = math;
            this.progress = progress;
        }
        public void Report(in T value)
        {
            var next = this.computeNext(in this.next, in value);
            if(next.Equals(this.next)) {
                return;
            }
            this.next = next;
            this.progress.Report(value);
        }
    }

    public static class IncrementalProgressExtensions
    {
        public static IProgress<Decimal> Incremental(this IProgress<Decimal> progress, Decimal increment)
        {
            Decimal Next(in Decimal current, in Decimal value)
            {
                var steps = Math.Floor((value - current) / increment);
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<Double> Incremental(this IProgress<Double> progress, Double increment)
        {
            Double Next(in Double current, in Double value)
            {
                var steps = Math.Floor((value - current) / increment);
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<Single> Incremental(this IProgress<Single> progress, Single increment)
        {
            Single Next(in Single current, in Single value)
            {
                var steps = (Single)Math.Floor((value - current) / increment);
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<Byte> Incremental(this IProgress<Byte> progress, Byte increment)
        {
            Byte Next(in Byte current, in Byte value)
            {
                var steps = (value - current) / increment;
                return (Byte)(current + (steps * increment));
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<Int16> Incremental(this IProgress<Int16> progress, Int16 increment)
        {
            Int16 Next(in Int16 current, in Int16 value)
            {
                var steps = (value - current) / increment;
                return (Int16)(current + (steps * increment));
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<Int32> Incremental(this IProgress<Int32> progress, Int32 increment)
        {
            Int32 Next(in Int32 current, in Int32 value)
            {
                var steps = (value - current) / increment;
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<Int64> Incremental(this IProgress<Int64> progress, Int64 increment)
        {
            Int64 Next(in Int64 current, in Int64 value)
            {
                var steps = (value - current) / increment;
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<UInt64> Incremental(this IProgress<UInt64> progress, UInt64 increment)
        {
            UInt64 Next(in UInt64 current, in UInt64 value)
            {
                var steps = (value - current) / increment;
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<UInt32> Incremental(this IProgress<UInt32> progress, UInt32 increment)
        {
            UInt32 Next(in UInt32 current, in UInt32 value)
            {
                var steps = (value - current) / increment;
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<UInt16> Incremental(this IProgress<UInt16> progress, UInt16 increment)
        {
            UInt16 Next(in UInt16 current, in UInt16 value)
            {
                var steps = (value - current) / increment;
                return (UInt16)(current + (steps * increment));
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<SByte> Incremental(this IProgress<SByte> progress, SByte increment)
        {
            SByte Next(in SByte current, in SByte value)
            {
                var steps = (value - current) / increment;
                return (SByte)(current + (steps * increment));
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<TimeSpan> Incremental(this IProgress<TimeSpan> progress, TimeSpan increment)
        {
            TimeSpan Next(in TimeSpan current, in TimeSpan value)
            {
                var steps = Math.Floor((value - current) / increment);
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        public static IProgress<DateTime> Incremental(this IProgress<DateTime> progress, TimeSpan increment)
        {
            DateTime Next(in DateTime current, in DateTime value)
            {
                var steps = Math.Floor((value - current) / increment);
                return current + (steps * increment);
            }
            return CreateIncremental(progress, Next);
        }
        private static IProgress<T> CreateIncremental<T>(IProgress<T> progress, NextFunc<T> next) where T : IEquatable<T>
        {
            return new IncrementalProgress<T>(progress, seed => new Incremental<T>(seed, next, progress).Report);
        }
    }
}