using System.Threading;
using System.Threading.Tasks;
using commands;
using progressTree;

namespace commandsTest.commands
{
    public static class Initialize
    {
        public static ICommandOut<TInitial> Create<TInitial>(TInitial initial) => new InitializeImpl<TInitial>(initial);
        private sealed class InitializeImpl<TInitial> : ICommandOut<TInitial>
        {
            public TInitial _initial;
            public InitializeImpl(TInitial initial) => _initial = initial;
            public Task<TInitial> Execute(CancellationToken _, Progress __) => Task.FromResult(_initial);
        }
    }
}
