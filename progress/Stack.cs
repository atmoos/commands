using System;
using System.Threading;

namespace progress
{
    internal sealed class Stack<TElement>
        where TElement : class
    {
        private TElement _element;
        public Stack(TElement element)
        {
            _element = element;
        }
        public TElement Peek() => _element;
        public IDisposable ResetWith(TElement element) => new Token(this, element);
        public TElement Push(TElement element) => Interlocked.Exchange(ref _element, element);
        private sealed class Token : IDisposable
        {
            private readonly TElement _reset;
            private readonly Stack<TElement> _stack;
            public Token(Stack<TElement> stack, TElement reset)
            {
                _stack = stack;
                _reset = reset;
            }
            public void Dispose() => _stack.Push(_reset);
        }
    }
}