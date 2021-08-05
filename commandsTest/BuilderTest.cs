using System;
using System.Threading;
using System.Threading.Tasks;
using commands;
using commands.commands;
using commands.tools;
using commandsTest.commands;
using progressTree;
using Xunit;
using static commands.extensions.BuildExtensions;

namespace commandsTest
{
    public class BuilderTest
    {
        [Fact]
        public async Task ChainCommandsOnBuilder()
        {
            Int32 expectedSum = 8;
            ICommand<Int32, Int32> increment = new Increment();
            var compiler = Initialize.Create(0).StartBuilder().Chain(increment, (UInt64)expectedSum);
            var executor = compiler.Build();
            var actualSum = await executor.Execute(CancellationToken.None, Progress.Empty).ConfigureAwait(false);
            Assert.Equal(expectedSum, actualSum);
        }
    }
}
