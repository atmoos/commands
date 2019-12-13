using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using commands;
using commands.tools;
using commands.commands;
using commandsTest.commands;


namespace commandsTest
{
    public class BuilderTest
    {
        public BuilderTest(){
            Console.WriteLine("Ctor.");
        }

        [Fact]
        public async Task Test1()
        {
            Int32 actualSum = -1;
            Int32 expectedSum = 8;
            ICommand<Int32, Int32> increment = new Increment();
            Builder<Int32> compiler = Builder<Int32>.Start(Initialize.Create(0));
            IBuilder<Int32> builder = compiler;
            for(Int32 i=0; i < expectedSum; ++i){
                builder = builder.Add<Int32>(increment);
            }
            builder.Add(new Sink<Int32>(result => actualSum = result));
            ICommand executor = compiler.Compile();
            await executor.Execute(CancellationToken.None, null);
            Assert.Equal(expectedSum, actualSum);
        }
    }
}
