using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools
{
    internal interface ICountable
    {
        public Int32 Count { get; }
    }
    interface IRun : ICountable
    {
        Task Run(CancellationToken cancellationToken, Progress progress);
    }

    interface IRun<TSource> : ICountable
    {
        Task<TSource> Run(CancellationToken cancellationToken, Progress progress);
    }

    internal sealed class CompiledCommand : ICommand
    {
        private readonly IRun runner;
        public CompiledCommand(IRun runner) => this.runner = runner;
        public async Task Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(this.runner.Count)) {
                await this.runner.Run(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class CompiledCommand<TResult> : ICommandOut<TResult>
    {
        private readonly IRun<TResult> runner;
        public CompiledCommand(IRun<TResult> runner) => this.runner = runner;
        public async Task<TResult> Execute(CancellationToken cancellationToken, Progress progress)
        {
            using(progress.Schedule(this.runner.Count)) {
                return await this.runner.Run(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }

    internal sealed class RootBuilder : IBuilder, IRun
    {
        private readonly List<ICommand> commands;

        public Int32 Count => this.commands.Count;

        public RootBuilder(ICommand command)
        {
            this.commands = new List<ICommand> { command };
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
            return new CompiledCommand(this);
        }

        public async Task Run(CancellationToken cancellationToken, Progress progress)
        {
            foreach(var command in this.commands) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class RootBuilder<TResult> : IRun<TResult>, IBuilder<TResult>
    {
        private readonly ICommandOut<TResult> argument;
        public Int32 Count => 1;
        public RootBuilder(ICommandOut<TResult> argument)
        {
            this.argument = argument;
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
            return this.argument;
        }

        async Task<TResult> IRun<TResult>.Run(CancellationToken cancellationToken, Progress progress)
        {
            return await this.argument.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
    internal sealed class Builder : IBuilder, IRun
    {
        private readonly IRun pre;
        private readonly List<ICommand> commands;
        public Int32 Count => this.commands.Count + this.pre.Count;

        public Builder(IRun pre, ICommand command)
        {
            this.pre = pre;
            this.commands = new List<ICommand> { command };
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
            return new CompiledCommand(this);
        }

        public async Task Run(CancellationToken cancellationToken, Progress progress)
        {
            await this.pre.Run(cancellationToken, progress).ConfigureAwait(false);
            foreach(var command in this.commands) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class SourceBuilder<TArgument> : IBuilder<TArgument>, IRun<TArgument>
    {
        private readonly IRun command;
        private readonly ICommandOut<TArgument> argument;
        public Int32 Count => 1 + this.command.Count;

        internal SourceBuilder(IRun command, ICommandOut<TArgument> argument)
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
            return new CompiledCommand<TArgument>(this);
        }

        public async Task<TArgument> Run(CancellationToken cancellationToken, Progress progress)
        {
            await this.command.Run(cancellationToken, progress).ConfigureAwait(false);
            return await this.argument.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
    internal sealed class SinkBuilder<TResult> : IBuilder, IRun
    {
        private readonly IRun<TResult> source;
        private readonly ICommandIn<TResult> sink;

        public Int32 Count => 1 + this.source.Count;

        internal SinkBuilder(IRun<TResult> source, ICommandIn<TResult> sink)
        {
            this.source = source;
            this.sink = sink;
        }

        public IBuilder Add(ICommand command)
        {
            return new Builder(this, command);
        }

        public IBuilder<TOtherResult> Add<TOtherResult>(ICommandOut<TOtherResult> command)
        {
            return new SourceBuilder<TOtherResult>(this, command);
        }

        public ICommand Build()
        {
            return new CompiledCommand(this);
        }

        public async Task Run(CancellationToken cancellationToken, Progress progress)
        {
            TResult argument = await this.source.Run(cancellationToken, progress).ConfigureAwait(false);
            await this.sink.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }

    public sealed class ThroughBuilder<TArgument, TResult> : IBuilder<TResult>, IRun<TResult>
    {
        private readonly IRun<TArgument> source;
        private readonly ICommand<TArgument, TResult> map;

        public Int32 Count => 1 + this.source.Count;

        internal ThroughBuilder(IRun<TArgument> source, ICommand<TArgument, TResult> map)
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
            return new CompiledCommand<TResult>(this);
        }

        public async Task<TResult> Run(CancellationToken cancellationToken, Progress progress)
        {
            var argument = await this.source.Run(cancellationToken, progress).ConfigureAwait(false);
            return await this.map.Execute(argument, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
