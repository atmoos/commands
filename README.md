# A simple and extendable commands library

Writing software often includes having a chain of commands one wants to execute. This library aims at providing just that. A simple command interface

```csharp
Task Execute(CancellationToken cancellationToken, Progress progress);
```

with a bunch of tools that build on that interface.

## Sample Code
The following example illustrates how commands can easily be chained to gether including typesafe output and input arguments.

```csharp
const Double expectedValue = Math.PI;
var command = StartBuilder(() => expectedValue)
                .Add(d => d.ToString("R"))
                .Add(Decimal.Parse)
                .Add(d => (Double)d)
                .Build();
Double roundRobin = await command.Execute(CancellationToken.None, Progress.Empty).ConfigureAwait(false);
Console.WriteLine($"expected: {expectedValue:g4}");
Console.WriteLine($"actual: {roundRobin:g4}");
```

The output is:
```text
expected: 3.142
actual: 3.142
```

## Principles
The four driving principles governing the design of this library are:
- Commands can be __executed__
- Commands support __cancellation__
- Commands report on the current __progress__
- Results and arguments are passed from command to command fully __typed__ and [null safe](https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references)

## Ideas to explore
- Deal with ``IAsyncEnumerable<T>`` elegantly.
- Support a non asynchronous version.
  - Including elegant transition from one to the other.
- Include parallelisation extensions for both progress reporting and commands.
- Make commands monadic, supporting *Select* and *SelectMany*.
