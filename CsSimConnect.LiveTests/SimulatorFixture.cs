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

using CsSimConnect.Sim;
using System;
using System.Threading;

namespace CsSimConnect.LiveTests
{
    /// <summary>
    /// xUnit class fixture that connects to the default SimConnect instance before
    /// each test class and disconnects afterwards.
    ///
    /// Usage:
    ///   public class MyTests : IClassFixture&lt;SimulatorFixture&gt;
    ///   {
    ///       public MyTests(SimulatorFixture fixture) { ... }
    ///   }
    /// </summary>
    public sealed class SimulatorFixture : IDisposable
    {
        public SimConnect Sim { get; }
        public bool IsConnected => Sim.IsConnected;

        /// <summary>
        /// True when a live simulator was reachable. Tests should skip when false.
        /// </summary>
        public bool IsAvailable { get; private set; }

        /// <summary>
        /// Human-readable reason why the fixture is unavailable (for skip messages).
        /// </summary>
        public string UnavailableReason { get; private set; } = string.Empty;

        /// <summary>
        /// Info received from the simulator's OPEN message. Available after a
        /// successful connection; null until the OnOpen event fires.
        /// </summary>
        public AppInfo? Info { get; private set; }

        public SimulatorFixture()
        {
            Sim = SimConnect.Instance;

            if (Environment.GetEnvironmentVariable("CSTESTS_LIVE") != "1")
            {
                UnavailableReason = "Live tests opt-in required: set CSTESTS_LIVE=1 and start a simulator.";
                return;
            }

            using var openEvent = new ManualResetEventSlim(false);

            Sim.OnOpen += info =>
            {
                Info = info;
                openEvent.Set();
            };

            try
            {
                if (Sim.Connect())
                {
                    // Wait up to 10 s for the OPEN message
                    openEvent.Wait(TimeSpan.FromSeconds(10));

                    // If OnOpen didn't fire within the timeout, try the cached AppInfo
                    if (Info == null && Sim.Info?.Simulator.Type != FlightSimType.Unknown)
                        Info = Sim.Info;

                    IsAvailable = Info != null;
                    if (!IsAvailable)
                        UnavailableReason = "SimConnect connected but OPEN message was not received within 10 s.";
                }
                else
                {
                    UnavailableReason = "SimConnect.Connect() returned false — is the simulator running?";
                }
            }
            catch (DllNotFoundException ex)
            {
                UnavailableReason = $"SimConnect DLL not found: {ex.Message}";
            }
            catch (Exception ex)
            {
                UnavailableReason = $"Connection failed: {ex.Message}";
            }
        }

        public void Dispose()
        {
            if (Sim.IsConnected)
                Sim.Disconnect();
        }
    }
}
