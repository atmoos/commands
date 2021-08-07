using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using progressTree;

namespace commands.tools.builders
{
    internal sealed class Builder : IBuilder, IRun
    {
        private readonly IRun pre;
        private readonly List<ICommand> commands;
        public Int32 Count => commands.Count + pre.Count;
        public Builder(IRun pre, ICommand command)
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
        public async Task Run(CancellationToken cancellationToken, Progress progress)
        {
            await pre.Run(cancellationToken, progress).ConfigureAwait(false);
            foreach(var command in commands) {
                await command.Execute(cancellationToken, progress).ConfigureAwait(false);
            }
        }
    }
}
