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
    public class MessageDispatcherTests
    {
        private static MessageDispatcher MakeDispatcher() => new("Test");

        // ── observer registration & dispatch ─────────────────────────────────

        [Fact]
        public void DispatchToObserver_RegisteredObserver_CallsOnNext()
        {
            var dispatcher = MakeDispatcher();
            var received = new List<TestMessage>();
            var observer = new MessageStream<TestMessage>(4);
            observer.Subscribe(received.Add);

            dispatcher.AddObserver(1u, observer);
            var msg = new TestMessage(99);
            dispatcher.DispatchToObserver(1u, msg);

            Assert.Single(received);
            Assert.Equal(99u, received[0].Payload);
        }

        [Fact]
        public void DispatchToObserver_NoObserver_ReturnsFalse()
        {
            var dispatcher = MakeDispatcher();
            bool dispatched = dispatcher.DispatchToObserver(99u, new TestMessage());
            Assert.False(dispatched);
        }

        [Fact]
        public void DispatchToObserver_NoObserver_ReturnsTrue_AfterObserverAdded()
        {
            var dispatcher = MakeDispatcher();
            // first call queues into lobby
            bool firstDispatch = dispatcher.DispatchToObserver(1u, new TestMessage(10));
            Assert.False(firstDispatch);

            var received = new List<TestMessage>();
            var observer = new MessageStream<TestMessage>(4);
            observer.Subscribe(received.Add);

            // AddObserver should drain the lobby
            dispatcher.AddObserver(1u, observer);

            Assert.Single(received);
            Assert.Equal(10u, received[0].Payload);
        }

        // ── lobby draining ────────────────────────────────────────────────────

        [Fact]
        public void AddObserver_WithMultipleLobbyMessages_StreamableReceivesAll()
        {
            var dispatcher = MakeDispatcher();
            dispatcher.DispatchToObserver(1u, new TestMessage(1));
            dispatcher.DispatchToObserver(1u, new TestMessage(2));
            dispatcher.DispatchToObserver(1u, new TestMessage(3));

            var received = new List<TestMessage>();
            var observer = new MessageStream<TestMessage>(4);
            observer.Subscribe(received.Add);

            dispatcher.AddObserver(1u, observer);

            Assert.Equal(3, received.Count);
            Assert.Equal(new uint[] { 1, 2, 3 }, received.ConvertAll(m => m.Payload));
        }

        [Fact]
        public void AddObserver_WithLobbyMessage_NonStreamable_ReceivesOnlyFirstThenRemoves()
        {
            var dispatcher = MakeDispatcher();
            dispatcher.DispatchToObserver(1u, new TestMessage(7));
            dispatcher.DispatchToObserver(1u, new TestMessage(8));

            var received = new List<TestMessage>();
            var observer = new MessageResult<TestMessage>();
            observer.Subscribe(received.Add);

            dispatcher.AddObserver(1u, observer);

            // Non-streamable should accept first value and stop
            Assert.Single(received);
            Assert.Equal(7u, received[0].Payload);
        }

        // ── Clear ─────────────────────────────────────────────────────────────

        [Fact]
        public void Clear_WithRegisteredObservers_CallsOnError()
        {
            var dispatcher = MakeDispatcher();
            Exception? captured = null;
            var observer = new MessageStream<TestMessage>(4);
            observer.OnError(e => captured = e);

            dispatcher.AddObserver(1u, observer);
            dispatcher.Clear(connectionLost: false);

            Assert.NotNull(captured);
            Assert.IsType<CsSimConnect.Exc.SimulatorDisconnectedException>(captured);
        }

        [Fact]
        public void Clear_WithLobbyEntries_CallsOnErrorForLobby()
        {
            // Regression test for issue #10:
            // MessageDispatcher.Clear() used to cast IEnumerator to IEnumerable<object>,
            // causing InvalidCastException when the lobby was non-empty.
            var dispatcher = MakeDispatcher();

            // Put a message in the lobby (no observer registered yet)
            dispatcher.DispatchToObserver(1u, new TestMessage(5));

            // This must not throw InvalidCastException
            var ex = Record.Exception(() => dispatcher.Clear(connectionLost: true));
            Assert.Null(ex);
        }

        [Fact]
        public void Clear_ConnectionLost_DeliversSimulatorConnectionLostException()
        {
            var dispatcher = MakeDispatcher();
            Exception? captured = null;
            var observer = new MessageStream<TestMessage>(4);
            observer.OnError(e => captured = e);

            dispatcher.AddObserver(1u, observer);
            dispatcher.Clear(connectionLost: true);

            Assert.IsType<CsSimConnect.Exc.SimulatorConnectionLostException>(captured);
        }

        // ── Remove ────────────────────────────────────────────────────────────

        [Fact]
        public void Remove_AfterRemove_MessagesGoToLobby()
        {
            var dispatcher = MakeDispatcher();
            var received = new List<TestMessage>();
            var observer = new MessageStream<TestMessage>(4);
            observer.Subscribe(received.Add);

            dispatcher.AddObserver(1u, observer);
            dispatcher.Remove(1u);

            // After removal, dispatch should no longer reach the old observer
            bool found = dispatcher.DispatchToObserver(1u, new TestMessage(99));
            Assert.False(found);
            Assert.Empty(received);
        }

        // ── concurrent dispatch ───────────────────────────────────────────────

        [Fact]
        public async Task DispatchToObserver_ConcurrentFromMultipleThreads_AllDelivered()
        {
            var dispatcher = MakeDispatcher();
            int count = 0;
            var observer = new MessageStream<TestMessage>(1000);
            observer.Subscribe(_ => System.Threading.Interlocked.Increment(ref count));

            dispatcher.AddObserver(1u, observer);

            const int threads = 8;
            const int perThread = 100;

            var tasks = new Task[threads];
            for (int t = 0; t < threads; t++)
            {
                tasks[t] = Task.Run(() =>
                {
                    for (int i = 0; i < perThread; i++)
                        dispatcher.DispatchToObserver(1u, new TestMessage());
                });
            }
            await Task.WhenAll(tasks);

            Assert.Equal(threads * perThread, count);
        }
    }
}
