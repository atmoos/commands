
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.commands
{
    internal sealed class Command<TArgument> : ICommand
    {
        private readonly ICommandOut<TArgument> argument;
        private readonly ICommandIn<TArgument> consumer;
        internal Command(ICommandOut<TArgument> argument, ICommandIn<TArgument> consumer)
        {
            this.argument = argument;
            this.consumer = consumer;
        }
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(2)) {
                var argument = await this.argument.Execute(cancellationToken, progress).ConfigureAwait(false);
                await this.consumer.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
}