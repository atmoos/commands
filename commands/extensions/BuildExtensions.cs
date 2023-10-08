using System;
using System.Threading;
using commands.tools;
using commands.tools.builders;

namespace commands.extensions;

public static class BuildExtensions
{
    public static IBuilder StartBuilder(this Action action) => new RootBuilder(action.ToCommand());
    public static IBuilder StartBuilder(this ICommand command) => new RootBuilder(command);
    public static IBuilder<TResult> StartBuilder<TResult>(this Func<TResult> func) => new RootBuilder<TResult>(func.ToCommand());
    public static IBuilder<TResult> StartBuilder<TResult>(this ICommandOut<TResult> command) => new RootBuilder<TResult>(command);
    public static IBuilder Add(this IBuilder builder, Action action) => builder.Add(action.ToCommand());
    public static IBuilder Add(this IBuilder builder, Action<CancellationToken> action) => builder.Add(action.ToCommand());
    public static IBuilder<TResult> Add<TResult>(this IBuilder builder, Func<TResult> func) => builder.Add(func.ToCommand());
    public static IBuilder<TResult> Add<TResult>(this IBuilder builder, Func<CancellationToken, TResult> func) => builder.Add(func.ToCommand());
    public static IBuilder Add<TArgument>(this IBuilder<TArgument> builder, Action<TArgument> action) => builder.Add(action.ToCommand());
    public static IBuilder Add<TArgument>(this IBuilder<TArgument> builder, Action<TArgument, CancellationToken> action) => builder.Add(action.ToCommand());
    public static IBuilder<TResult> Add<TArgument, TResult>(this IBuilder<TArgument> builder, Func<TArgument, TResult> func) => builder.Add(func.ToCommand());
    public static IBuilder<TResult> Add<TArgument, TResult>(this IBuilder<TArgument> builder, Func<TArgument, CancellationToken, TResult> func) => builder.Add(func.ToCommand());
    public static IBuilder Chain(this IBuilder builder, ICommand command, UInt64 count) => builder.Repeat((b, _) => b.Add(command), count);
    public static IBuilder Chain(this IBuilder builder, Func<UInt64, ICommand> generator, UInt64 count) => builder.Repeat((b, i) => b.Add(generator(i)), count);
    public static IBuilder<TChain> Chain<TChain>(this IBuilder<TChain> builder, ICommand<TChain, TChain> command, UInt64 count) => builder.Repeat((b, _) => b.Add(command), count);
    public static IBuilder<TChain> Chain<TChain>(this IBuilder<TChain> builder, Func<UInt64, ICommand<TChain, TChain>> generator, UInt64 count) => builder.Repeat((b, i) => b.Add(generator(i)), count);
    private static TBuilder Repeat<TBuilder>(this TBuilder builder, Func<TBuilder, UInt64, TBuilder> repetition, UInt64 count)
    {
        for(UInt64 iteration = 0; iteration < count; ++iteration) {
            builder = repetition(builder, iteration);
        }
        return builder;
    }
}