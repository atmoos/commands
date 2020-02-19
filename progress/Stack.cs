using System.Threading;

namespace progress
{
    internal sealed class Stack<TElement>
        where TElement : class
    {
        private TElement _element;
        public Stack(TElement element) => _element = element;
        public TElement Peek() => _element;
        public TElement Push(TElement element) => Interlocked.Exchange(ref _element, element);
    }
}