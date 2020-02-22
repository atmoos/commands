using System;
using System.Collections.Generic;

namespace progressReportingTest
{
    public static class Convenience
    {
        private static readonly Random RAND = new Random(3);
        public static IEnumerable<TProgress> Report<TProgress>(IProgress<TProgress> progress, IEnumerable<TProgress> source)
        {
            foreach(var element in source) {
                progress.Report(element);
                yield return element;
            }
        }
        public static IEnumerable<Int32> RandomIntegers()
        {
            while(true) {
                yield return RAND.Next();
            }
        }
    }
}