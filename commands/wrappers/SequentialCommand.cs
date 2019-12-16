using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace commands.wrappers
{
    internal sealed class SequentialCommand : ICommand
    {
        private readonly List<ICommand> _commands;
        public SequentialCommand(List<ICommand> commands) => _commands = commands;
        public async Task Execute(CancellationToken cancellationToken, IProgress<Double> progress)
        {
            foreach(var command in _commands) {
                cancellationToken.ThrowIfCancellationRequested();
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}