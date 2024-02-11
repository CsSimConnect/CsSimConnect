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

using CsSimConnect.DataDefs.Annotated;
using CsSimConnect.Exc;
using Rakis.Logging;
using System;

namespace CsSimConnect.DataDefs.Dynamic
{
    public class DynamicDataDefinition : DefinitionBase
    {

        private static readonly ILogger log = Logger.GetLogger(typeof(AnnotatedDataDefinition));

        public string Units { get; set; }
        public DataType Type { get; set; }
        public Type TargetType { get; set; }
        public float Epsilon { get; set; }
        public uint Size { get; set; }
        public uint Tag { get; set; }

        internal delegate object ValueTransfer(ObjectData data);
        internal ValueTransfer Set;

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Type targetType)
        {
            Name = name;
            Units = units;
            Type = type;
            Epsilon = epsilon;
            Size = (size == 0) ? ObjectDefinition.DataSize[(uint)Type] : size;
            Tag = tag;
            TargetType = targetType;
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<int> valueSetter)
            : this(name, units, type, epsilon, size, tag, typeof(int))
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) =>
                    {
                        int obj = msg.Data.Int32();
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                case DataType.Int64:
                    Set = (data) =>
                    {
                        long obj = data.Data.Int64();
                        valueSetter((int)obj);
                        return obj;
                    };
                    break;
                case DataType.Float32:
                    Set = (data) =>
                    {
                        float obj = data.Data.Float32();
                        valueSetter((int)obj);
                        return obj;
                    };
                    break;
                case DataType.Float64:
                    Set = (data) =>
                    {
                        double obj = data.Data.Float64();
                        valueSetter((int)obj);
                        return obj;
                    };
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to an int.");
                    throw new NoConversionAvailableException(this, Type, typeof(int));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<uint> valueSetter)
            : this(name, units, type, epsilon, size, tag, typeof(uint))
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) =>
                    {
                        int obj = msg.Data.Int32();
                        valueSetter((uint)obj);
                        return obj;
                    };
                    break;
                case DataType.Int64:
                    Set = (data) =>
                    {
                        long obj = data.Data.Int64();
                        valueSetter((uint)obj);
                        return obj;
                    };
                    break;
                case DataType.Float32:
                    Set = (data) =>
                    {
                        float obj = data.Data.Float32();
                        valueSetter((uint)obj);
                        return obj;
                    };
                    break;
                case DataType.Float64:
                    Set = (data) =>
                    {
                        double obj = data.Data.Float64();
                        valueSetter((uint)obj);
                        return obj;
                    };
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a uint.");
                    throw new NoConversionAvailableException(this, Type, typeof(uint));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<long> valueSetter)
            : this(name, units, type, epsilon, size, tag, typeof(long))
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) =>
                    {
                        int obj = msg.Data.Int32();
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                case DataType.Int64:
                    Set = (data) =>
                    {
                        long obj = data.Data.Int64();
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                case DataType.Float32:
                    Set = (data) =>
                    {
                        float obj = data.Data.Float32();
                        valueSetter((long)obj);
                        return obj;
                    };
                    break;
                case DataType.Float64:
                    Set = (data) =>
                    {
                        double obj = data.Data.Float64();
                        valueSetter((long)obj);
                        return obj;
                    };
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a long.");
                    throw new NoConversionAvailableException(this, Type, typeof(long));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<ulong> valueSetter)
            : this(name, units, type, epsilon, size, tag, typeof(ulong))
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) =>
                    {
                        int obj = msg.Data.Int32();
                        valueSetter((ulong)obj);
                        return obj;
                    };
                    break;
                case DataType.Int64:
                    Set = (data) =>
                    {
                        long obj = data.Data.Int64();
                        valueSetter((ulong)obj);
                        return obj;
                    };
                    break;
                case DataType.Float32:
                    Set = (data) =>
                    {
                        float obj = data.Data.Float32();
                        valueSetter((ulong)obj);
                        return obj;
                    };
                    break;
                case DataType.Float64:
                    Set = (data) =>
                    {
                        double obj = data.Data.Float64();
                        valueSetter((ulong)obj);
                        return obj;
                    };
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a ulong.");
                    throw new NoConversionAvailableException(this, Type, typeof(ulong));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<bool> valueSetter)
            : this(name, units, type, epsilon, size, tag, typeof(bool))
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) =>
                    {
                        int obj = msg.Data.Int32();
                        valueSetter(0 != obj);
                        return obj;
                    };
                    break;
                case DataType.Int64:
                    Set = (data) =>
                    {
                        long obj = data.Data.Int64();
                        valueSetter(0 != obj);
                        return obj;
                    };
                    break;
                case DataType.Float32:
                    Set = (data) =>
                    {
                        float obj = data.Data.Float32();
                        valueSetter(0 != obj);
                        return obj;
                    };
                    break;
                case DataType.Float64:
                    Set = (data) =>
                    {
                        double obj = data.Data.Float64();
                        valueSetter(0 != obj);
                        return obj;
                    };
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a bool.");
                    throw new NoConversionAvailableException(this, Type, typeof(bool));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<string> valueSetter)
            : this(name, units, type, epsilon, size, tag, typeof(string))
        {
            switch (Type)
            {
                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    Set = (data) =>
                    {
                        string obj = data.Data.FixedString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;

                case DataType.StringV:
                    Set = (data) =>
                    {
                        string obj = data.Data.VariableString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                    Set = (data) =>
                    {
                        string obj = data.Data.FixedWString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;

                case DataType.WStringV:
                    Set = (data) =>
                    {
                        string obj = data.Data.VariableWString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a string.");
                    throw new NoConversionAvailableException(this, Type, typeof(string));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag, Action<object> valueSetter)
            : this(name, units, type, epsilon, size, tag, typeof(object))
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) =>
                    {
                        int obj = msg.Data.Int32();
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                case DataType.Int64:
                    Set = (data) =>
                    {
                        long obj = data.Data.Int64();
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                case DataType.Float32:
                    Set = (data) =>
                    {
                        float obj = data.Data.Float32();
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                case DataType.Float64:
                    Set = (data) =>
                    {
                        double obj = data.Data.Float64();
                        valueSetter(obj);
                        return obj;
                    };
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    Set = (data) =>
                    {
                        string obj = data.Data.FixedString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;

                case DataType.StringV:
                    Set = (data) =>
                    {
                        string obj = data.Data.VariableString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                    Set = (data) =>
                    {
                        string obj = data.Data.FixedWString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;

                case DataType.WStringV:
                    Set = (data) =>
                    {
                        string obj = data.Data.VariableWString(Size);
                        valueSetter(obj);
                        return obj;
                    };
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a string.");
                    throw new NoConversionAvailableException(this, Type, typeof(string));
            }
        }

        public DynamicDataDefinition(string name, string units, DataType type, float epsilon, uint size, uint tag)
            : this(name, units, type, epsilon, size, tag, typeof(object))
        {
            switch (Type)
            {
                case DataType.Int32:
                    Set = (msg) => msg.Data.Int32();
                    break;
                case DataType.Int64:
                    Set = (data) => data.Data.Int64();
                    break;
                case DataType.Float32:
                    Set = (data) => data.Data.Float32();
                    break;
                case DataType.Float64:
                    Set = (data) => data.Data.Float64();
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    Set = (data) => data.Data.FixedString(Size);
                    break;

                case DataType.StringV:
                    Set = (data) => data.Data.VariableString(Size);
                    break;

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                    Set = (data) => data.Data.FixedWString(Size);
                    break;

                case DataType.WStringV:
                    Set = (data) => data.Data.VariableWString(Size);
                    break;
                default:
                    log.Error?.Log($"Cannot assign a {Type} to a string.");
                    throw new NoConversionAvailableException(this, Type, typeof(string));
            }
        }
    }
}