﻿/*
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

using CsSimConnect.DataDefs.Standard;
using CsSimConnect.Exc;
using CsSimConnect.Reflection;
using Rakis.Logging;
using System;
using System.Reflection;

namespace CsSimConnect.DataDefs.Annotated
{
    public class AnnotatedDataDefinition : MemberDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(AnnotatedDataDefinition));

        public delegate T FieldGetter<T>();
        public delegate void FieldSetter<T>(T value);

        public string Units { get; set; }
        public DataType Type { get; set; }
        public float Epsilon { get; set; }
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

        public AnnotatedDataDefinition(DataDefinition attr) : base(attr.Name, attr.Usage)
        {
            Units = attr.Units;
            Type = attr.Type;
            Epsilon = attr.Epsilon;
            Size = attr.Size;
        }

        private void ToObject<T>(object obj, FieldGetter<T> get)
        {
            if (prop != null)
            {
                if (prop.PropertyType.IsAssignableFrom(typeof(T)))
                {
                    prop.SetValue(obj, get.Invoke());
                }
                else if (field != null && field.FieldType.IsAssignableFrom(typeof(T)))
                {
                    field.SetValue(obj, get.Invoke());
                }
                else
                {
                    log.Error?.Log($"Cannot assign a {Type} to a {prop.PropertyType.FullName}.");
                    throw new NoConversionAvailableException(this, Type, prop.PropertyType);
                }
            }
            else if (field != null)
            {
                if (field.FieldType.IsAssignableFrom(typeof(T)))
                {
                    field.SetValue(obj, get.Invoke());
                }
                else
                {
                    log.Error?.Log($"Cannot assign a {Type} to a {prop.PropertyType.FullName}.");
                    throw new NoConversionAvailableException(this, Type, field.FieldType);
                }
            }
            else
            {
                log.Error?.Log("No field or property to assign to.");
                throw new DataDefinitionException(this, "No field or property to assign to.");
            }
        }

        private void FromObject<T>(object obj, FieldSetter<T> set)
        {
            if (prop != null)
            {
                if (typeof(T).IsAssignableFrom(prop.PropertyType))
                {
                    set.Invoke((T)prop.GetValue(obj));
                }
                else
                {
                    log.Error?.Log("Cannot assign a {0} to a {1}.", prop.PropertyType.FullName, Type.ToString());
                    throw new NoConversionAvailableException(this, Type, prop.PropertyType);
                }
            }
            else if (field != null)
            {
                if (typeof(T).IsAssignableFrom(field.FieldType))
                {
                    set.Invoke((T)field.GetValue(obj));
                }
                else
                {
                    log.Error?.Log("Cannot assign a {0} to a {1}.", field.FieldType.FullName, Type.ToString());
                    throw new NoConversionAvailableException(this, Type, field.FieldType);
                }
            }
            else
            {
                log.Error?.Log("No field or property to assign to.");
                throw new DataDefinitionException(this, "No field or property to assign to.");
            }
        }

        private void ToSCStruct<T>(object obj, FieldGetter<T> get)
        {
            if (prop != null)
            {
                if (prop.PropertyType == typeof(T))
                {
                    prop.SetValue(obj, get.Invoke());
                }
                else
                {
                    log.Error?.Log("Cannot assign a {0} to a {1}.", Type.ToString(), prop.PropertyType.FullName);
                    throw new NoConversionAvailableException(this, Type, prop.PropertyType);
                }
            }
            else if (field != null)
            {
                if (field.FieldType == typeof(T))
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

        private void FromScStruct<T>(object obj, FieldSetter<T> set)
        {
            if (prop != null)
            {
                if (prop.PropertyType == typeof(T))
                {
                    set.Invoke((T)prop.GetValue(obj));
                }
                else
                {
                    log.Error?.Log("Cannot assign a {0} to a {1}.", prop.PropertyType.FullName, Type.ToString());
                    throw new NoConversionAvailableException(this, Type, prop.PropertyType);
                }
            }
            else if (field != null)
            {
                if (field.FieldType == typeof(T))
                {
                    set.Invoke((T)field.GetValue(obj));
                }
                else
                {
                    log.Error?.Log("Cannot assign a {0} to a {1}.", field.FieldType.FullName, Type.ToString());
                    throw new NoConversionAvailableException(this, Type, field.FieldType);
                }
            }
            else
            {
                log.Error?.Log("No field or property to assign to.");
                throw new DataDefinitionException(this, "No field or property to assign to.");
            }
        }

        private void SetBoolean(object obj, FieldGetter<bool> get, ref uint pos)
        {
            prop.SetValue(obj, get);
        }

        private void SetupDirect()
        {
            switch (Type)
            {
                case DataType.Int32:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Int32());
                    SetValue = (data, obj) => FromObject<int>(obj, i => data.Int32(i));
                    break;

                case DataType.Int64:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Int64());
                    SetValue = (data, obj) => FromObject<long>(obj, l => data.Int64(l));
                    break;

                case DataType.Float32:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Float32());
                    SetValue = (data, obj) => FromObject<float>(obj, f => data.Float32(f));
                    break;

                case DataType.Float64:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Float64());
                    SetValue = (data, obj) => FromObject<double>(obj, d => data.Float64(d));
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.FixedString(Size));
                    SetValue = (data, obj) => FromObject<string>(obj, s => data.FixedString(s, Size));
                    break;

                case DataType.StringV:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.VariableString(Size));
                    SetValue = (data, obj) => FromObject<string>(obj, s => data.VariableString(s));
                    break;

                case DataType.WString8:
                case DataType.WString32:
                case DataType.WString64:
                case DataType.WString128:
                case DataType.WString256:
                case DataType.WString260:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.FixedWString(Size));
                    SetValue = (data, obj) => FromObject<string>(obj, s => data.FixedWString(s, Size));
                    break;

                case DataType.WStringV:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.VariableWString(Size));
                    SetValue = (data, obj) => FromObject<string>(obj, i => data.VariableWString(i));
                    break;

                case DataType.LatLonAlt:
                    GetValue = (obj, data) => ToSCStruct(obj, () => data.Data.LatLonAlt());
                    SetValue = (data, obj) => FromScStruct<LatLonAlt>(obj, l => data.LatLonAlt(l));
                    break;

                case DataType.PBH:
                    GetValue = (obj, data) => ToSCStruct(obj, () => data.Data.PBH());
                    SetValue = (data, obj) => FromScStruct<PBH>(obj, pbh => data.PBH(pbh));
                    break;

                case DataType.InitPosition:
                case DataType.MarkerState:
                case DataType.Waypoint:
                case DataType.XYZ:
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
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Int32() != 0);
                    SetValue = (data, obj) => FromObject<bool>(obj, l => data.Int32(l ? 1 : 0));
                    break;

                case DataType.Int64:
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Int64() != 0);
                    SetValue = (data, obj) => FromObject<bool>(obj, l => data.Int64(l ? 1 : 0));
                    break;

                case DataType.Float32:
                    log.Warn?.Log("Converting between a Float32 and a bool.");
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Float32() != 0);
                    SetValue = (data, obj) => FromObject<bool>(obj, l => data.Float32(l ? 1 : 0));
                    break;

                case DataType.Float64:
                    log.Warn?.Log("Converting between a Float64 and a bool.");
                    GetValue = (obj, data) => ToObject(obj, () => data.Data.Float64() != 0);
                    SetValue = (data, obj) => FromObject<bool>(obj, l => data.Float64(l ? 1 : 0));
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
                    GetValue = (obj, data) => ToObject(obj, () => Enum.ToObject(enumType, data.Data.Int32()));
                    break;

                case DataType.Int64:
                    GetValue = (obj, data) => ToObject(obj, () => Enum.ToObject(enumType, data.Data.Int64()));
                    break;

                case DataType.Float32:
                    log.Warn?.Log("Converting a Float32 to an enum.");
                    GetValue = (obj, data) => ToObject(obj, () => Enum.ToObject(enumType, (int)data.Data.Float32()));
                    break;

                case DataType.Float64:
                    log.Warn?.Log("Converting a Float64 to an enum.");
                    GetValue = (obj, data) => ToObject(obj, () => Enum.ToObject(enumType, (int)data.Data.Float64()));
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

        protected override void Setup(PropertyInfo prop)
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

        protected override void Setup(FieldInfo field)
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
