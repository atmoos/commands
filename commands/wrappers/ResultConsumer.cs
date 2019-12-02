
using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands.wrappers
{
    internal sealed class ResultConsumer<TArgument> : ICommand
    {
        private readonly IResult<TArgument> _argument;
        private readonly ICommandIn<TArgument> _command;

        internal ResultConsumer(ICommandIn<TArgument> command, IResult<TArgument> argument){
            _argument = argument;
            _command = command;
        }       
        public Task Execute(CancellationToken cancellationToken, IProgress<Double> progress){
            return _command.Execute(_argument.Result, cancellationToken, progress);
        }
    }
}