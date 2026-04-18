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

namespace CsSimConnect.Tests
{
    public class SimulatorMappingTests
    {
        // ── FlightSimVersion.FromAppInfo ─────────────────────────────────────

        [Fact]
        public void FromAppInfo_KittyHawk_ReturnsMSFS2020()
        {
            var ver = FlightSimVersion.FromAppInfo("KittyHawk");
            Assert.Equal(FlightSimType.MSFlightSimulator, ver.Type);
            Assert.Equal("2020", ver.Version);
        }

        [Fact]
        public void FromAppInfo_SunRise_ReturnsMSFS2024()
        {
            var ver = FlightSimVersion.FromAppInfo("SunRise");
            Assert.Equal(FlightSimType.MSFlightSimulator, ver.Type);
            Assert.Equal("2024", ver.Version);
        }

        [Fact]
        public void FromAppInfo_Prepar3Dv4_ReturnsPrepar3Dv4()
        {
            var ver = FlightSimVersion.FromAppInfo("Lockheed Martin\u00ae Prepar3D\u00ae v4");
            Assert.Equal(FlightSimType.Prepar3D, ver.Type);
            Assert.Equal("4", ver.Version);
        }

        [Fact]
        public void FromAppInfo_Prepar3Dv5_ReturnsPrepar3Dv5()
        {
            var ver = FlightSimVersion.FromAppInfo("Lockheed Martin\u00ae Prepar3D\u00ae v5");
            Assert.Equal(FlightSimType.Prepar3D, ver.Type);
            Assert.Equal("5", ver.Version);
        }

        [Fact]
        public void FromAppInfo_Prepar3Dv6_ReturnsPrepar3Dv6()
        {
            var ver = FlightSimVersion.FromAppInfo("Lockheed Martin\u00ae Prepar3D\u00ae v6");
            Assert.Equal(FlightSimType.Prepar3D, ver.Type);
            Assert.Equal("6", ver.Version);
        }

        [Fact]
        public void FromAppInfo_Test_ReturnsTestType()
        {
            var ver = FlightSimVersion.FromAppInfo("Test");
            Assert.Equal(FlightSimType.Test, ver.Type);
            Assert.Equal("", ver.Version);
        }

        [Fact]
        public void FromAppInfo_UnknownName_ReturnsUnknown()
        {
            var ver = FlightSimVersion.FromAppInfo("SomeRandomSim");
            Assert.Equal(FlightSimType.Unknown, ver.Type);
            Assert.Equal("", ver.Version);
        }

        [Fact]
        public void FromAppInfo_EmptyString_ReturnsUnknown()
        {
            var ver = FlightSimVersion.FromAppInfo("");
            Assert.Equal(FlightSimType.Unknown, ver.Type);
        }

        // ── FlightSimVersion.ToString ────────────────────────────────────────

        [Fact]
        public void ToString_MSFS2020_FormatsCorrectly()
        {
            var ver = FlightSimVersion.FromAppInfo("KittyHawk");
            Assert.Equal("MSFS2020", ver.ToString());
        }

        [Fact]
        public void ToString_MSFS2024_FormatsCorrectly()
        {
            var ver = FlightSimVersion.FromAppInfo("SunRise");
            Assert.Equal("MSFS2024", ver.ToString());
        }

        [Fact]
        public void ToString_Prepar3Dv5_FormatsCorrectly()
        {
            var ver = FlightSimVersion.FromAppInfo("Lockheed Martin\u00ae Prepar3D\u00ae v5");
            Assert.Equal("P3D5", ver.ToString());
        }

        // ── AppInfo (internal string-name constructor) ───────────────────────

        [Fact]
        public void AppInfo_KittyHawk_HasCorrectSimulator()
        {
            var info = new AppInfo("KittyHawk");
            Assert.Equal(FlightSimType.MSFlightSimulator, info.Simulator.Type);
            Assert.Equal("2020", info.Simulator.Version);
        }

        [Fact]
        public void AppInfo_SunRise_HasCorrectSimulator()
        {
            var info = new AppInfo("SunRise");
            Assert.Equal(FlightSimType.MSFlightSimulator, info.Simulator.Type);
            Assert.Equal("2024", info.Simulator.Version);
        }

        [Fact]
        public void AppInfo_Name_IsPreserved()
        {
            var info = new AppInfo("KittyHawk");
            Assert.Equal("KittyHawk", info.Name);
        }
    }
}
