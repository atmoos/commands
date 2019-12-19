using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using progress;

namespace commands.wrappers
{
    internal sealed class SequentialCommand : ICommand
    {
        private readonly List<ICommand> _commands;
        public SequentialCommand(List<ICommand> commands) => _commands = commands;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(var reporter = progress.Setup(_commands.Count)) {
                foreach(var command in _commands) {
                    cancellationToken.ThrowIfCancellationRequested();
                    await command.Execute(cancellationToken, progress).ConfigureAwait(false);
                    reporter.Report();
                }
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}