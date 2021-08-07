using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class SourceBuilder<TArgument> : IBuilder<TArgument>, IRun<TArgument>
    {
        private readonly IRun pre;
        private readonly ICommandOut<TArgument> argument;
        public Int32 Count => 1 + pre.Count;
        internal SourceBuilder(IRun pre, ICommandOut<TArgument> argument)
        {
            this.pre = pre;
            this.argument = argument;
        }
        public IBuilder Add(ICommandIn<TArgument> command) => new SinkBuilder<TArgument>(this, command);
        public IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command) => new MapBuilder<TArgument, TResult>(this, command);
        public ICommandOut<TArgument> Build() => new CompiledCommand<TArgument>(this);
        public async Task<TArgument> Run(CancellationToken cancellationToken, Progress progress)
        {
            await pre.Run(cancellationToken, progress).ConfigureAwait(false);
            return await argument.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
