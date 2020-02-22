using System;
using Xunit;
using progressTree;

namespace progressTreeTest
{
    public sealed class StackTest
    {
        private static readonly String origin = "origin";
        private readonly Stack<String> _stack = new Stack<String>(origin);

        [Fact]
        public void PeekReturnsCurrentElement()
        {
            Assert.Same(origin, _stack.Peek());
        }
        [Fact]
        public void PushReturnsPreviousElement()
        {
            Assert.Same(origin, _stack.Push("new"));
        }
        [Fact]
        public void PeekReturnsNewElementAfterPush()
        {
            const String newItem = "new";
            _stack.Push(newItem);
            Assert.Same(newItem, _stack.Peek());
        }
    }
}