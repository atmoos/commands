using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class CompiledCommand : ICommand
    {
        private readonly IRun runner;
        public CompiledCommand(IRun runner) => this.runner = runner;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(runner.Count)) {
                await runner.Run(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class CompiledCommand<TResult> : ICommandOut<TResult>
    {
        private readonly IRun<TResult> runner;
        public CompiledCommand(IRun<TResult> runner) => this.runner = runner;
        public async Task<TResult> Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(this.runner.Count)) {
                return await this.runner.Run(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
}
