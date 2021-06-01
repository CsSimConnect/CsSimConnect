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

using CsSimConnect.Reflection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CsSimConnect.DataDefs
{

    public class ObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(ObjectDefinition));

        public uint DefinitionId { get; init; }

        public bool Defined { private get; set; }
        public bool IsDefined => Defined;

        public Type Type { get; init; }
        public uint TotalDataSize { get; set; }

        public ObjectDefinition(Type type)
        {
            DefinitionId = DataManager.Instance.NextId();
            Type = type;

            if (log.IsDebugEnabled)
            {
                LogDefinition();
            }
        }

        public class DataDefInfo
        {
            public MemberInfo Member { get; init; }
            public DefinitionBase Definition { get; init; }

            public DataDefInfo(MemberInfo member, DefinitionBase definition)
            {
                Member = member;
                Definition = definition;
            }
        }

        private readonly List<DataDefInfo> fields = new();

        private static readonly uint[] DataSize =
        {
            0, // Invalid,

            4, // Int32,
            8, // Int64,
            4, // Float32,
            8, // Float64,

            8, // String8,
            32, // String32,
            64, // String64,
            128, // String128,
            256, // String256,
            260, // String260,
            0, // StringV,

            56, // InitPosition,
            68, // MarkerState,
            44, // Waypoint,
            24, // LatLonAlt,
            24, // XYZ,
            24, // PBH,
            80, // Observer, 32 + sizeof(LatLonAlt) + sizeof(PBH)
            120, // VideoStreamInfo,

            16, // WString8,
            64, // WString32,
            128, // WString64,
            256, // WString128,
            512, // WString256,
            520, // WString260,
            0, // WStringV,

            0, // Max,

        };

        private void CopyFields()
        {
            uint tag = 0;
            TotalDataSize = 0;
            fields.Clear();

            foreach (FieldInfo field in Type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DataDefinition)) is DataDefinition def)
                {
                    def.Tag = tag++;
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    TotalDataSize += def.Size;
                    fields.Add(new(field, def));
                    def.Setup(field);
                }
                else if (Attribute.GetCustomAttribute(field, typeof(MetaDataDefinition)) is MetaDataDefinition metaDef)
                {
                    fields.Add(new(field, metaDef));
                    metaDef.Setup(field);
                }
            }
            foreach (PropertyInfo prop in Type.GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(DataDefinition)) is DataDefinition def)
                {
                    def.Tag = tag++;
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    TotalDataSize += def.Size;
                    fields.Add(new(prop, def));
                    def.Setup(prop);
                }
                else if (Attribute.GetCustomAttribute(prop, typeof(MetaDataDefinition)) is MetaDataDefinition metaDef)
                {
                    fields.Add(new(prop, metaDef));
                    metaDef.Setup(prop);
                }
            }

        }

        public void DefineObject()
        {
            lock (this)
            {
                if (IsDefined)
                {
                    return;
                }
                var simConnect = SimConnect.Instance;
                var dataMgr = DataManager.Instance;

                fields.Clear();
                CopyFields();
                foreach (DataDefInfo info in fields)
                {
                    if (info.Definition is DataDefinition)
                    {
                        dataMgr.AddToDefinition(DefinitionId, info);
                    }
                }
                Defined = true;
            }
        }

        private void LogDefinition()
        {
            log.Debug?.Log("ObjectDefinition '{0}'", Type.FullName);
            foreach (PropertyInfo prop in Type.GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(DataDefinition)) is DataDefinition def)
                {
                    log.Debug?.Log("  Property '{0}': Name = '{1}', Units = '{2}', Type = {3}, Epsilon = {4}", prop.Name, def.Name, def.Units, def.Type.ToString(), def.Epsilon);
                }
                else if (Attribute.GetCustomAttribute(prop, typeof(DataDefinition)) is MetaDataDefinition meta)
                {
                    log.Debug?.Log("  Property '{0}': Name = '{1}'", prop.Name);
                }
            }
            foreach (FieldInfo field in Type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DataDefinition)) is DataDefinition def)
                {
                    log.Debug?.Log("  Field '{0}'   : Name = '{1}', Units = '{2}', Type = {3}, Epsilon = {4}", field.Name, def.Name, def.Units, def.Type.ToString(), def.Epsilon);
                }
                else if (Attribute.GetCustomAttribute(field, typeof(DataDefinition)) is MetaDataDefinition meta)
                {
                    log.Debug?.Log("  Field '{0}': Name = '{1}'", field.Name);
                }
            }

        }

        internal void CopyData<T>(ObjectData msg, T data)
            where T : class
        {
            log.Trace?.Log("Filling an instance of {0}.", typeof(T).FullName);

            foreach (DataDefInfo info in fields)
            {
                log.Trace?.Log("Copying value of '{0}' into member '{1}'", info.Definition.Name, info.Definition.MemberName);
                info.Definition?.SetValue(data, msg);
            }
        }

        internal T GetData<T>(ObjectData msg)
            where T : class
        {
            log.Trace?.Log("Creating an instance of {0}.", typeof(T).FullName);
            T data = (T)Activator.CreateInstance(typeof(T));
            CopyData(msg, data);

            return data;
        }

        internal byte[] SetData<T>(T data)
        {
            byte[] result = new byte[TotalDataSize];

            return result;
        }
    }
}
