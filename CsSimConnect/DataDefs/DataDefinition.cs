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
    public class DataDefinition : Attribute
    {

        private static readonly Logger log = Logger.GetLogger(typeof(DataDefinition));

        public delegate T ValueGetter<T>(ref uint p);
        public delegate void ValueSetter(object obj, ObjectData data, ref uint pos);

        public string Name { get; set; }
        public string Units { get; set; }
        public DataType Type { get; set; }
        public float Epsilon { get; set; }
        public uint Tag { get; set; }
        public uint Size { get; set; }

        public string MemberName { get; set; }
        private PropertyInfo prop;
        private FieldInfo field;
        public ValueSetter SetValue;

        public DataDefinition(string name)
        {
            Name = name;
            Units = "NULL"; // Default for strings and structs
            Type = DataType.Float64;
            Epsilon = 0.0f;
            Tag = 0xffffffff;
        }

        private void Set<T>(object obj, ValueGetter<T> get, ref uint pos)
        {
            if ((prop != null))
            {
                if (prop.PropertyType.IsAssignableFrom(typeof(T)))
                {
                    prop.SetValue(obj, get(ref pos));
                }
                else if ((field != null) && field.FieldType.IsAssignableFrom(typeof(T)))
                {
                    field.SetValue(obj, get(ref pos));
                }
                else
                {
                    log.Error("Cannot assign a {0} to a {1}.", Type.ToString(), prop.PropertyType.FullName);
                    throw new NoConversionAvailableException(this, Type, prop.PropertyType);
                }
            }
            else if ((field != null))
            {
                if (field.FieldType.IsAssignableFrom(typeof(T)))
                {
                    field.SetValue(obj, get(ref pos));
                }
                else
                {
                    log.Error("Cannot assign a {0} to a {1}.", Type.ToString(), prop.PropertyType.FullName);
                    throw new NoConversionAvailableException(this, Type, prop.PropertyType);
                }
            }
            else
            {
                log.Error("No field or property to assign to.");
                throw new DataDefinitionException(this, "No field or property to assign to.");
            }
        }

        private void SetBoolean(object obj, ValueGetter<bool> get, ref uint pos)
        {
            prop.SetValue(obj, get(ref pos));
        }

        private void SetupDirect()
        {
            switch (Type)
            {
                case DataType.Int32:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => data.AsInt32(ref p), ref pos);
                    break;

                case DataType.Int64:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => data.AsInt64(ref p), ref pos);
                    break;

                case DataType.Float32:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => data.AsFloat32(ref p), ref pos);
                    break;

                case DataType.Float64:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => data.AsFloat64(ref p), ref pos);
                    break;

                case DataType.String8:
                case DataType.String32:
                case DataType.String64:
                case DataType.String128:
                case DataType.String256:
                case DataType.String260:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => data.AsFixedString(ref p, Size), ref pos);
                    break;

                case DataType.StringV:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => data.AsVariableString(ref p, Size), ref pos);
                    break;

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
                    throw new NoConversionAvailableException(this, Type);
            }
        }

        private void SetupBoolean()
        {
            switch (Type)
            {
                case DataType.Int32:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => (data.AsInt32(ref p) != 0), ref pos);
                    break;

                case DataType.Int64:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => (data.AsInt64(ref p) != 0), ref pos);
                    break;

                case DataType.Float32:
                    log.Warn("Converting a Float32 to a bool.");
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => (data.AsFloat32(ref p) != 0), ref pos);
                    break;

                case DataType.Float64:
                    SetValue = (object obj, ObjectData data, ref uint pos) => Set(obj, (ref uint p) => (data.AsFloat64(ref p) != 0), ref pos);
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
                    log.Error("Don't know how to convert a {0} to a bool.", Type.ToString());
                    throw new NoConversionAvailableException(this, Type);
            }
        }

        public void Setup(PropertyInfo prop)
        {
            this.prop = prop;
            MemberName = prop.Name;
            if (prop.PropertyType.IsAssignableFrom(typeof(bool)))
            {
                SetupBoolean();
            }
            else
            {
                SetupDirect();
            }
        }

        public void Setup(FieldInfo field)
        {
            this.field = field;
            MemberName = field.Name;
            if (field.FieldType.IsAssignableFrom(typeof(bool)))
            {
                SetupBoolean();
            }
            else
            {
                SetupDirect();
            }
        }

    }
}
