
using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands.commands
{
    public sealed class Sink<TArgument> : ICommandIn<TArgument>
    {
        private readonly Action<TArgument> _sink;

        public Sink(Action<TArgument> sink)
        {
            _sink = sink;
        }

        public async Task Execute(TArgument argument, CancellationToken cancellationToken, IProgress<Double> progress)
        {
            await Task.Yield();
            _sink(argument);
        }
    }
}