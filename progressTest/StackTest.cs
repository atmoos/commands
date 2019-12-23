using System;
using Xunit;
using progress;

namespace progressTest
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
        [Fact]
        public void RegisteringResetDoesNotAlterStackState()
        {
            String expectedState = _stack.Peek();
            using(_stack.RegisterReset("other")) {
                Assert.Same(expectedState, _stack.Peek());
            }
        }
        [Fact]
        public void RegisteredResetResetsStackAfterDispose()
        {
            const String expectedReset = "reset";
            using(_stack.RegisterReset(expectedReset)) {
                Assert.NotSame(expectedReset, _stack.Peek());
            }
            Assert.Same(expectedReset, _stack.Peek());
        }
    }
}