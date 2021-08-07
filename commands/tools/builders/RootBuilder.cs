using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class RootBuilder : IBuilder, IRun
    {
        private readonly List<ICommand> commands;
        public Int32 Count => commands.Count;
        public RootBuilder(ICommand command) => commands = new List<ICommand> { command };
        public IBuilder Add(ICommand command)
        {
            commands.Add(command);
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command) => new SourceBuilder<TResult>(this, command);
        public ICommand Build() => new CompiledCommand(this);
        public async Task Run(CancellationToken cancellationToken, Progress progress)
        {
            foreach(var command in commands) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class RootBuilder<TResult> : IRun<TResult>, IBuilder<TResult>
    {
        private readonly ICommandOut<TResult> argument;
        public Int32 Count => 1;
        public RootBuilder(ICommandOut<TResult> argument) => this.argument = argument;
        public IBuilder Add(ICommandIn<TResult> command) => new SinkBuilder<TResult>(this, command);
        public IBuilder<TOtherResult> Add<TOtherResult>(ICommand<TResult, TOtherResult> command) => new MapBuilder<TResult, TOtherResult>(this, command);
        public ICommandOut<TResult> Build() => this.argument;
        async Task<TResult> IRun<TResult>.Run(CancellationToken cancellationToken, Progress progress)
        {
            return await this.argument.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
