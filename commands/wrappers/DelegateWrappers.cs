
using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands.wrappers
{
    internal sealed class ActionCommand: ICommand
    {
        private readonly Action<CancellationToken> _action;

        public ActionCommand(Action action) : this(_ => action()) { }
        public ActionCommand(Action<CancellationToken> action)
        {
            _action = action;
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            await Task.Yield();
            _action(cancellationToken);
        }
    }
    internal sealed class ActionCommand<TArgument, TResult>: ICommand<TArgument, TResult>
    {
        private readonly Func<TArgument, CancellationToken, TResult> _func;

        public ActionCommand(Func<TArgument, TResult> func) : this((a, _) => func(a)) { }
        public ActionCommand(Func<TArgument, CancellationToken, TResult> func)
        {
            _func = func;
        }

        public async Task<TResult> Execute(TArgument argument, CancellationToken cancellationToken, IProgress<double> progress)
        {
            await Task.Yield();
            return _func(argument, cancellationToken);
        }
    }
}