using System;
using System.Collections.Generic;
using commands.wrappers;

namespace commands.tools
{
    public sealed class Builder
    {
        private readonly List<ICommand> _commands;

        private Builder(List<ICommand> commands){
            _commands = commands;
        }
        public Builder With<TArgument>(ICommandIn<TArgument> command, Func<TArgument> argumentGenerator){
            _commands.Add(new ArgWrapper<TArgument>(command, argumentGenerator));
            return this;
        }
        public Builder With<TResult>(ICommandOut<TResult> command, out Func<TResult> resultFetcher){
            var resultCommand = new ResultWrapper<TResult>(command);
            _commands.Add(resultCommand);
            resultFetcher = () => resultCommand.Result;
            return this;
        }
        public Builder With<TExchange>(ICommandOut<TExchange> prev, ICommandIn<TExchange> post){
            return With<TExchange>(prev, out Func<TExchange> resultFetcher).With<TExchange>(post, resultFetcher);
        }
        public ICommand Build() => null;

        public static Builder Start() => new Builder(new List<ICommand>());
    }
}