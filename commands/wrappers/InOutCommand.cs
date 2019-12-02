﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace commands.wrappers
{
    internal sealed class InOutCommand<TArgument, TResult> : ICommand, IResult<TResult>
    {
        private readonly IResult<TArgument> _resultGetter;
        private readonly ICommand<TArgument, TResult> _command;
        public TResult Result {get; private set;}
        internal InOutCommand(ICommand<TArgument, TResult> command, IResult<TArgument> resultGetter){
            _command = command;
            _resultGetter = resultGetter;
        }  
        public async Task Execute(CancellationToken cancellationToken, IProgress<Double> progress){
            Result = await _command.Execute(_resultGetter.Result, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}