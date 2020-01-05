using System.Threading;
using System.Threading.Tasks;
using progress;

namespace commands.commands
{
    internal sealed class OutCommand<TResult> : ICommand, IResult<TResult>
    {
        private readonly ICommandOut<TResult> _command;
        public TResult Result { get; private set; }
        internal OutCommand(ICommandOut<TResult> command) => _command = command;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            Result = await _command.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
}