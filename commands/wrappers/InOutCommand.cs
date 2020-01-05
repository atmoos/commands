using System.Threading;
using System.Threading.Tasks;
using progress;

namespace commands.commands
{
    internal sealed class InOutCommand<TArgument, TResult> : ICommand, IResult<TResult>
    {
        private readonly IResult<TArgument> _resultGetter;
        private readonly ICommand<TArgument, TResult> _command;
        public TResult Result { get; private set; }
        internal InOutCommand(ICommand<TArgument, TResult> command, IResult<TArgument> resultGetter)
        {
            _command = command;
            _resultGetter = resultGetter;
        }
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            Result = await _command.Execute(_resultGetter.Result, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}