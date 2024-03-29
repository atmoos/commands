
using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.commands;

internal sealed class SinkCommand<TArgument> : ICommandIn<TArgument>
{
    private readonly Action<TArgument, CancellationToken> sink;
    public SinkCommand(Action<TArgument> sink) : this((a, _) => sink(a)) { }
    public SinkCommand(Action<TArgument, CancellationToken> sink) => this.sink = sink;
    public async Task Execute(TArgument argument, CancellationToken cancellationToken, Progress progress)
    {
        using(progress.Schedule(1)) {
            await Task.Yield();
            this.sink(argument, cancellationToken);
        }
    }
}