# Commands
Executing a sequence of commands may seem quite a trivial task at first. This holds true for very short sequences that don't exchange much data. When chains of sequences become a bit longer and execute non-trivial tasks, the problem becomes a bit involved.

This library sets out at solving this set of problems:
- Passing data form one command to the next.
    - using C# strong type safety.
    - making it obvious for all involved in a project how data is passed around.
- Building the chain of commands.
- Cancelling the execution of the chain of commands.
- Reporting progress during execution of the chain of commands.

## Concept
In this libraries abstraction of a command, there are five principal properties a command must fulfil:
- Every command can be **executed**.
- Execution of every command can be **cancelled**.
- **Progress** of individual commands or a group of commands can be easily observed.
- **Obvious data dependencies** form one command to the next.
- **Failure** is reported using standard C# semantics with `Exception`s.

### Four Interfaces

The library hinges on four distinct `ICommand` interfaces that each define a single `Execute` method, reflecting the stated properties above: 

```csharp
Task Execute(CancellationToken cancellationToken, Progress progress);

Task Execute(TArgument argument, CancellationToken cancellationToken, Progress progress);

Task<TResult> Execute(CancellationToken cancellationToken, Progress progress);

Task<TResult> Execute(TArgument argument, CancellationToken cancellationToken, Progress progress);
```

- The first three properties (*execute*, *cancel*, *progress*) are obvious, as they are each an explicit part of the signature.
- The fourth property (*data dependency*) is made obvious by defining a distinct interface for each variant of data exchange (none, in, out, both).
- The fifth property (*failure*) is the least obvious property. It is "implemented" by just using standard C# error semantics.

### Extension Methods
This library aims at making it as easy as possible for developers to implement any sequence of commands. For this reason, there is a large set of extension methods that take over most of the plumbing required to integrate arbitrary existing code into a chain of commands.

--> It ought to be able to integrate existing codebases to use this library without having to implement any of the four ``ICommand`` interfaces.

### Command Builder
To easily chain up commands, there is a pre-implemented builder pattern. It allows for obvious chaining of commands reflecting their data dependencies in a type safe manner.

As with the commands, most of this functionality is exposed using extension methods.

### A Note on Asynchronicity
For now the pattern is implemented using the task asynchronous pattern (TAP).
This does not mean that all commands are executed in parallel or out of order. The standard way of chaining commands is in *sequential order*.

Concurrent execution is just an option.
### Distinction from the [GoF Command Pattern](https://en.wikipedia.org/wiki/Command_pattern)
It is a "forward only" command pattern without an "undo" operation. This greatly simplifies dealing with a chain of commands. However, it limits the usefulness of this library should "undo" be required.

The focus is on easily *chaining up* commands.

# Examples

## Method to ICommand
```csharp
public ICommand<Byte[], SomeDomainObject> CreateCommandFormMethod()
{
    static SomeDomainObject SerializeMethod(Byte[] json)
    {
        return JsonSerializer.Deserialize<SomeDomainObject>(json);
    }

    return FuncExtensions.ToCommand<Byte[], SomeDomainObject>(SerializeMethod);
}
```

## Integrate a Method using Builder
Note how the builders reflect which input output type combination is expected for the command (or sequence of commands).
- the input builder expects to receive a command or method that takes an array of bytes as input.
- The method we add takes that array of bytes and serializes it into some domain object.
- Then in the next step in the sequence of commands, we expect a command that takes that serialized domain object as input.
```csharp
public IBuilder<SomeDomainObject> AddCommandToBuilder(IBuilder<Byte[]> builder)
{
    static SomeDomainObject SerializeMethod(Byte[] json)
    {
        return JsonSerializer.Deserialize<SomeDomainObject>(json);
    }

    return builder.Add(SerializeMethod);
}
```

## Building a Chain of Commands
This example focusses on how commands can be chained together in a type safe way. The example focusses on the chaining and, hence, does not show the actual implementations of the methods themselves.

```csharp
// Create an opaque command given some parameters.
public static ICommand EvaluateFromFileSystem(String fileToRead, String pathToPersistTo)
{
    // Read form file -> do some computation -> persist back to file.
    IBuilder<FileInfo> builder = ((Func<FileInfo>)(() => GetFileToProcess(fileToRead))).StartBuilder();
    IBuilder completedBuilder = builder.Add(ReadFile)
                                    .AddComputationEngine()
                                    .Add(content => PersistToFile(pathToPersistTo, content));
    return completedBuilder.Build();
}

// Add "computation engine" that is oblivious from where the input bytes come from,
// or where the resulting output bytes are persisted to.
public static IBuilder<Byte[]> AddComputationEngine(this IBuilder<Byte[]> builder)
{
    return builder.Add(Deserialize).Add(Parse).Add(Compute).Add(Serialize);

    /* The same chain with explicit type arguments: 
    IBuilder<Addition> deserialize = builder.Add(Deserialize);
    IBuilder<(Addition, BinaryOperation)> parse = deserialize.Add(Parse);
    IBuilder<Result> compute = parse.Add(Compute);
    IBuilder<Byte> serialize = compute.Add(Serialize);
    return serialize;
    */
}

// Use the example in a console application.
public static async Task Main(String[] _)
{
    ICommand command = EvaluateFromFileSystem("input.json", "output.json");
    await command.Execute(CancellationToken.None, Progress.Empty).ConfigureAwait(false);
}
```

