using System;
using System.Threading;
using System.Threading.Tasks;
using commands;
using commands.commands;
using commands.tools;
using commandsTest.commands;
using progressTree;
using Xunit;
using static commands.extensions.Builder;

namespace commandsTest
{
    public class BuilderTest
    {
        [Fact]
        public async Task ChainCommandsOnBuilder()
        {
            Int32 actualSum = -1;
            Int32 expectedSum = 8;
            ICommand<Int32, Int32> increment = new Increment();
            var compiler = Builder.Start(Initialize.Create(0)).Chain(increment, (UInt64)expectedSum).Add(result => actualSum = result);
            ICommand executor = compiler.Compile();
            await executor.Execute(CancellationToken.None, Progress.Empty).ConfigureAwait(false);
            Assert.Equal(expectedSum, actualSum);
        }
    }
}
