using System;
using System.Threading;
using System.Threading.Tasks;
using commands;
using progress;

namespace commandsTest.commands
{
    public abstract class Initialize
    {
        private Initialize() { }
        public static ICommandOut<TInitial> Create<TInitial>(TInitial initial) => new InitializeImpl<TInitial>(initial);

        private sealed class InitializeImpl<TInitial> : Initialize, ICommandOut<TInitial>
        {
            public TInitial _initial;

            public InitializeImpl(TInitial initial)
             : base()
            {
                _initial = initial;
            }

            public Task<TInitial> Execute(CancellationToken cancellationToken, Progress progress) => Task<TInitial>.FromResult(_initial);
        }
    }
}