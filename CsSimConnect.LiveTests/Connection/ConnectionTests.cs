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

using CsSimConnect.LiveTests;
using CsSimConnect.Sim;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CsSimConnect.LiveTests.Connection
{
    /// <summary>
    /// Smoke tests — verify a basic connect / open / disconnect cycle works.
    /// </summary>
    [Collection("Simulator")]
    public class SmokeTests
    {
        private readonly SimulatorFixture _fixture;

        public SmokeTests(SimulatorFixture fixture)
        {
            _fixture = fixture;
        }

        [LiveFact]
        public void Connect_ToRunningSimulator_Succeeds()
        {
            Assert.True(_fixture.IsAvailable, _fixture.UnavailableReason);
        }

        [LiveFact]
        public void Connect_ReceivesOpenMessage_WithKnownSimulator()
        {
            Assert.True(_fixture.IsAvailable, _fixture.UnavailableReason);
            Assert.NotNull(_fixture.Info);
            Assert.NotEqual(FlightSimType.Unknown, _fixture.Info!.Simulator.Type);
        }

        [LiveFact]
        public void Connect_AppInfo_NameIsNotEmpty()
        {
            Assert.True(_fixture.IsAvailable, _fixture.UnavailableReason);
            Assert.NotNull(_fixture.Info);
            Assert.NotEmpty(_fixture.Info!.Name);
        }

        [LiveFact]
        public void Connect_AppInfo_SimulatorVersionIsNotEmpty()
        {
            Assert.True(_fixture.IsAvailable, _fixture.UnavailableReason);
            Assert.NotNull(_fixture.Info);
            Assert.NotEmpty(_fixture.Info!.Simulator.Version);
        }
    }

    // ── Bug #12 — Connect() must use ClientName, not the hardcoded default ───

    /// <summary>
    /// Unit-level (no simulator needed): verify the ClientName property is wired correctly.
    /// </summary>
    public class ClientNameUnitTests
    {
        [Fact]
        public void DefaultInstance_ClientName_IsDefaultClientName()
        {
            Assert.Equal(SimConnect.DefaultClientName, SimConnect.Instance.ClientName);
        }

        [Fact]
        public void Connect_WithCustomName_SetsClientName()
        {
            var sc = SimConnect.Connect("UnitTestClient_Bug12");
            Assert.Equal("UnitTestClient_Bug12", sc.ClientName);
        }
    }

    /// <summary>
    /// Live: verify a custom-named connection actually reaches the simulator.
    /// </summary>
    [Collection("Simulator")]
    public class ClientNameLiveTests
    {
        private readonly SimulatorFixture _fixture;

        public ClientNameLiveTests(SimulatorFixture fixture)
        {
            _fixture = fixture;
        }

        [LiveFact]
        public void Connect_WithCustomClientName_ConnectsSuccessfully()
        {
            var sc = SimConnect.Connect("LiveTest_CustomName");
            Assert.Equal("LiveTest_CustomName", sc.ClientName);

            using var openReceived = new ManualResetEventSlim(false);
            AppInfo? info = null;
            sc.OnOpen += received => { info = received; openReceived.Set(); };

            bool connected = sc.Connect();
            Assert.True(connected, "Custom-named SimConnect failed to connect");

            openReceived.Wait(TimeSpan.FromSeconds(5));
            Assert.NotNull(info);

            sc.Disconnect();
        }
    }

    // ── Bug #15 — connections dict must be safe under concurrent access ───────

    /// <summary>
    /// Unit-level (no simulator needed): concurrent calls to SimConnect.Connect(name)
    /// must return distinct instances per name with no dictionary corruption.
    /// </summary>
    public class ConcurrentConnectionUnitTests
    {
        [Fact]
        public void Connect_ConcurrentWithDistinctNames_ReturnsDistinctInstances()
        {
            const int count = 16;
            var results = new SimConnect[count];

            Parallel.For(0, count, i =>
            {
                results[i] = SimConnect.Connect($"ConcTest_{i}_Bug15");
            });

            Assert.Equal(count, results.Distinct().Count());
        }

        [Fact]
        public void Connect_ConcurrentWithSameName_ReturnsSameInstance()
        {
            const int count = 16;
            var results = new SimConnect[count];

            Parallel.For(0, count, i =>
            {
                results[i] = SimConnect.Connect("SharedName_Bug15");
            });

            Assert.Single(results.Distinct());
        }
    }

    /// <summary>
    /// Live: concurrent connections with distinct names all succeed.
    /// </summary>
    public class ConcurrentConnectionLiveTests
    {
        [LiveFact]
        public void Connect_ConcurrentWithDistinctNames_AllConnectSuccessfully()
        {
            const int count = 4;
            var names = Enumerable.Range(0, count).Select(i => $"LiveConcTest_{i}").ToArray();

            var instances = new SimConnect[count];
            Parallel.For(0, count, i => instances[i] = SimConnect.Connect(names[i]));
            Assert.Equal(count, instances.Distinct().Count());

            var opens = new ManualResetEventSlim[count];
            for (int i = 0; i < count; i++)
            {
                opens[i] = new ManualResetEventSlim(false);
                int idx = i;
                instances[idx].OnOpen += _ => opens[idx].Set();
            }

            var connected = new bool[count];
            Parallel.For(0, count, i => connected[i] = instances[i].Connect());

            for (int i = 0; i < count; i++)
                opens[i].Wait(TimeSpan.FromSeconds(5));

            try
            {
                Assert.All(connected, c => Assert.True(c));
            }
            finally
            {
                foreach (var sc in instances)
                    if (sc.IsConnected) sc.Disconnect();
            }
        }
    }
}
