using System;
using System.Threading;
using commands.commands;

namespace commands.extensions
{
    public static class FuncExtensions
    {
        public static ICommand ToCommand(this Action action) => new VoidCommand(action);
        public static ICommand ToCommand(this Action<CancellationToken> action) => new VoidCommand(action);
        public static ICommandOut<TResult> ToCommand<TResult>(this Func<TResult> func) => new SourceCommand<TResult>(func);
        public static ICommandOut<TResult> ToCommand<TResult>(this Func<CancellationToken, TResult> func) => new SourceCommand<TResult>(func);
        public static ICommandIn<TArgument> ToCommand<TArgument>(this Action<TArgument> action) => new SinkCommand<TArgument>(action);
        public static ICommandIn<TArgument> ToCommand<TArgument>(this Action<TArgument, CancellationToken> action) => new SinkCommand<TArgument>(action);
        public static ICommand<TArgument, TResult> ToCommand<TArgument, TResult>(this Func<TArgument, TResult> func) => new MapCommand<TArgument, TResult>(func);
        public static ICommand<TArgument, TResult> ToCommand<TArgument, TResult>(this Func<TArgument, CancellationToken, TResult> func) => new MapCommand<TArgument, TResult>(func);
    }
}