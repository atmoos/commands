using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.commands
{
    internal sealed class MapCommand<TArgument, TResult> : ICommand<TArgument, TResult>
    {
        private readonly Func<TArgument, CancellationToken, TResult> func;
        public MapCommand(Func<TArgument, TResult> func) : this((a, _) => func(a)) { }
        public MapCommand(Func<TArgument, CancellationToken, TResult> func) => this.func = func;
        public async Task<TResult> Execute(TArgument argument, CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(1)) {
                await Task.Yield();
                return this.func(argument, cancellationToken);
            }
        }
    }
}