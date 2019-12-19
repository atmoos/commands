# A simple and extendable commands library

Writing software often includes having a chain of commands one wants to execute. This library aims at providing just that. A simple command interface

```csharp
Task Execute(CancellationToken cancellationToken, IProgress<T> progress);
```

with a bunch of tools that build on that interface.

## Principles
- asynchonous
- functional
- imutable
- easy to implement and read
- free tooling
- dendency injection
- tested

## Ideas
- Combine progress and cancellation on the same type?
- Have extensions methods on Progress
- Have wrappers that wrap command progress in any IProgress<T> via metric mappers (mapping T -> Double). Useful to integrate other progress reporting methods.
- Have progress that reports on the state of a (linear) process with (lower?) and upper bounds. For, say, the state of a heating process that should reach a certain temperature.