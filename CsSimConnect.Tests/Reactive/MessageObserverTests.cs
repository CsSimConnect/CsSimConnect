/*
 * Copyright (c) 2021-2024. Bert Laverman
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *    http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using CsSimConnect.Reactive;
using CsSimConnect.Tests.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CsSimConnect.Tests.Reactive
{
    public class MessageObserverTests
    {
        // ── IsCompleted initial state (issue #17: reversed visibility) ──────────

        [Fact]
        public void MessageStream_IsCompleted_InitiallyFalse()
        {
            var stream = new MessageStream<TestMessage>(4);
            Assert.False(stream.IsCompleted);
        }

        [Fact]
        public void MessageResult_IsCompleted_InitiallyFalse()
        {
            var result = new MessageResult<TestMessage>();
            Assert.False(result.IsCompleted);
        }

        // ── MessageStream (streamable observer) ───────────────────────────────

        [Fact]
        public void MessageStream_IsStreamable()
        {
            var stream = new MessageStream<TestMessage>(4);
            Assert.True(stream.IsStreamable());
        }

        [Fact]
        public void MessageStream_OnNext_InvokesSubscribeCallback()
        {
            var stream = new MessageStream<TestMessage>(4);
            var received = new List<TestMessage>();
            stream.Subscribe(received.Add);

            var msg = new TestMessage(7);
            stream.OnNext(msg);

            Assert.Single(received);
            Assert.Same(msg, received[0]);
        }

        [Fact]
        public void MessageStream_OnNext_MultipleMessages_AllDelivered()
        {
            var stream = new MessageStream<TestMessage>(4);
            var received = new List<uint>();
            stream.Subscribe(m => received.Add(m.Payload));

            stream.OnNext(new TestMessage(1));
            stream.OnNext(new TestMessage(2));
            stream.OnNext(new TestMessage(3));

            Assert.Equal(new uint[] { 1, 2, 3 }, received);
        }

        [Fact]
        public void MessageStream_OnError_InvokesErrorCallback()
        {
            var stream = new MessageStream<TestMessage>(4);
            Exception? captured = null;
            stream.OnError(e => captured = e);

            var error = new InvalidOperationException("test error");
            stream.OnError(error);

            Assert.Same(error, captured);
        }

        [Fact]
        public void MessageStream_OnCompleted_SetsIsCompleted()
        {
            var stream = new MessageStream<TestMessage>(4);
            stream.OnCompleted();
            Assert.True(stream.IsCompleted);
        }

        [Fact]
        public void MessageStream_OnError_SetsIsCompleted()
        {
            var stream = new MessageStream<TestMessage>(4);
            stream.OnError(new Exception("boom"));
            Assert.True(stream.IsCompleted);
        }

        [Fact]
        public void MessageStream_OnComplete_InvokesCompletionCallback()
        {
            var stream = new MessageStream<TestMessage>(4);
            bool called = false;
            stream.OnComplete(() => called = true);
            stream.OnCompleted();
            Assert.True(called);
        }

        [Fact]
        public void MessageStream_Subscribe_WithAllCallbacks_EachIsTriggered()
        {
            var stream = new MessageStream<TestMessage>(4);
            bool nextCalled = false, errorCalled = false, completeCalled = false;
            stream.Subscribe(_ => nextCalled = true, _ => errorCalled = true, () => completeCalled = true);

            stream.OnNext(new TestMessage());
            Assert.True(nextCalled);
            Assert.False(errorCalled);
            Assert.False(completeCalled);

            stream.OnCompleted();
            Assert.True(completeCalled);
        }

        // ── MessageResult (non-streamable observer) ───────────────────────────

        [Fact]
        public void MessageResult_IsNotStreamable()
        {
            var result = new MessageResult<TestMessage>();
            Assert.False(result.IsStreamable());
        }

        [Fact]
        public void MessageResult_OnNext_Get_ReturnsValue()
        {
            var result = new MessageResult<TestMessage>();
            var msg = new TestMessage(42);

            Task.Run(() => result.OnNext(msg));

            var received = result.Get();
            Assert.Same(msg, received);
        }

        [Fact]
        public void MessageResult_OnError_Get_Throws()
        {
            var result = new MessageResult<TestMessage>();
            var error = new InvalidOperationException("fail");

            Task.Run(() => result.OnError(error));

            var ex = Assert.ThrowsAny<Exception>(() => result.Get());
            // Get() calls Task.Result which wraps in AggregateException
            IEnumerable<Exception> innerExceptions = ex is AggregateException agg ? agg.InnerExceptions : new[] { ex };
            Assert.Contains(error, innerExceptions);
        }

        [Fact]
        public void MessageResult_OnNext_SetsIsCompleted()
        {
            var result = new MessageResult<TestMessage>();
            result.OnNext(new TestMessage());
            Assert.True(result.IsCompleted);
        }

        [Fact]
        public void MessageResult_DoubleOnNext_ThrowsDoubleResultException()
        {
            var result = new MessageResult<TestMessage>();
            result.OnNext(new TestMessage(1));

            Exception? captured = null;
            result.OnError(e => captured = e);
            result.OnNext(new TestMessage(2));  // second call must trigger error path

            Assert.IsType<DoubleResultException>(captured);
        }

        [Fact]
        public void MessageResult_OnComplete_InvokesCompletionCallback()
        {
            var result = new MessageResult<TestMessage>();
            bool called = false;
            result.OnComplete(() => called = true);
            result.OnNext(new TestMessage());   // OnNext → base.OnCompleted
            Assert.True(called);
        }

        // ── ErrorResult factory ───────────────────────────────────────────────

        [Fact]
        public void MessageResult_ErrorResult_IsCompletedWithError()
        {
            var error = new InvalidOperationException("err");
            var result = MessageResult<TestMessage>.ErrorResult(0, error);

            Assert.True(result.IsCompleted);
            Assert.NotNull(result.Error);
        }

        [Fact]
        public void MessageStream_ErrorResult_IsCompletedWithError()
        {
            var error = new InvalidOperationException("err");
            var stream = MessageStream<TestMessage>.ErrorResult(0, error);

            Assert.True(stream.IsCompleted);
            Assert.NotNull(stream.Error);
        }
    }
}
