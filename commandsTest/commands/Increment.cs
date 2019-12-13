using System;
using System.Threading;
using System.Threading.Tasks;
using commands;

namespace commandsTest.commands
{
    public sealed class Increment : ICommand<Int32, Int32>
    {
        public Task<Int32> Execute(Int32 argument, CancellationToken cancellationToken, IProgress<double> progress) => Task.FromResult(argument + 1);
    }
}