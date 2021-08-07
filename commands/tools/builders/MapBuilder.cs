using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class MapBuilder<TArgument, TResult> : IBuilder<TResult>, ICommandChain<TResult>
    {
        private readonly ICommandChain<TArgument> source;
        private readonly ICommand<TArgument, TResult> map;
        public Int32 Count => 1 + source.Count;
        internal MapBuilder(ICommandChain<TArgument> source, ICommand<TArgument, TResult> map)
        {
            this.source = source;
            this.map = map;
        }
        public IBuilder Add(ICommandIn<TResult> command) => new SinkBuilder<TResult>(this, command);
        public IBuilder<TOtherResult> Add<TOtherResult>(ICommand<TResult, TOtherResult> command) => new MapBuilder<TResult, TOtherResult>(this, command);
        public ICommandOut<TResult> Build() => new CompiledCommand<TResult>(this);
        public async Task<TResult> Execute(CancellationToken cancellationToken, Progress progress)
        {
            var argument = await source.Execute(cancellationToken, progress).ConfigureAwait(false);
            return await map.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
