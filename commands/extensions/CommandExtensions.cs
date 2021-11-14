using System.Threading;
using System.Threading.Tasks;

namespace commands.extensions
{
    public static class CommandExtensions
    {
        // ToDo: Add all variants!
        public static Task Execute(this ICommand command) => command.Execute(progressTree.Progress.Empty);
        public static Task Execute(this ICommand command, progressTree.Progress progress) => command.Execute(CancellationToken.None, progress);
    }
}