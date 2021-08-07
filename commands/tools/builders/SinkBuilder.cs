using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class SinkBuilder<TResult> : IBuilder, IRun
    {
        private readonly IRun<TResult> source;
        private readonly ICommandIn<TResult> sink;
        public Int32 Count => 1 + source.Count;
        internal SinkBuilder(IRun<TResult> source, ICommandIn<TResult> sink)
        {
            this.source = source;
            this.sink = sink;
        }
        public IBuilder Add(ICommand command) => new Builder(this, command);
        public IBuilder<TOtherResult> Add<TOtherResult>(ICommandOut<TOtherResult> command) => new SourceBuilder<TOtherResult>(this, command);
        public ICommand Build() => new CompiledCommand(this);
        public async Task Run(CancellationToken cancellationToken, Progress progress)
        {
            TResult argument = await source.Run(cancellationToken, progress).ConfigureAwait(false);
            await sink.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
