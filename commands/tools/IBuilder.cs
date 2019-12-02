using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands.tools
{
    public interface IBuilder
    {
        IBuilder Add(ICommand command);
        IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command);
        IBuilder Add<TArgument>(ICommandIn<TArgument> command, Func<TArgument> argGetter);
        ICommand Build();
    }

    public interface IBuilder<TArgument> : IBuilder
    {
        IBuilder Add(ICommandIn<TArgument> command);
    }
}