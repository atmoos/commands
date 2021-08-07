
using System;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.commands
{
    internal sealed class SourceCommand<TResult> : ICommandOut<TResult>
    {
        private readonly Func<CancellationToken, TResult> source;
        public SourceCommand(Func<TResult> source) : this(_ => source()) { }
        public SourceCommand(Func<CancellationToken, TResult> source) => this.source = source;
        public async Task<TResult> Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(1)) {
                await Task.Yield();
                return this.source(cancellationToken);
            }
        }
    }
}