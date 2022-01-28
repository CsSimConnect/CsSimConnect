/*
 * Copyright (c) 2021. Bert Laverman
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

using CsSimConnect.DataDefs;
using Rakis.Logging;
using System;
using System.Text;

namespace CsSimConnect
{
    public class DataBlock
    {

        private static readonly Logger log = Logger.GetLogger(typeof(DataBlock));

        public uint Pos { get; set; }
        public byte[] Data { get; }
        public uint Size { get; private set; }

        public DataBlock(uint size)
        {
            Size = size;
            Data = new byte[size];
            Pos = 0;
        }

        public unsafe DataBlock(uint size, byte* src)
        {
            Data = new byte[size];
            Pos = 0;
            fixed (byte* dst = &Data[0])
            {
                Buffer.MemoryCopy(src, dst, Data.Length, size);
            }
            log.Trace?.Log("Copied {0} bytes of data into ObjectData.Data", size);
        }

        public void Reset()
        {
            Pos = 0;
        }

        public Int32 Int32()
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    Int32 result = *((Int32*)p);
                    Pos += sizeof(Int32);
                    return result;
                }
            }
        }

        public void Int32(Int32 value)
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    *((Int32*)p) = value;
                    Pos += sizeof(Int32);
                }
            }
        }

        public Int64 Int64()
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    Int64 result = *((Int64*)p);
                    Pos += sizeof(Int64);
                    return result;
                }
            }
        }

        public void Int64(Int64 value)
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    *((Int64*)p) = value;
                    Pos += sizeof(Int64);
                }
            }
        }

        public float Float32()
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    float result = *((float*)p);
                    Pos += sizeof(float);
                    return result;
                }
            }
        }

        public void Float32(float value)
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    *((float*)p) = value;
                    Pos += sizeof(float);
                }
            }
        }

        public double Float64()
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    double result = *((double*)p);
                    Pos += sizeof(double);
                    return result;
                }
            }
        }

        public void Float64(double value)
        {
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    *((double*)p) = value;
                    Pos += sizeof(double);
                }
            }
        }

        public string FixedString(uint len)
        {
            int strLen = Array.IndexOf<byte>(Data, 0, (int)Pos, (int)len) - (int)Pos;
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    string result = Encoding.Latin1.GetString(p, strLen).Trim();
                    Pos += len;
                    return result;
                }
            }
        }

        public void FixedString(string value, uint len)
        {
            Array.Fill<byte>(Data, 0, (int)Pos, (int)len);
            byte[] strVal = Encoding.Latin1.GetBytes(value);
            Array.Copy(strVal, 0, Data, Pos, strVal.Length);
            Pos += len;
        }

        public string FixedWString(uint len)
        {
            int strLen = 0;
            while ((strLen < (len*2)) && ((Data[Pos + strLen] != 0) || (Data[Pos + strLen + 1] != 0))) strLen += 2;
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    string result = Encoding.Unicode.GetString(p, strLen).Trim();
                    Pos += len*2;
                    return result;
                }
            }
        }

        public void FixedWString(string value, uint len)
        {
            Array.Fill<byte>(Data, 0, (int)Pos, (int)len*2);
            byte[] strVal = Encoding.Unicode.GetBytes(value);
            Array.Copy(strVal, 0, Data, Pos, strVal.Length);
            Pos += len*2;
        }

        public string VariableString(uint maxLen)
        {
            log.Trace?.Log("VariableString({0}, {1}), {2} bytes in Data", Pos, maxLen, Data.Length);
            int strLen = 0;//Array.IndexOf<byte>(Data, 0, (int)Pos, (int)maxLen) - (int)Pos;
            if (maxLen == 0)
            {
                maxLen = (uint)Data.Length - Pos;
                log.Warn?.Log("VariableString called with maxLen 0, setting to {} instead.", maxLen);
            }
            while ((strLen <= maxLen) && (Data[Pos + strLen] != 0)) strLen++;
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    string result = Encoding.Latin1.GetString(p, strLen).Trim();
                    Pos += (uint)result.Length;
                    Pos += 4 - (Pos & 0x03);
                    log.Trace?.Log("String extracted = '{0}'", result);
                    return result;
                }
            }
        }

        public void VariableString(string value)
        {
            byte[] strVal = Encoding.Latin1.GetBytes(value);
            Array.Copy(strVal, 0, Data, Pos, strVal.Length);
            Pos += (uint)strVal.Length;
            Data[Pos++] = 0;
            Pos += 4 - (Pos & 0x03);
        }

        public string VariableWString(uint maxLen)
        {
            log.Trace?.Log("VariableString({0}, {1}), {2} bytes in Data", Pos, maxLen, Data.Length);
            int strLen = 0;//Array.IndexOf<byte>(Data, 0, (int)Pos, (int)maxLen) - (int)Pos;
            if (maxLen == 0)
            {
                maxLen = (uint)Data.Length - Pos;
                log.Warn?.Log("VariableString called with maxLen 0, setting to {} instead.", maxLen);
            }
            while ((strLen <= maxLen) && (Data[Pos + strLen] != 0)) strLen++;
            unsafe
            {
                fixed (byte* p = &Data[Pos])
                {
                    string result = Encoding.Unicode.GetString(p, strLen).Trim();
                    Pos += (uint)result.Length;
                    Pos += 4 - (Pos & 0x03);
                    log.Trace?.Log("String extracted = '{0}'", result);
                    return result;
                }
            }
        }

        public void VariableWString(string value)
        {
            byte[] strVal = Encoding.Unicode.GetBytes(value);
            Array.Copy(strVal, 0, Data, Pos, strVal.Length);
            Pos += (uint)strVal.Length;
            Data[Pos++] = 0;
            Data[Pos++] = 0;
            Pos += 4 - (Pos & 0x03);
        }

        public LatLonAlt LatLonAlt()
        {
            double lat = Float64();
            double lon = Float64();
            double alt = Float64();
            return new(lat, lon, alt);
        }

        public void LatLonAlt(LatLonAlt value)
        {
            Float64(value.Latitude);
            Float64(value.Longitude);
            Float64(value.Altitude);
        }

        public XYZ XYZ()
        {
            double x = Float64();
            double y = Float64();
            double z = Float64();
            return new(x, y, z);
        }

        public void XYZ(XYZ value)
        {
            Float64(value.X);
            Float64(value.Y);
            Float64(value.Z);
        }

        public PBH PBH()
        {
            double pitch = Float64();
            double bank = Float64();
            double heading = Float64();
            return new(pitch, bank, heading);
        }

        public void PBH(PBH value)
        {
            Float64(value.Pitch);
            Float64(value.Bank);
            Float64(value.Heading);
        }

        public InitPosition InitPosition()
        {
            double lat = Float64();
            double lon = Float64();
            double alt = Float64();
            double pitch = Float64();
            double bank = Float64();
            double heading = Float64();
            bool onGround = Int32() != 0;
            int airSpeed = Int32();
            return new InitPosition(lat, lon, alt, pitch, bank, heading, onGround, airSpeed);
        }

        public void InitPosition(InitPosition value)
        {
            Float64(value.Position.Latitude);
            Float64(value.Position.Longitude);
            Float64(value.Position.Altitude);
            Float64(value.Orientation.Pitch);
            Float64(value.Orientation.Bank);
            Float64(value.Orientation.Heading);
            Int32(value.OnGround ? 1 : 0);
            Int32(value.AirSpeed);
        }
    }
}
