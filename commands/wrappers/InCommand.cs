
using System.Threading;
using System.Threading.Tasks;
using progress;

namespace commands.commands
{
    internal sealed class InCommand<TArgument> : ICommand
    {
        private readonly IResult<TArgument> _argument;
        private readonly ICommandIn<TArgument> _command;
        internal InCommand(ICommandIn<TArgument> command, IResult<TArgument> argument)
        {
            _argument = argument;
            _command = command;
        }
        public Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            return _command.Execute(_argument.Result, cancellationToken, progress);
        }
    }
}