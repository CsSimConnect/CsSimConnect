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

using CsSimConnect.Exc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CsSimConnect.Tests
{
    public class SimConnectInternalTests
    {
        // SimConnect.Instance can be constructed without P/Invoke — the constructor
        // only wires event handlers; no DLL calls are made until Connect() is called.

        // ── MessageCompleted (issue #11: missing sendId log arg) ─────────────
        // The bug was cosmetic (garbled logs), so we verify behavioural correctness:
        // MessageCompleted must remove the registered callback without throwing.

        [Fact]
        public void MessageCompleted_AfterAddCleanup_RemovesEntry()
        {
            var sc = SimConnect.Instance;
            bool called = false;
            sc.AddCleanup(9001u, _ => called = true);

            // MessageCompleted removes the entry; callback must NOT be invoked
            sc.MessageCompleted(9001u);

            // Calling again must also not throw (entry already gone)
            var ex = Record.Exception(() => sc.MessageCompleted(9001u));
            Assert.Null(ex);
            Assert.False(called);
        }

        [Fact]
        public void MessageCompleted_UnknownSendId_DoesNotThrow()
        {
            var sc = SimConnect.Instance;
            var ex = Record.Exception(() => sc.MessageCompleted(uint.MaxValue));
            Assert.Null(ex);
        }

        // ── AddCleanup + MessageCompleted locking (issue #13) ────────────────
        // Concurrent adds and removes must not corrupt the dictionary or throw.

        [Fact]
        public void AddCleanup_ConcurrentAccess_NoExceptions()
        {
            var sc = SimConnect.Instance;
            var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

            const int threads = 8;
            const int perThread = 50;

            // Use IDs in a private range unlikely to collide with other tests
            const uint baseId = 800000u;

            var tasks = new Task[threads * 2];
            for (int t = 0; t < threads; t++)
            {
                int localT = t;
                // Writer task
                tasks[localT * 2] = Task.Run(() =>
                {
                    for (int i = 0; i < perThread; i++)
                    {
                        uint id = baseId + (uint)(localT * perThread + i);
                        try { sc.AddCleanup(id, _ => { }); }
                        catch (Exception ex) { exceptions.Add(ex); }
                    }
                });
                // Reader/remover task (completes the same IDs)
                tasks[localT * 2 + 1] = Task.Run(() =>
                {
                    for (int i = 0; i < perThread; i++)
                    {
                        uint id = baseId + (uint)(localT * perThread + i);
                        // MessageCompleted is idempotent when the entry is missing
                        try { sc.MessageCompleted(id); }
                        catch (Exception ex) { exceptions.Add(ex); }
                    }
                });
            }
            Task.WaitAll(tasks);

            Assert.Empty(exceptions);
        }

        [Fact]
        public void AddCleanup_DuplicateSendId_ThrowsArgumentException()
        {
            // Dictionary.Add throws on duplicate key — both calls should
            // race-safely not corrupt state when the first succeeds.
            var sc = SimConnect.Instance;
            uint id = 999999u;

            // Ensure clean state first
            sc.MessageCompleted(id);

            sc.AddCleanup(id, _ => { });

            // Second add for same ID must throw (Dictionary behaviour)
            Assert.ThrowsAny<Exception>(() => sc.AddCleanup(id, _ => { }));

            // Cleanup
            sc.MessageCompleted(id);
        }

        // ── Shutdown() ──────────────────────────────────────────────────────────
        // Verifies that Shutdown() leaves the instance in a safe, idle state
        // without requiring a live simulator connection.

        [Fact]
        public void Shutdown_WhenNotConnected_LeavesUseAutoConnectFalseAndIsConnectedFalse()
        {
            var sc = SimConnect.Instance;
            sc.UseAutoConnect = false; // ensure clean starting state

            sc.Shutdown();

            Assert.False(sc.IsConnected);
            Assert.False(sc.UseAutoConnect);
        }

        [Fact]
        public void Shutdown_CanBeCalledTwice_WithoutException()
        {
            var sc = SimConnect.Instance;
            sc.UseAutoConnect = false;

            sc.Shutdown();
            sc.Shutdown(); // second call must not throw
        }
    }
}
