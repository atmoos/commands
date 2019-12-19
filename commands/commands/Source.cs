
using System;
using System.Threading;
using System.Threading.Tasks;
using progress;

namespace commands.commands
{
    internal sealed class Source<TResult> : ICommandOut<TResult>
    {
        private readonly Func<CancellationToken, TResult> _source;

        public Source(Func<TResult> source) : this(_ => source()) { }
        public Source(Func<CancellationToken, TResult> source)
        {
            _source = source;
        }

        public async Task<TResult> Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Setup(1)) {
                await Task.Yield();
                return _source(cancellationToken);
            }
        }
    }
}