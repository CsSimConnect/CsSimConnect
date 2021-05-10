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
using System.Threading;

namespace CsSimConnect
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

            if (log.IsDebugEnabled())
            {
                LogDefinition();
            }
        }

        public class DataDefInfo
        {
            public MemberInfo Member { get; init; }
            public DataDefinition Definition { get; init; }

            public DataDefInfo(MemberInfo member, DataDefinition definition)
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
                DataDefinition def = (DataDefinition)Attribute.GetCustomAttribute(field, typeof(DataDefinition));
                if (def != null)
                {
                    def.Tag = tag++;
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    TotalDataSize += def.Size;
                    fields.Add(new(field, def));
                    def.FinishSetup();
                }
            }
            foreach (PropertyInfo prop in Type.GetProperties())
            {
                DataDefinition def = (DataDefinition)Attribute.GetCustomAttribute(prop, typeof(DataDefinition));
                if (def != null)
                {
                    def.Tag = tag++;
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    TotalDataSize += def.Size;
                    fields.Add(new(prop, def));
                    def.FinishSetup();
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
                    dataMgr.AddToDefinition(DefinitionId, info);
                }
                Defined = true;
            }
        }

        private void LogDefinition()
        {
            log.Debug("ObjectDefinition '{0}'", Type.FullName);
            foreach (PropertyInfo prop in Type.GetProperties())
            {
                DataDefinition def = (DataDefinition)Attribute.GetCustomAttribute(prop, typeof(DataDefinition));
                if (def == null)
                {
                    log.Debug("  Property '{0}': No DataDefinition found", prop.Name);
                }
                else
                {
                    log.Debug("  Property '{0}': Name = '{1}', Units = '{2}', Type = {3}, Epsilon = {4}", prop.Name, def.Name, def.Units, def.Type.ToString(), def.Epsilon);
                }
            }
            foreach (FieldInfo field in Type.GetFields())
            {
                DataDefinition def = (DataDefinition)Attribute.GetCustomAttribute(field, typeof(DataDefinition));
                if (def == null)
                {
                    log.Debug("  Field '{0}'   : No DataDefinition found", field.Name);
                }
                else
                {
                    log.Debug("  Field '{0}'   : Name = '{1}', Units = '{2}', Type = {3}, Epsilon = {4}", field.Name, def.Name, def.Units, def.Type.ToString(), def.Epsilon);
                }
            }

        }

        public T GetData<T>(ObjectData data)
            where T : class
        {
            log.Trace("Creating an instance of {0}.", typeof(T).FullName);
            T obj = (T)Activator.CreateInstance(typeof(T));
            uint pos = 0;
            foreach (DataDefInfo info in fields)
            {
                if (info.Member is PropertyInfo prop)
                {
                    log.Trace("Copying value of {0} into property {1}", info.Definition.Name, prop.Name);
                    info.Definition.SetPropertyValue(obj, prop, data, ref pos);
                }
                else if (info.Member is FieldInfo field)
                {
                    log.Trace("Copying value of {0} into field {1}", info.Definition.Name, field.Name);
                    info.Definition.SetFieldValue(obj, field, data, ref pos);
                }
                else
                {
                    throw new NoConversionAvailableException(info.Member);
                }
            }

            return obj;
        }
    }
}
