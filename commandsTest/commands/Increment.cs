using System;
using System.Threading;
using System.Threading.Tasks;
using commands;
using progressTree;

namespace commandsTest.commands
{
    public sealed class Increment : ICommand<Int32, Int32>
    {
        public Task<Int32> Execute(Int32 argument, CancellationToken _, Progress progress)
        {
            using(progress.Schedule(1)) {
                return Task.FromResult(argument + 1);
            }
        }
    }
}