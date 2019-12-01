using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands
{
    public interface ICommand
    {
        Task Execute(CancellationToken cancellationToken, IProgress<Double> progress);
    }
}
