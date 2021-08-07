using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class SinkBuilder<TResult> : IBuilder, ICommandChain
    {
        private readonly ICommandChain<TResult> source;
        private readonly ICommandIn<TResult> sink;
        Int32 ICountable.Count => 1 + this.source.Count;
        internal SinkBuilder(ICommandChain<TResult> source, ICommandIn<TResult> sink)
        {
            this.source = source;
            this.sink = sink;
        }
        public IBuilder Add(ICommand command) => new Builder(this, command);
        public IBuilder<TOtherResult> Add<TOtherResult>(ICommandOut<TOtherResult> command) => new SourceBuilder<TOtherResult>(this, command);
        public ICommand Build() => new CompiledCommand(this);
        async Task ICommandChain.Execute(CancellationToken cancellationToken, Progress progress)
        {
            TResult argument = await this.source.Execute(cancellationToken, progress).ConfigureAwait(false);
            await sink.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
