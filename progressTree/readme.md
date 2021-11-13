ToDo:
- No performance optimisations yet...



# Progress Tree
Reporting the progress of some process is usually a valuable piece of information for the user of a piece of software, but by no means something trivial to implement. The dotnet interface `IProgress<T>` is a good starting point to build on, but itself does not provide a pattern by which meaningful progress can be reported.

This library attempts to provide a pattern and implementation by which meaningful progress reporting can be easily achieved.

## Definition of Progress
A definition of progress is used that is assumed to be generic in nature and enable meaningful design decisions to be made.

- Progress is reported in the closed interval [0, 1].
- The two endpoints "0" and "1" are guaranteed to be reported.
- Progress is strictly monotonic increasing.

With this definition, it is possible to report progress reliably and in a meaningful way.

## Out of scope
We all know of progress bars that reach a value close to 100% quite quickly - say 98% - but then stay there completing those 2% in more time it took to reach 98%. This occurs when the mapping from actual progress to the reported state of overall progress is bad (skewed).

This library cannot solve that issue. It only provides tools that should aid in not running into that situation.

## Design approach
The design builds of the `IProgress<T>` interface and is extendable through it. The design aims at two common scenarios of reporting progress. It is known a priori...
- how many steps will be executed before completion.
- how much time a (sub-) process will take to execute.

Usually processes branch out into (sequential) sub tasks, which themselves might branch out once again and so on until leaf tasks are reached that no longer branch out. Essentially creating a process **tree**. In order to report progress, a mapping from the _process tree_ to some **progress tree** must be found.
This library abstracts that away from each sub task and maps the _process tree_ automatically.

No sub task needs to have any knowledge about the overall process in which it is embedded.

# Examples
These are very basic examples of how this library can (and should) be used. There are extension methods available that simplify some scenarios.

## Report Progress of Steps
Here's an example of a leaf task that consists of three simple steps. After each step progress is reported in increments of 1/3.
```csharp
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
```

Similarly, the input of a sub task may be the basis of the steps that need to be performed.
```csharp
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
```
See also extension methods further below.

## Report Progress of Time Based Processes
When a process completes based on a pre-defined duration, progress reporting can be scheduled based on time directly:
```csharp
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
```

## Nested Processes
Nesting functions that individually report their progress is also very easily possible.
```csharp
public void NestedReporting(Progress progress)
{
    using(progress.Schedule(4)) {
        DiscreteSteps(42, progress); // [0, 0.25]
        DiscreteSteps(new List<Int32> { 23, 8, 11 }, progress); // ]0.25, 0.5]
        TimeBasedReporting(duration, interval, token: default, progress); // ]0.5, 0.75]
        NonLinearReporting(137, progress); // ]0.75, 1]
    }
}
```

## Integrating `IProgress<T>` Dependencies
When there is a dependency to some function that reports progress using `IProgress<T>` that can be integrated into the progress tree as well.

Here is some function that reports its progress in integer percent.
```csharp
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
```

## Separate Reporting of Sub-Processes
Say a sub-progress is of particular interest and it's progress is to be reported separately while still reporting to the overarching process.

This is achieved by using the overload of `Schedule` that accepts a parameter of `IProgress<Double>`.
```csharp
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
```
The `subProgress` instance will receive progress updates in the regular interval [0, 1], enabling meaningful progress reporting of that particular function.

The function will nevertheless still contribute to overall progress (should it be part of any), just within a narrower progress window.

# Extension Methods
A common pattern is to iterate over a collection of items and then perform some work using each element. An extension method simplifies the plumbing for exactly that use case:
```csharp
public Int64 DiscreteStepsUsingExtensionMethod(ICollection<Int32> numbers, Progress progress)
{
    Int64 sum = 0;
    foreach(var number in progress.Enumerate(numbers)) {
        sum += number;
    }
    return sum;
}
```

# Reporting Progress of Concurrent Processes
Integrating concurrent process into the progress tree is enabled via a set of extension methods.

When two processes report progress concurrently (i.e. at the same time), it must be decided how these simultaneous reports are mapped to the root `progress`. This is achieved by choosing an appropriate norm.

```csharp
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
```

When all concurrent tasks should be awaited (`Task.WhenAll`), it doesn't make sense to propagate progress from the fastest task up the progress tree, as that would lead to the situation we all don't like to encounter: fast progress at the beginning that then grinds to a halt shortly before 100% is reached.
Thus, progress from the slowest task should be reported. This is achieved by using the minimum norm.

If, however, the first task to completes wins (`Task.WhenAny`), progress of that task should be reported. This is achieved by using the maximum norm.

In short:
- `Task.WhenAny` -> `Norm.Max`
- `Task.WhenAll` -> `Norm.Min`

