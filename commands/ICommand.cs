using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands
{
    public interface ICommand
    {
        Task Execute(CancellationToken cancellationToken, IProgress<Double> progress);
    }
    public interface ICommandIn<in TArgument>
    {
        Task Execute(TArgument argument, CancellationToken cancellationToken, IProgress<Double> progress);
    }   
    public interface ICommandOut<TResult>
    {
        Task<TResult> Execute(CancellationToken cancellationToken, IProgress<Double> progress);
    }
    public interface ICommand<in TArgument, TResult>
    {
        Task<TResult> Execute(TArgument argument, CancellationToken cancellationToken, IProgress<Double> progress);
    }
}
