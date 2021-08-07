using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class CompiledCommand : ICommand
    {
        private readonly ICommandChain chain;
        public CompiledCommand(ICommandChain chain) => this.chain = chain;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(chain.Count)) {
                await chain.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class CompiledCommand<TResult> : ICommandOut<TResult>
    {
        private readonly ICommandChain<TResult> chain;
        public CompiledCommand(ICommandChain<TResult> chain) => this.chain = chain;
        public async Task<TResult> Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(this.chain.Count)) {
                return await this.chain.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
}
