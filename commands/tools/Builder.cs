using System;
using System.Collections.Generic;
using commands.wrappers;

namespace commands.tools
{
    public abstract class Builder : IBuilder
    {
        private readonly List<ICommand> _commands;

        private Builder(List<ICommand> commands){
            _commands = commands;
        }
        public IBuilder Add(ICommand command){
            _commands.Add(command);
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command){
            var resultCommand = new OutCommand<TResult>(command);
            _commands.Add(resultCommand);
            return new BuilderImpl<TResult>(_commands, resultCommand);
        }
        public ICommand Build() => null;

        public static IBuilder Start(ICommand command) => new BuilderImpl(new List<ICommand>{command});
        public static IBuilder<TResult> Start<TResult>(ICommandOut<TResult> command){
            var resultCommand = new OutCommand<TResult>(command);
            return new BuilderImpl<TResult>(new List<ICommand> { resultCommand }, resultCommand);
        }
        private sealed class BuilderImpl : Builder
        {
            public BuilderImpl(List<ICommand> commands): base(commands){}
        }

        private sealed class BuilderImpl<TArgument> : Builder, IBuilder<TArgument>
        {
            private readonly IResult<TArgument> _result;
            public BuilderImpl(List<ICommand> commands, IResult<TArgument> result): base(commands){
                _result = result;
            }
            public IBuilder Add(ICommandIn<TArgument> command){
                _commands.Add(new InCommand<TArgument>(command, _result));
                return this;
            }
            public IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command)
            {
                var wrapper = new InOutCommand<TArgument, TResult>(command, _result);
                return new BuilderImpl<TResult>(_commands, wrapper);
            }
        }
    }
}