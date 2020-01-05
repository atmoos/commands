using System;
using System.Threading;
using System.Threading.Tasks;
using progress;

namespace commands.commands
{
    internal sealed class MapCommand<TArgument, TResult> : ICommand<TArgument, TResult>
    {
        private readonly Func<TArgument, CancellationToken, TResult> _func;
        public MapCommand(Func<TArgument, TResult> func) : this((a, _) => func(a)) { }
        public MapCommand(Func<TArgument, CancellationToken, TResult> func) => _func = func;
        public async Task<TResult> Execute(TArgument argument, CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Setup(1)) {
                await Task.Yield();
                return _func(argument, cancellationToken);
            }
        }
    }
}