using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class Builder : IBuilder, ICommandChain
    {
        private readonly ICommandChain pre;
        private readonly List<ICommand> commands;
        Int32 ICountable.Count => commands.Count + pre.Count;
        public Builder(ICommandChain pre, ICommand command)
        {
            this.pre = pre;
            this.commands = new List<ICommand> { command };
        }

        public IBuilder Add(ICommand command)
        {
            commands.Add(command);
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command) => new SourceBuilder<TResult>(this, command);
        public ICommand Build() => new CompiledCommand(this);
        async Task ICommandChain.Execute(CancellationToken cancellationToken, Progress progress)
        {
            await pre.Execute(cancellationToken, progress).ConfigureAwait(false);
            foreach(var command in commands) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
}
