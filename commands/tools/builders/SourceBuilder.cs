using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class SourceBuilder<TArgument> : IBuilder<TArgument>, ICommandChain<TArgument>
    {
        private readonly ICommandChain pre;
        private readonly ICommandOut<TArgument> argument;
        public Int32 Count => 1 + pre.Count;
        internal SourceBuilder(ICommandChain pre, ICommandOut<TArgument> argument)
        {
            this.pre = pre;
            this.argument = argument;
        }
        public IBuilder Add(ICommandIn<TArgument> command) => new SinkBuilder<TArgument>(this, command);
        public IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command) => new MapBuilder<TArgument, TResult>(this, command);
        public ICommandOut<TArgument> Build() => new CompiledCommand<TArgument>(this);
        public async Task<TArgument> Execute(CancellationToken cancellationToken, Progress progress)
        {
            await pre.Execute(cancellationToken, progress).ConfigureAwait(false);
            return await argument.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
