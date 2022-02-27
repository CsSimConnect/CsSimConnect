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

using CsSimConnect.Exc;
using Rakis.Logging;
using System;

namespace CsSimConnect.DataDefs
{
    internal class DynamicDataDefinition : DefinitionBase
    {

        private static readonly Logger log = Logger.GetLogger(typeof(AnnotatedDataDefinition));

        public string Units { get; set; }
        public DataType Type { get; set; }
        public float Epsilon { get; set; }
        public uint Size { get; set; }
        public uint Tag { get; set; }

        internal delegate void ValueTransfer(ObjectData data);
        internal ValueTransfer Set;

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag)
        {
            Name = name;
            Units = units;
            Type = type;
            Epsilon = epsilon;
            Size = size;
            Tag = tag;
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<int> valueSetter)
            : this(name, units, type, epsilon, size, tag)
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) => valueSetter(msg.Data.Int32());
                    break;
                case DataType.Int64:
                    Set = (data) => valueSetter((int)data.Data.Int64());
                    break;
                case DataType.Float32:
                    Set = (data) => valueSetter((int)data.Data.Float32());
                    break;
                case DataType.Float64:
                    Set = (data) => valueSetter((int)data.Data.Float64());
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to an int.");
                    throw new NoConversionAvailableException(this, Type, typeof(int));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<uint> valueSetter)
            : this(name, units, type, epsilon, size, tag)
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) => valueSetter((uint)msg.Data.Int32());
                    break;
                case DataType.Int64:
                    Set = (data) => valueSetter((uint)data.Data.Int64());
                    break;
                case DataType.Float32:
                    Set = (data) => valueSetter((uint)data.Data.Float32());
                    break;
                case DataType.Float64:
                    Set = (data) => valueSetter((uint)data.Data.Float64());
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a uint.");
                    throw new NoConversionAvailableException(this, Type, typeof(uint));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<long> valueSetter)
            : this(name, units, type, epsilon, size, tag)
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) => valueSetter(msg.Data.Int32());
                    break;
                case DataType.Int64:
                    Set = (data) => valueSetter(data.Data.Int64());
                    break;
                case DataType.Float32:
                    Set = (data) => valueSetter((long)data.Data.Float32());
                    break;
                case DataType.Float64:
                    Set = (data) => valueSetter((long)data.Data.Float64());
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a long.");
                    throw new NoConversionAvailableException(this, Type, typeof(long));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<ulong> valueSetter)
            : this(name, units, type, epsilon, size, tag)
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) => valueSetter((ulong)msg.Data.Int32());
                    break;
                case DataType.Int64:
                    Set = (data) => valueSetter((ulong)data.Data.Int64());
                    break;
                case DataType.Float32:
                    Set = (data) => valueSetter((ulong)data.Data.Float32());
                    break;
                case DataType.Float64:
                    Set = (data) => valueSetter((ulong)data.Data.Float64());
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a ulong.");
                    throw new NoConversionAvailableException(this, Type, typeof(ulong));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<bool> valueSetter)
            : this(name, units, type, epsilon, size, tag)
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) => valueSetter(0 != msg.Data.Int32());
                    break;
                case DataType.Int64:
                    Set = (data) => valueSetter(0 != data.Data.Int64());
                    break;
                case DataType.Float32:
                    Set = (data) => valueSetter(0 != data.Data.Float32());
                    break;
                case DataType.Float64:
                    Set = (data) => valueSetter(0 != data.Data.Float64());
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a bool.");
                    throw new NoConversionAvailableException(this, Type, typeof(bool));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<string> valueSetter)
            : this(name, units, type, epsilon, size, tag)
        {
            switch (Type)
            {
                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    Set = (data) => valueSetter(data.Data.FixedString(Size));
                    break;

                case DataType.StringV:
                    Set = (data) => valueSetter(data.Data.VariableString(Size));
                    break;

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                    Set = (data) => valueSetter(data.Data.FixedWString(Size));
                    break;

                case DataType.WStringV:
                    Set = (data) => valueSetter(data.Data.VariableWString(Size));
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a string.");
                    throw new NoConversionAvailableException(this, Type, typeof(string));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<object> valueSetter)
            : this(name, units, type, epsilon, size, tag)
        {
        }
    }
}