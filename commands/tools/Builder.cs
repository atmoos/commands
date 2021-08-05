using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools
{
    internal interface ILinkedBuilder
    {
        public Int32 Count { get; }
    }
    internal sealed class RootBuilder<TResult> : IBuilder<TResult>
    {
        private readonly ICommandOut<TResult> argument;
        public RootBuilder(ICommandOut<TResult> argument)
        {
            this.argument = argument;
        }

        public IBuilder Add(ICommandIn<TResult> command)
        {
            return new SinkBuilder<TResult>(this.argument, command);
        }

        public IBuilder<TOtherResult> Add<TOtherResult>(ICommand<TResult, TOtherResult> command)
        {
            return new ThroughBuilder<TResult, TOtherResult>(this.argument, command);
        }

        public ICommandOut<TOtherResult> Build<TOtherResult>(ICommand<TResult, TOtherResult> command)
        {
            return new ThroughBuilder<TResult, TOtherResult>(this.argument, command);
        }

        public ICommandOut<TResult> Build()
        {
            return this.argument;
        }
    }

    internal sealed class Builder : IBuilder, ICommand
    {
        private readonly List<ICommand> commands;

        public Builder(List<ICommand> commands)
        {
            this.commands = commands;
        }

        public IBuilder Add(ICommand command)
        {
            this.commands.Add(command);
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command)
        {
            return new SourceBuilder<TResult>(this, command);
        }

        public ICommand Build()
        {
            return this;
        }

        async Task ICommand.Execute(CancellationToken cancellationToken, Progress progress)
        {
            foreach(var command in this.commands) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class SourceBuilder<TArgument> : IBuilder<TArgument>, ICommandOut<TArgument>
    {
        private readonly ICommand command;
        private readonly ICommandOut<TArgument> argument;

        internal SourceBuilder(ICommand command, ICommandOut<TArgument> argument)
        {
            this.command = command;
            this.argument = argument;
        }
        public IBuilder Add(ICommandIn<TArgument> command)
        {
            return new SinkBuilder<TArgument>(this, command);
        }
        public IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command)
        {
            return new ThroughBuilder<TArgument, TResult>(this, command);
        }

        public ICommandOut<TArgument> Build()
        {
            return this;
        }

        async Task<TArgument> ICommandOut<TArgument>.Execute(CancellationToken cancellationToken, Progress progress)
        {
            await this.command.Execute(cancellationToken, progress).ConfigureAwait(false);
            return await this.argument.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
    internal sealed class SinkBuilder<TResult> : IBuilder, ICommand
    {
        private readonly ICommandOut<TResult> source;
        private readonly ICommandIn<TResult> sink;

        internal SinkBuilder(ICommandOut<TResult> source, ICommandIn<TResult> sink)
        {
            this.source = source;
            this.sink = sink;
        }

        public IBuilder Add(ICommand command)
        {
            return new Builder(new List<ICommand> { this, command });
        }

        public IBuilder<TOtherResult> Add<TOtherResult>(ICommandOut<TOtherResult> command)
        {
            return new SourceBuilder<TOtherResult>(this, command);
        }

        public ICommand Build()
        {
            return this;
        }

        async Task ICommand.Execute(CancellationToken cancellationToken, Progress progress)
        {
            TResult argument = await this.source.Execute(cancellationToken, progress).ConfigureAwait(false);
            await this.sink.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }

    public sealed class ThroughBuilder<TArgument, TResult> : IBuilder<TResult>, ICommandOut<TResult>
    {
        private readonly ICommandOut<TArgument> source;
        private readonly ICommand<TArgument, TResult> map;

        public ThroughBuilder(ICommandOut<TArgument> source, ICommand<TArgument, TResult> map)
        {
            this.source = source;
            this.map = map;
        }

        public IBuilder Add(ICommandIn<TResult> command)
        {
            return new SinkBuilder<TResult>(this, command);
        }

        public IBuilder<TOtherResult> Add<TOtherResult>(ICommand<TResult, TOtherResult> command)
        {
            return new ThroughBuilder<TResult, TOtherResult>(this, command);
        }

        public ICommandOut<TResult> Build()
        {
            return this;
        }

        async Task<TResult> ICommandOut<TResult>.Execute(CancellationToken cancellationToken, Progress progress)
        {
            var argument = await this.source.Execute(cancellationToken, progress).ConfigureAwait(false);
            return await this.map.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
