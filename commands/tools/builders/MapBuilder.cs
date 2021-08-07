using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class MapBuilder<TArgument, TResult> : IBuilder<TResult>, IRun<TResult>
    {
        private readonly IRun<TArgument> source;
        private readonly ICommand<TArgument, TResult> map;
        public Int32 Count => 1 + source.Count;
        internal MapBuilder(IRun<TArgument> source, ICommand<TArgument, TResult> map)
        {
            this.source = source;
            this.map = map;
        }
        public IBuilder Add(ICommandIn<TResult> command) => new SinkBuilder<TResult>(this, command);
        public IBuilder<TOtherResult> Add<TOtherResult>(ICommand<TResult, TOtherResult> command) => new MapBuilder<TResult, TOtherResult>(this, command);
        public ICommandOut<TResult> Build() => new CompiledCommand<TResult>(this);
        public async Task<TResult> Run(CancellationToken cancellationToken, Progress progress)
        {
            var argument = await source.Run(cancellationToken, progress).ConfigureAwait(false);
            return await map.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
