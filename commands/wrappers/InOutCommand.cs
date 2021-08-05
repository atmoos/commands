using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.commands
{
    internal sealed class InOutCommand<TArgument, TResult> : ICommandOut<TResult>
    {
        private readonly ICommandOut<TArgument> argument;
        private readonly ICommand<TArgument, TResult> consumer;
        public InOutCommand(ICommandOut<TArgument> argument, ICommand<TArgument, TResult> consumer)
        {
            this.argument = argument;
            this.consumer = consumer;
        }
        public async Task<TResult> Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(2)) {
                var argument = await this.argument.Execute(cancellationToken, progress).ConfigureAwait(false);
                return await this.consumer.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
}