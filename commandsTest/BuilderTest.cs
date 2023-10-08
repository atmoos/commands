using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using commands.extensions;
using commands.tools;
using progressReporting;
using progressTree;
using Xunit;

namespace commandsTest;

public class BuilderTest
{
    [Fact]
    public async Task ChainCommandsOnBuilder()
    {
        const Int32 expectedSum = 8;
        Func<Int32, Int32> increment = i => i + 1;
        var command = Builder(() => 0).Chain(increment.ToCommand(), expectedSum).Build();
        var actualSum = await command.Execute(CancellationToken.None, Progress.Empty).ConfigureAwait(false);
        Assert.Equal(expectedSum, actualSum);
    }

    [Fact]
    public async Task AddCommandAccrossVariousTypes()
    {
        const Double expectedValue = Math.PI;
        var command = Builder(() => expectedValue).Add(d => d.ToString("R")).Add(Decimal.Parse).Add(d => (Double)d).Build();
        var roundRobin = await command.Execute(CancellationToken.None, Progress.Empty).ConfigureAwait(false);
        Assert.Equal(expectedValue, roundRobin);
    }

    [Fact]
    public async Task BuiltCommandCanBeCancelled()
    {
        using(var cancellation = new CancellationTokenSource()) {
            void cancelCommand(Int32 _) => cancellation.Cancel();
            var fourChainedCommands = Builder(() => 0).Add(i => i + 3).Add(cancelCommand).Add(() => 66).Build();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await fourChainedCommands.Execute(cancellation.Token, Progress.Empty).ConfigureAwait(false)).ConfigureAwait(false);
        }
    }

    [Fact]
    public async Task BuiltCommandReportsProgress()
    {
        const Int32 chainedCommandCount = 16;
        var actualProgress = new ProgressRecorder<Double>();
        Func<Int32, Int32> increment = i => i + 1;
        var chainedCommands = Builder(() => 1).Chain(increment.ToCommand(), chainedCommandCount - 1).Build();

        await chainedCommands.Execute(CancellationToken.None, Progress.Create(actualProgress)).ConfigureAwait(false);

        var expectedProgress = Enumerable.Range(0, chainedCommandCount + 1).Select(i => (Double)i / chainedCommandCount);
        Assert.Equal(expectedProgress, actualProgress);
    }

    private static IBuilder<TOut> Builder<TOut>(Func<TOut> init) => init.StartBuilder();
}
