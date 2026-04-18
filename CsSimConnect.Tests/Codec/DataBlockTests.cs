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

using CsSimConnect.DataDefs.Standard;

namespace CsSimConnect.Tests.Codec
{
    public class DataBlockTests
    {
        // ── Int32 ─────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Int32_RoundTrip(int value)
        {
            var block = new DataBlock(4);
            block.Int32(value);
            block.Reset();
            Assert.Equal(value, block.Int32());
        }

        [Fact]
        public void Int32_AdvancesPosition()
        {
            var block = new DataBlock(8);
            block.Int32(1);
            block.Int32(2);
            block.Reset();
            Assert.Equal(1, block.Int32());
            Assert.Equal(2, block.Int32());
            Assert.Equal(8u, block.Pos);
        }

        // ── Int64 ─────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0L)]
        [InlineData(1L)]
        [InlineData(-1L)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void Int64_RoundTrip(long value)
        {
            var block = new DataBlock(8);
            block.Int64(value);
            block.Reset();
            Assert.Equal(value, block.Int64());
        }

        // ── Float32 ───────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0f)]
        [InlineData(1f)]
        [InlineData(-1f)]
        [InlineData(float.MaxValue)]
        [InlineData(float.MinValue)]
        [InlineData(float.NaN)]
        [InlineData(float.PositiveInfinity)]
        public void Float32_RoundTrip(float value)
        {
            var block = new DataBlock(4);
            block.Float32(value);
            block.Reset();
            Assert.Equal(value, block.Float32());
        }

        // ── Float64 ───────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.0)]
        [InlineData(1.0)]
        [InlineData(-1.0)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(51.9225, 4.4792)]   // Amsterdam coordinates
        public void Float64_RoundTrip(double value, double _ = 0)
        {
            var block = new DataBlock(8);
            block.Float64(value);
            block.Reset();
            Assert.Equal(value, block.Float64());
        }

        // ── FixedString ───────────────────────────────────────────────────────

        [Theory]
        [InlineData("Hello")]
        [InlineData("")]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")]
        public void FixedString_RoundTrip(string value)
        {
            const uint len = 64;
            var block = new DataBlock(len);
            block.FixedString(value, len);
            block.Reset();
            Assert.Equal(value, block.FixedString(len));
        }

        [Fact]
        public void FixedString_NullTerminated_ReadsUpToNull()
        {
            const uint len = 16;
            var block = new DataBlock(len);
            block.FixedString("Hi", len);
            block.Reset();
            // The field is padded with zeros; reading back should give only "Hi"
            Assert.Equal("Hi", block.FixedString(len));
        }

        [Fact]
        public void FixedString_TwoSequentialFields_ReadCorrectly()
        {
            var block = new DataBlock(32);
            block.FixedString("Alpha", 16);
            block.FixedString("Beta", 16);
            block.Reset();
            Assert.Equal("Alpha", block.FixedString(16));
            Assert.Equal("Beta", block.FixedString(16));
        }

        // ── FixedWString ──────────────────────────────────────────────────────

        [Theory]
        [InlineData("Hello")]
        [InlineData("")]
        [InlineData("Ünïcödé")]
        public void FixedWString_RoundTrip(string value)
        {
            const uint len = 64;
            var block = new DataBlock(len * 2);
            block.FixedWString(value, len);
            block.Reset();
            Assert.Equal(value, block.FixedWString(len));
        }

        // ── VariableString ────────────────────────────────────────────────────

        [Theory]
        [InlineData("Hello")]
        [InlineData("Short")]
        public void VariableString_RoundTrip(string value)
        {
            var block = new DataBlock(256);
            block.VariableString(value);
            block.Reset();
            Assert.Equal(value, block.VariableString((uint)value.Length + 8));
        }

        // ── LatLonAlt ─────────────────────────────────────────────────────────

        [Fact]
        public void LatLonAlt_RoundTrip()
        {
            var block = new DataBlock(24);
            var pos = new LatLonAlt(51.9225, 4.4792, 100.5);
            block.LatLonAlt(pos);
            block.Reset();
            var result = block.LatLonAlt();
            Assert.Equal(pos.Latitude, result.Latitude);
            Assert.Equal(pos.Longitude, result.Longitude);
            Assert.Equal(pos.Altitude, result.Altitude);
        }

        // ── PBH ───────────────────────────────────────────────────────────────

        [Fact]
        public void PBH_RoundTrip()
        {
            var block = new DataBlock(24);
            var pbh = new PBH(5.0, -3.5, 270.0);
            block.PBH(pbh);
            block.Reset();
            var result = block.PBH();
            Assert.Equal(pbh.Pitch, result.Pitch);
            Assert.Equal(pbh.Bank, result.Bank);
            Assert.Equal(pbh.Heading, result.Heading);
        }

        // ── XYZ ───────────────────────────────────────────────────────────────

        [Fact]
        public void XYZ_RoundTrip()
        {
            var block = new DataBlock(24);
            var xyz = new XYZ(1.1, 2.2, 3.3);
            block.XYZ(xyz);
            block.Reset();
            var result = block.XYZ();
            Assert.Equal(xyz.X, result.X);
            Assert.Equal(xyz.Y, result.Y);
            Assert.Equal(xyz.Z, result.Z);
        }

        // ── InitPosition ──────────────────────────────────────────────────────

        [Fact]
        public void InitPosition_RoundTrip()
        {
            var block = new DataBlock(56);
            var pos = new InitPosition(52.3, 4.9, 1500, 2.0, -1.0, 180.0, false, -1);
            block.InitPosition(pos);
            block.Reset();
            var result = block.InitPosition();
            Assert.Equal(pos.Position.Latitude, result.Position.Latitude);
            Assert.Equal(pos.Position.Longitude, result.Position.Longitude);
            Assert.Equal(pos.Position.Altitude, result.Position.Altitude);
            Assert.Equal(pos.Orientation.Pitch, result.Orientation.Pitch);
            Assert.Equal(pos.Orientation.Bank, result.Orientation.Bank);
            Assert.Equal(pos.Orientation.Heading, result.Orientation.Heading);
            Assert.Equal(pos.OnGround, result.OnGround);
            Assert.Equal(pos.AirSpeed, result.AirSpeed);
        }

        // ── Reset / sequential writes ─────────────────────────────────────────

        [Fact]
        public void Reset_ReturnsPositionToZero()
        {
            var block = new DataBlock(16);
            block.Int32(1);
            block.Int32(2);
            Assert.Equal(8u, block.Pos);
            block.Reset();
            Assert.Equal(0u, block.Pos);
        }

        [Fact]
        public void MixedTypes_Sequential_RoundTrip()
        {
            var block = new DataBlock(20);
            block.Int32(42);
            block.Float64(3.14);
            block.Int32(-7);
            block.Float32(2.5f);
            block.Reset();
            Assert.Equal(42, block.Int32());
            Assert.Equal(3.14, block.Float64());
            Assert.Equal(-7, block.Int32());
            Assert.Equal(2.5f, block.Float32());
        }
    }
}
