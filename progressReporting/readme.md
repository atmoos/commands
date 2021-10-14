# Progress Reporting
This library contains generic functionality on the `IProgress<T>` interface. In particular, filter and mapping functions.

## Concept
Similar to `Linq`, all functionality is exposed via extension methods. This allows for an API that lets you create fluent and easily readable code.

## Examples
These examples all assume there is some instance (client) of `IProgress<T>` to which progress is to be reported. This is the progress root.

Also assume there is some pre-processing of the raw incoming progress reports before they should be reported. A typical scenario would be rate limiting or type conversion.

ToDo: Graphic:

raw `IProgress<T'>` --> [pre-processing] --> report to root `IProgress<T>`

### Filters
Generic filters on types that implement `IComparable<T>`:
```csharp
IProgress<T> progress = /* progress root */;
IProgress<T> bounded = progress.Bounded(lower, upper).Inclusive();
IProgress<T> monotonicDecreasing = bounded.Monotonic().Decreasing();
IProgress<T> monotonicStrictlyIncreasing = bounded.Monotonic().Strictly.Increasing();
```

Report progressing in increments no smaller than a given value on numeric types:
```csharp
IProgress<Double> progress = /* progress root */;
IProgress<Double> incremental = progress.Incremental(increment: 0.1);
```
### Mapping
Progress can be mapped from any type to any other type, by just using a simple (lambda) expression. In this example from any type to a string.
```csharp
IProgress<String> progress = /* consumes progress reports as string */;
IProgress<T> mapsToString = progress.Map((T p) => p.ToString());
```
### Aggregation
Aggregation of progress via `Zip` is currently the only aggregation available.
```csharp
IProgress<Double> channelA = /* report to some independent channel */;
IProgress<Double> channelB = /* report to some other independent channel */;
IProgress<T> combined = channelA.Zip(channelB);
```
### Convenience
Convenience components that are largely useful for writing unit tests. Also, for when no progress is needed at all, i.e. `Empty` progress. 
```csharp
IProgress<T> progress = Empty<T>();

IProgress<T> observable = progress.Observable((T p) => WriteLine($"Current progress: {p}"));

ProgressRecorder<T> recorder = new();
T progressAtPosition = recorder[3];
T lastRecordedValue = recorder[^1];
IEnumerable<T> filteredValues = recorder.Where(p => p is not null);
```

