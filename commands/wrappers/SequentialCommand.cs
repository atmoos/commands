using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using progress;
using static progress.extensions.Flow;

namespace commands.commands
{
    internal sealed class SequentialCommand : ICommand
    {
        private readonly List<ICommand> _commands;
        public SequentialCommand(List<ICommand> commands) => _commands = commands;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            foreach(var command in progress.Enumerate(_commands, cancellationToken)) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
}