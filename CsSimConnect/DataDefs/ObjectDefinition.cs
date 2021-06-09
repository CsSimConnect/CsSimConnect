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

    public class DataDefInfo
    {
        public MemberInfo Member { get; init; }
        public DefinitionBase Definition { get; init; }
        public uint Tag { get; init; }

        public DataDefInfo(MemberInfo member, DefinitionBase definition, uint tag = 0)
        {
            Member = member;
            Definition = definition;
            Tag = tag;
        }
    }

    public abstract class ObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(ObjectDefinition));

        public uint DefinitionId { get; private set; }

        public bool Defined { private get; set; }
        public bool IsDefined => Defined;

        public Type Type { get; init; }
        public uint TotalSize { get; set; }

        public ObjectDefinition(Type type)
        {
            Type = type;
            DefinitionId = DataManager.Instance.NextId();

            if (log.IsDebugEnabled)
            {
                LogDefinition();
            }
        }

        protected readonly List<DataDefInfo> fields = new();

        protected static readonly uint[] DataSize =
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

        protected abstract void CopyFields();

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

    }
}
