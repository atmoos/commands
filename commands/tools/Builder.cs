using System;
using System.Collections.Generic;
using commands.wrappers;

namespace commands.tools
{
    public abstract class Builder : IBuilder, IGenerator
    {
        protected readonly List<ICommand> _commands;

        internal Builder(List<ICommand> commands){
            _commands = commands;
        }
        public IBuilder Add(ICommand command){
            _commands.Add(command);
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command){
            var resultCommand = new OutCommand<TResult>(command);
            _commands.Add(resultCommand);
            return new Builder<TResult>(_commands, resultCommand);
        }
        public ICommand Compile() => new SequentialCommand(_commands);
        public static Builder Start(ICommand command) => new BuilderImpl(new List<ICommand>{command});
        public static Builder<TResult> Start<TResult>(ICommandOut<TResult> command){
            var resultCommand = new OutCommand<TResult>(command);
            return new Builder<TResult>(new List<ICommand>{resultCommand}, resultCommand);
        }
        private sealed class BuilderImpl : Builder
        {
            public BuilderImpl(List<ICommand> commands): base(commands){}
        }
    }
    public sealed class Builder<TArgument> : Builder, IBuilder<TArgument>
    {
        private readonly IResult<TArgument> _result;
        internal Builder(List<ICommand> commands, IResult<TArgument> result): base(commands){
            _result = result;
        }
        public IBuilder Add(ICommandIn<TArgument> command){
            _commands.Add(new InCommand<TArgument>(command, _result));
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command)
        {
            return new Builder<TResult>(_commands, new InOutCommand<TArgument, TResult>(command, _result));
        }
    }
}
