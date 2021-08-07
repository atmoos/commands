using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class RootBuilder : IBuilder, ICommandChain
    {
        private readonly List<ICommand> commands;
        Int32 ICountable.Count => commands.Count;
        public RootBuilder(ICommand command) => commands = new List<ICommand> { command };
        public IBuilder Add(ICommand command)
        {
            commands.Add(command);
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command) => new SourceBuilder<TResult>(this, command);
        public ICommand Build() => new CompiledCommand(this);
        async Task ICommandChain.Execute(CancellationToken cancellationToken, Progress progress)
        {
            foreach(var command in commands) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
    internal sealed class RootBuilder<TResult> : ICommandChain<TResult>, IBuilder<TResult>
    {
        private readonly ICommandOut<TResult> argument;
        Int32 ICountable.Count => 1;
        public RootBuilder(ICommandOut<TResult> argument) => this.argument = argument;
        public IBuilder Add(ICommandIn<TResult> command) => new SinkBuilder<TResult>(this, command);
        public IBuilder<TOtherResult> Add<TOtherResult>(ICommand<TResult, TOtherResult> command) => new MapBuilder<TResult, TOtherResult>(this, command);
        public ICommandOut<TResult> Build() => this.argument;
        async Task<TResult> ICommandChain<TResult>.Execute(CancellationToken cancellationToken, Progress progress)
        {
            return await this.argument.Execute(cancellationToken, progress).ConfigureAwait(false);
        }
    }
}
