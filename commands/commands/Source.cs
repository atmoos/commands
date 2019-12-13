
using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands.commands
{
    public sealed class Source<TResult> : ICommandOut<TResult>
    {
        private readonly Func<TResult> _source;

        public Source(Func<TResult> source)
        {
            _source = source;
        }

        public async Task<TResult> Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            await Task.Yield();
            return _source();
        }
    }
}