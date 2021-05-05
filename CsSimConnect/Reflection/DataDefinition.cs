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

using System;
using System.Reflection;

namespace CsSimConnect.Reflection
{

    public class NoConversionAvailableException : Exception
    {
        public NoConversionAvailableException(DataType source, Type target) : base(String.Format("No conversion available from {0} to {1}", source.ToString(), target.FullName))
        {
        }
        public NoConversionAvailableException(DataType source) : base(String.Format("DataType {0} not implemented", source.ToString()))
        {
        }
        public NoConversionAvailableException(MemberInfo target) : base(String.Format("Member type {0} not implemented", target.Name))
        {
        }
    }

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
    public class DataDefinition : Attribute
    {

        private static readonly Logger log = Logger.GetLogger(typeof(DataDefinition));

        public delegate T ValueGetter<T>(ref uint p);
        public delegate void PropertyValueSetter(object obj, PropertyInfo prop, ObjectData data, ref uint pos);
        public delegate void FieldValueSetter(object obj, FieldInfo prop, ObjectData data, ref uint pos);

        public string Name { get; set; }
        public string Units { get; set; }
        public DataType Type { get; set; }
        public float Epsilon { get; set; }
        public uint Tag { get; set; }
        public uint Size { get; set; }

        public PropertyValueSetter SetPropertyValue;
        public FieldValueSetter SetFieldValue;

        public DataDefinition(string name)
        {
            Name = name;
            Units = "NULL"; // Default for strings and structs
            Type = DataType.Float64;
            Epsilon = 0.0f;
            Tag = 0xffffffff;
        }

        private void Set<T>(object obj, PropertyInfo prop, ValueGetter<T> get, ref uint pos)
        {
            if (prop.PropertyType.IsAssignableFrom(typeof(T)))
            {
                prop.SetValue(obj, get(ref pos));
            }
            else
            {
                log.Error("Cannot assign a {0} to a {1}.", Type.ToString(), prop.PropertyType.FullName);
                throw new NoConversionAvailableException(Type, prop.PropertyType);
            }
            pos += Size;
        }

        private void Set<T>(object obj, FieldInfo prop, ValueGetter<T> get, ref uint pos)
        {
            if (prop.FieldType.IsAssignableFrom(typeof(T)))
            {
                prop.SetValue(obj, get(ref pos));
            }
            else
            {
                log.Error("Cannot assign a {0} to a {1}.", Type.ToString(), prop.FieldType.FullName);
                throw new NoConversionAvailableException(Type, prop.FieldType);
            }
            pos += Size;
        }

        public void FinishSetup()
        {
            switch (Type)
            {
                case DataType.Int32:
                    SetPropertyValue = (object obj, PropertyInfo prop, ObjectData data, ref uint pos) => Set(obj, prop, (ref uint p) => data.AsInt32(p), ref pos);
                    SetFieldValue = (object obj, FieldInfo field, ObjectData data, ref uint pos) => Set(obj, field, (ref uint p) => data.AsInt32(p), ref pos);
                    break;

                case DataType.Int64:
                    SetPropertyValue = (object obj, PropertyInfo prop, ObjectData data, ref uint pos) => Set(obj, prop, (ref uint p) => data.AsInt64(p), ref pos);
                    SetFieldValue = (object obj, FieldInfo field, ObjectData data, ref uint pos) => Set(obj, field, (ref uint p) => data.AsInt64(p), ref pos);
                    break;

                case DataType.Float32:
                    SetPropertyValue = (object obj, PropertyInfo prop, ObjectData data, ref uint pos) => Set(obj, prop, (ref uint p) => data.AsFloat32(p), ref pos);
                    SetFieldValue = (object obj, FieldInfo field, ObjectData data, ref uint pos) => Set(obj, field, (ref uint p) => data.AsFloat32(p), ref pos);
                    break;

                case DataType.Float64:
                    SetPropertyValue = (object obj, PropertyInfo prop, ObjectData data, ref uint pos) => Set(obj, prop, (ref uint p) => data.AsFloat64(p), ref pos);
                    SetFieldValue = (object obj, FieldInfo field, ObjectData data, ref uint pos) => Set(obj, field, (ref uint p) => data.AsFloat64(p), ref pos);
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    SetPropertyValue = (object obj, PropertyInfo prop, ObjectData data, ref uint pos) => Set(obj, prop, (ref uint p) => data.AsFixedString(p, Size), ref pos);
                    SetFieldValue = (object obj, FieldInfo field, ObjectData data, ref uint pos) => Set(obj, field, (ref uint p) => data.AsFixedString(p, Size), ref pos);
                    break;

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
                    log.Error("Don't know how to decode a {0}.", Type.ToString());
                    throw new NoConversionAvailableException(Type);
            }
        }

    }
}
