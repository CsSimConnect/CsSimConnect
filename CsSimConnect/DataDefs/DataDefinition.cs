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
using CsSimConnect.Exc;
using System;
using System.Reflection;

namespace CsSimConnect.Reflection
{

    public enum DataType
    {
        Invalid,

        Int32,
        Int64,
        Float32,
        Float64,

        String8,
        String32,
        String64,
        String128,
        String256,
        String260,
        StringV,

        InitPosition,
        MarkerState,
        Waypoint,
        LatLonAlt,
        XYZ,
        PBH,
        Observer,
        VideoStreamInfo,

        WString8,
        WString32,
        WString64,
        WString128,
        WString256,
        WString260,
        WStringV,

        Max,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DataDefinition : DefinitionBase
    {

        private static readonly Logger log = Logger.GetLogger(typeof(DataDefinition));

        public delegate T ValueGetter<T>();

        public string Units { get; set; }
        public DataType Type { get; set; }
        public float Epsilon { get; set; }
        public uint Tag { get; set; }
        public uint Size { get; set; }

        public Type GetTargetType()
        {
            if (prop != null)
            {
                return prop.PropertyType;
            }
            if (field != null)
            {
                return field.FieldType;
            }
            return null;
        }

        public DataDefinition(string name)
        {
            Name = name;
            Units = "NULL"; // Default for strings and structs
            Type = DataType.Float64;
            Epsilon = 0.0f;
            Tag = 0xffffffff;
        }

        private void Set<T>(object obj, ValueGetter<T> get)
        {
            if ((prop != null))
            {
                if (prop.PropertyType.IsAssignableFrom(typeof(T)))
                {
                    prop.SetValue(obj, get.Invoke());
                }
                else if ((field != null) && field.FieldType.IsAssignableFrom(typeof(T)))
                {
                    field.SetValue(obj, get.Invoke());
                }
                else
                {
                    log.Error?.Log("Cannot assign a {0} to a {1}.", Type.ToString(), prop.PropertyType.FullName);
                    throw new NoConversionAvailableException(this, Type, prop.PropertyType);
                }
            }
            else if ((field != null))
            {
                if (field.FieldType.IsAssignableFrom(typeof(T)))
                {
                    field.SetValue(obj, get.Invoke());
                }
                else
                {
                    log.Error?.Log("Cannot assign a {0} to a {1}.", Type.ToString(), field.FieldType.FullName);
                    throw new NoConversionAvailableException(this, Type, field.FieldType);
                }
            }
            else
            {
                log.Error?.Log("No field or property to assign to.");
                throw new DataDefinitionException(this, "No field or property to assign to.");
            }
        }

        private void SetBoolean(object obj, ValueGetter<bool> get, ref uint pos)
        {
            prop.SetValue(obj, get);
        }

        private void SetupDirect()
        {
            switch (Type)
            {
                case DataType.Int32:
                    SetValue = (obj, data) => Set(obj, () => data.Data.Int32());
                    break;

                case DataType.Int64:
                    SetValue = (obj, data) => Set(obj, () => data.Data.Int64());
                    break;

                case DataType.Float32:
                    SetValue = (obj, data) => Set(obj, () => data.Data.Float32());
                    break;

                case DataType.Float64:
                    SetValue = (obj, data) => Set(obj, () => data.Data.Float64());
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    SetValue = (obj, data) => Set(obj, () => data.Data.FixedString(Size));
                    break;

                case DataType.StringV:
                    SetValue = (obj, data) => Set(obj, () => data.Data.VariableString(Size));
                    break;

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                    SetValue = (obj, data) => Set(obj, () => data.Data.FixedWString(Size));
                    break;

                case DataType.WStringV:
                    SetValue = (obj, data) => Set(obj, () => data.Data.VariableWString(Size));
                    break;


                case DataType.InitPosition:
                case DataType.MarkerState:
                case DataType.Waypoint:
                case DataType.LatLonAlt:
                case DataType.XYZ:
                case DataType.PBH:
                case DataType.Observer:
                case DataType.VideoStreamInfo:

                default:
                    log.Error?.Log("Don't know how to decode a {0}.", Type.ToString());
                    throw new NoConversionAvailableException(this, Type, GetTargetType());
            }
        }

        private void SetupBoolean()
        {
            switch (Type)
            {
                case DataType.Int32:
                    SetValue = (obj, data) => Set(obj, () => data.Data.Int32() != 0);
                    break;

                case DataType.Int64:
                    SetValue = (obj, data) => Set(obj, () => data.Data.Int64() != 0);
                    break;

                case DataType.Float32:
                    log.Warn?.Log("Converting a Float32 to a bool.");
                    SetValue = (obj, data) => Set(obj, () => data.Data.Float32() != 0);
                    break;

                case DataType.Float64:
                    log.Warn?.Log("Converting a Float64 to a bool.");
                    SetValue = (obj, data) => Set(obj, () => data.Data.Float64() != 0);
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                case DataType.StringV:
                case DataType.InitPosition:
                case DataType.MarkerState:
                case DataType.Waypoint:
                case DataType.LatLonAlt:
                case DataType.XYZ:
                case DataType.PBH:
                case DataType.Observer:
                case DataType.VideoStreamInfo:

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                case DataType.WStringV:

                default:
                    log.Error?.Log("Don't know how to convert a {0} to a bool.", Type.ToString());
                    throw new NoConversionAvailableException(this, Type, GetTargetType());
            }
        }

        private void SetupEnum(Type enumType)
        {
            switch (Type)
            {
                case DataType.Int32:
                    SetValue = (obj, data) => Set(obj, () => Enum.ToObject(enumType, data.Data.Int32()));
                    break;

                case DataType.Int64:
                    SetValue = (obj, data) => Set(obj, () => Enum.ToObject(enumType, data.Data.Int64()));
                    break;

                case DataType.Float32:
                    log.Warn?.Log("Converting a Float32 to an enum.");
                    SetValue = (obj, data) => Set(obj, () => Enum.ToObject(enumType, (int)data.Data.Float32()));
                    break;

                case DataType.Float64:
                    log.Warn?.Log("Converting a Float64 to an enum.");
                    SetValue = (obj, data) => Set(obj, () => Enum.ToObject(enumType, (int)data.Data.Float64()));
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                case DataType.StringV:
                case DataType.InitPosition:
                case DataType.MarkerState:
                case DataType.Waypoint:
                case DataType.LatLonAlt:
                case DataType.XYZ:
                case DataType.PBH:
                case DataType.Observer:
                case DataType.VideoStreamInfo:

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                case DataType.WStringV:

                default:
                    log.Error?.Log("Don't know how to convert a {0} to an enum.", Type.ToString());
                    throw new NoConversionAvailableException(this, Type, GetTargetType());
            }
        }

        public void Setup(PropertyInfo prop)
        {
            this.prop = prop;
            if (prop.PropertyType.IsAssignableFrom(typeof(bool)))
            {
                SetupBoolean();
            }
            else if (prop.PropertyType.IsEnum)
            {
                SetupEnum(prop.PropertyType);
            }
            else
            {
                SetupDirect();
            }
        }

        public void Setup(FieldInfo field)
        {
            this.field = field;
            if (field.FieldType.IsAssignableFrom(typeof(bool)))
            {
                SetupBoolean();
            }
            else if (field.FieldType.IsEnum)
            {
                SetupEnum(field.FieldType);
            }
            else
            {
                SetupDirect();
            }
        }

    }
}
