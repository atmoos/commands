using System.Collections.Generic;
using commands.commands;

namespace commands.tools
{
    internal abstract class Builder : IBuilder
    {
        // Make this thing recursively call all registered commands :-)
        // --> The list is not required!
        // --> Also the compile function ought to return the recursive executor that computes the final result!
        protected readonly List<ICommand> _commands;
        internal Builder(List<ICommand> commands) => _commands = commands;
        public IBuilder Add(ICommand command)
        {
            _commands.Add(command);
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command)
        {
            return new Builder<TResult>(_commands, command);
        }
        public ICommand Compile() => new SequentialCommand(_commands);
        public static IBuilder Start(ICommand command) => new BuilderImpl(new List<ICommand> { command });
        public static IBuilder<TResult> Start<TResult>(ICommandOut<TResult> command) => new Builder<TResult>(new List<ICommand>(), command);
        private sealed class BuilderImpl : Builder
        {
            public BuilderImpl(List<ICommand> commands) : base(commands) { }
        }
    }
    internal sealed class Builder<TArgument> : Builder, IBuilder<TArgument>
    {
        private readonly ICommandOut<TArgument> argument;
        internal Builder(List<ICommand> commands, ICommandOut<TArgument> argument) : base(commands) => this.argument = argument;
        public IBuilder Add(ICommandIn<TArgument> command)
        {
            _commands.Add(new Command<TArgument>(this.argument, command));
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command)
        {
            return new Builder<TResult>(_commands, new InOutCommand<TArgument, TResult>(this.argument, command));
        }
    }
}
