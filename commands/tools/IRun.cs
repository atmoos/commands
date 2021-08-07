using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools
{
    internal interface IRun : ICountable
    {
        Task Run(CancellationToken cancellationToken, Progress progress);
    }

    internal interface IRun<TSource> : ICountable
    {
        Task<TSource> Run(CancellationToken cancellationToken, Progress progress);
    }
}
