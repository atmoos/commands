using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using commands;
using commands.tools;
using commands.commands;
using commandsTest.commands;

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
            Builder<Int32> compiler = Builder<Int32>.Start(Initialize.Create(0));
            compiler.Chain(increment, (UInt64)expectedSum).Add(result => actualSum = result);
            ICommand executor = compiler.Compile();
            await executor.Execute(CancellationToken.None, null);
            Assert.Equal(expectedSum, actualSum);
        }
    }
}
