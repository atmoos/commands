using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands
{
    public interface ICommand
    {
        Task Execute(CancellationToken cancellationToken, Progress progress);
    }
    public interface ICommandIn<in TArgument>
    {
        Task Execute(TArgument argument, CancellationToken cancellationToken, Progress progress);
    }
    public interface ICommandOut<TResult>
    {
        Task<TResult> Execute(CancellationToken cancellationToken, Progress progress);
    }
    public interface ICommand<in TArgument, TResult>
    {
        Task<TResult> Execute(TArgument argument, CancellationToken cancellationToken, Progress progress);
    }
}
