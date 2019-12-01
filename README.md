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
