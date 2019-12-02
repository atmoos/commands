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
        public IBuilder Add<TArgument>(ICommandIn<TArgument> command, Func<TArgument> argGetter){
            _commands.Add(new ArgWrapper<TArgument>(command, argGetter));
            return this;
        }
        public IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command){
            var resultCommand = new ResultWrapper<TResult>(command);
            _commands.Add(resultCommand);
            return new BuilderImpl<TResult>(_commands, resultCommand);
        }
        public ICommand Build() => null;

        public static IBuilder Start() => new BuilderImpl(new List<ICommand>());
        public static IBuilder<TResult> Start<TResult>(ICommandOut<TResult> command){
            var resultCommand = new ResultWrapper<TResult>(command);
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
                _commands.Add(new ResultConsumer<TArgument>(command, _result));
                return this;
            }
        }
    }
}