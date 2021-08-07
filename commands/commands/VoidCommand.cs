using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.commands
{
    internal sealed class VoidCommand : ICommand
    {
        private readonly Action<CancellationToken> action;
        public VoidCommand(Action action) : this(_ => action()) { }
        public VoidCommand(Action<CancellationToken> action) => this.action = action;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(1)) {
                await Task.Yield();
                this.action(cancellationToken);
            }
        }
    }
}
