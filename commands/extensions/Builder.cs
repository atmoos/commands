using System;
using System.Threading;
using commands.commands;
using commands.tools;

namespace commands.extensions
{
    public static class BuildExtensions
    {
        public static IBuilder StartBuilder(this ICommand command) => new Builder(new() { command });
        public static IBuilder<TResult> StartBuilder<TResult>(this ICommandOut<TResult> command) => new RootBuilder<TResult>(command);
        public static IBuilder Add(this IBuilder builder, Action action) => builder.Add(new VoidCommand(action));
        public static IBuilder Add(this IBuilder builder, Action<CancellationToken> action) => builder.Add(new VoidCommand(action));
        public static IBuilder<TResult> Add<TResult>(this IBuilder builder, Func<TResult> func) => builder.Add(new SourceCommand<TResult>(func));
        public static IBuilder<TResult> Add<TResult>(this IBuilder builder, Func<CancellationToken, TResult> func) => builder.Add(new SourceCommand<TResult>(func));
        public static IBuilder Add<TArgument>(this IBuilder<TArgument> builder, Action<TArgument> action) => builder.Add(new SinkCommand<TArgument>(action));
        public static IBuilder Add<TArgument>(this IBuilder<TArgument> builder, Action<TArgument, CancellationToken> action) => builder.Add(new SinkCommand<TArgument>(action));
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
}