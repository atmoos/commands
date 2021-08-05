namespace commands.tools
{
    public interface IBuilder : IGenerator
    {
        IBuilder Add(ICommand command);
        IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command);
    }

    public interface IBuilder<TArgument> : IBuilder
    {
        IBuilder Add(ICommandIn<TArgument> command);
        IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command);
    }
}