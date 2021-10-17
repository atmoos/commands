using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using progressReporting;
using progressReporting.concurrent;
using progressTree;
using progressTree.extensions;

namespace progressTreeTest
{
    public sealed class Examples
    {
        private readonly ILogger logger;
        private static readonly TimeSpan duration = TimeSpan.FromMilliseconds(10 * Math.PI);
        private static readonly TimeSpan interval = TimeSpan.FromMilliseconds(Math.E);

        public Progress Instantiation(IProgress<Double> rootProgress)
        {
            var progress = Progress.Create(rootProgress);
            // progress will be reported strictly monotonic in the range [0, 1].
            return progress;
        }

        public String DiscreteSteps(Int32 number, Progress progress)
        {
            using(var reporter = progress.Schedule(3)) {
                var div = Math.DivRem(number, 4, out var remainder);
                reporter.Report(); // reports the first step: 1/3 = 0.333...
                var result = remainder == 0 ? $"{div}" : $"{div} rem {remainder}";
                reporter.Report(); // reports the second step: 2/3 = 0.666...
                return $"{number}/4 = {result}";
            } // On disposal the last step is reported: 3/3 = 1
        }

        public Int64 DiscreteSteps(ICollection<Int32> numbers, Progress progress)
        {
            using(var reporter = progress.Schedule(numbers.Count)) {
                Int64 sum = 0;
                foreach(var number in numbers) {
                    sum += number;
                    reporter.Report(); // reports the i-th step: i/Count
                }
                return sum;
            }
        }

        public Int64 DiscreteStepsUsingExtensionMethod(ICollection<Int32> numbers, Progress progress)
        {
            Int64 sum = 0;
            foreach(var number in progress.Enumerate(numbers)) {
                sum += number;
            }
            return sum;
        }

        public void TimeBasedReporting(TimeSpan duration, TimeSpan interval, CancellationToken token, Progress progress)
        {
            using(var reporter = progress.Schedule(duration)) {
                var timer = Stopwatch.StartNew();
                while(timer.Elapsed < duration) {
                    Task.Delay(interval, token).GetAwaiter().GetResult();
                    reporter.Report();
                }
            }
        }

        public void NestedReporting(Progress progress)
        {
            using(progress.Schedule(4)) {
                DiscreteSteps(42, progress); // [0, 0.25]
                DiscreteSteps(new List<Int32> { 23, 8, 11 }, progress); // ]0.25, 0.5]
                TimeBasedReporting(duration, interval, token: default, progress); // ]0.5, 0.75]
                NonLinearReporting(137, progress); // ]0.75, 1]
            }
        }

        public void InterfacingWithStandardProgressReporting(Progress progress)
        {
            using(var reporter = progress.Schedule(1)) {
                // the Progress property expects reports in interval [0, 1]
                IProgress<Double> standardProgress = reporter.Progress;
                // create IProgress instance that accepts reports in integer percentage [0, 100]
                IProgress<Int32> progressInPercentage = standardProgress.Map((Int32 percent) => percent / 100d);
                ExternalCodeUsing(progressInPercentage);
            }
        }

        public void ReportSubProgressSeparately(Progress progress)
        {
            IProgress<String> externalProgress = null /* e.g. through constructor injection */;
            IProgress<Double> subProgress = externalProgress.Map((Double p) => $"Sub is at {p:P}");
            using(progress.Schedule(4, subProgress)) {
                DiscreteSteps(1, progress); // [0, 0.25]
                DiscreteSteps(2, progress); // ]0.25, 0.5]
                DiscreteSteps(4, progress); // ]0.5, 0.75]
                DiscreteSteps(8, progress); // ]0.75, 1]
            }
        }

        public async Task ConcurrentReporting(Progress progress)
        {
            static Task LongRunning(Progress progress) => /* Dummy */ Task.CompletedTask;

            static async Task FirstToCompleteWins(Progress progress)
            {
                // By using the maximum norm, the highest progress value
                // is reported, thus matching the call to Task.WhenAny
                using(var concurrentProgress = progress.Concurrent(Norm.Max, 2)) {
                    var taskA = LongRunning(concurrentProgress[0]);
                    var taskB = LongRunning(concurrentProgress[1]);
                    await Task.WhenAny(taskA, taskB).ConfigureAwait(false);
                }
            }

            // By using the minimum norm, the lowest progress value
            // is reported, thus matching the call to Task.WhenAll
            using(var concurrentProgress = progress.Concurrent(Norm.Min, 2)) {
                var someTask = LongRunning(concurrentProgress[0]);
                var someOtherTask = FirstToCompleteWins(concurrentProgress[1]);
                await Task.WhenAll(someTask, someOtherTask).ConfigureAwait(false);
            }
        }
        public void NonLinearReporting(Double targetTemperature, Progress progress)
        {
            // ToDo: Create an expert section!
            static Double HeatingModel(Double time) => 3 * Math.Log(time - 1d);
            static Double InverseHeatingModel(Double temp) => Math.Exp(temp / 3d) + 1;
        }

        private interface ILogger
        {
            void Log(String message);
        }

        private static void ExternalCodeUsing(IProgress<Int32> progress) { }
    }
}