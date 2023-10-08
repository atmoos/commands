using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;
using static progressTree.extensions.Flow;

namespace commands.commands;

internal sealed class SequentialCommand : ICommand
{
    private readonly List<ICommand> commands;
    public SequentialCommand(List<ICommand> commands) => this.commands = commands;
    public async Task Execute(CancellationToken cancellationToken, Progress progress)
    {
        foreach(var command in progress.Enumerate(this.commands, cancellationToken)) {
            await command.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
}