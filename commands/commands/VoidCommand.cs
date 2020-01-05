using System;
using System.Threading;
using System.Threading.Tasks;
using progress;

namespace commands.commands
{
    internal sealed class VoidCommand : ICommand
    {
        private readonly Action<CancellationToken> _action;
        public VoidCommand(Action action) : this(_ => action()) { }
        public VoidCommand(Action<CancellationToken> action) => _action = action;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Setup(1)) {
                await Task.Yield();
                _action(cancellationToken);
            }
        }
    }
}
