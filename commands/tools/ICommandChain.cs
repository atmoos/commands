using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools;

internal interface ICommandChain : ICountable
{
    Task Execute(CancellationToken cancellationToken, Progress progress);
}

internal interface ICommandChain<TSource> : ICountable
{
    Task<TSource> Execute(CancellationToken cancellationToken, Progress progress);
}
