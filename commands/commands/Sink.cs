
using System;
using System.Threading;
using System.Threading.Tasks;
using progress;

namespace commands.commands
{
    internal sealed class Sink<TArgument> : ICommandIn<TArgument>
    {
        private readonly Action<TArgument, CancellationToken> _sink;

        public Sink(Action<TArgument> sink) : this((a, _) => sink(a)) { }
        public Sink(Action<TArgument, CancellationToken> sink)
        {
            _sink = sink;
        }
        public async Task Execute(TArgument argument, CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Setup(1)) {
                await Task.Yield();
                _sink(argument, cancellationToken);
            }
        }
    }
}