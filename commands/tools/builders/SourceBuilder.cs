using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders;

internal sealed class SourceBuilder<TArgument> : IBuilder<TArgument>, ICommandChain<TArgument>
{
    private readonly ICommandChain pre;
    private readonly ICommandOut<TArgument> sink;
    Int32 ICountable.Count => 1 + this.pre.Count;
    internal SourceBuilder(ICommandChain pre, ICommandOut<TArgument> sink)
    {
        this.pre = pre;
        this.sink = sink;
    }
    public IBuilder Add(ICommandIn<TArgument> command) => new SinkBuilder<TArgument>(this, command);
    public IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command) => new MapBuilder<TArgument, TResult>(this, command);
    public ICommandOut<TArgument> Build() => new CompiledCommand<TArgument>(this);
    async Task<TArgument> ICommandChain<TArgument>.Execute(CancellationToken cancellationToken, Progress progress)
    {
        await this.pre.Execute(cancellationToken, progress).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        return await this.sink.Execute(cancellationToken, progress).ConfigureAwait(false);
    }
}
