namespace commands.tools
{
    public interface IBuilder
    {
        IBuilder Add(ICommand command);
        IBuilder<TResult> Add<TResult>(ICommandOut<TResult> command);
        ICommand Build();
    }

    public interface IBuilder<TArgument>
    {
        IBuilder Add(ICommandIn<TArgument> command);
        IBuilder<TResult> Add<TResult>(ICommand<TArgument, TResult> command);
        ICommandOut<TArgument> Build();
    }
}