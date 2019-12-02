
using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands.wrappers
{
    internal sealed class ArgWrapper<TArgument> : ICommand
    {
        private readonly Func<TArgument> _argument;
        private readonly ICommandIn<TArgument> _command;

        internal ArgWrapper(ICommandIn<TArgument> command, Func<TArgument> argument){
            _argument = argument;
            _command = command;
        }       
        public Task Execute(CancellationToken cancellationToken, IProgress<Double> progress){
            return _command.Execute(_argument(), cancellationToken, progress);
        }
    }
}