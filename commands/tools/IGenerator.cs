
namespace commands.tools
{
    public interface IGenerator
    {
        ICommand Compile();
        ICommandOut<TResult> Compile<TResult>(ICommandOut<TResult> command);
    }
    public interface IGenerator<out TArgument>
    {
        ICommand Compile(ICommandIn<TArgument> command);
        ICommandOut<TResult> Compile<TResult>(ICommand<TArgument, TResult> command);
    }
}
