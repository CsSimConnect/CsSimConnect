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

namespace CsSimConnect
{

    public class ObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(ObjectDefinition));

        public uint DefinitionId { get; init; }

        public bool Defined { private get; set; }
        public bool IsDefined => Defined;

        public Type Type { get; init; }

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

        private SortedDictionary<UInt32, DataDefInfo> fields = new();

        private void CopyFields()
        {
            foreach (FieldInfo field in Type.GetFields())
            {
                DataDefinition def = (DataDefinition)Attribute.GetCustomAttribute(field, typeof(DataDefinition));
                if (def != null)
                {
                    uint tag = (uint)(fields.Count + 1);
                    def.Tag = tag;
                    fields.Add(tag, new(field, def));
                }
            }
            foreach (PropertyInfo prop in Type.GetProperties())
            {
                DataDefinition def = (DataDefinition)Attribute.GetCustomAttribute(prop, typeof(DataDefinition));
                if (def != null)
                {
                    uint tag = (uint)(fields.Count + 1);
                    def.Tag = tag;
                    fields.Add(tag, new(prop, def));
                }
            }

        }

        public void DefineObject()
        {
            if (IsDefined)
            {
                return;
            }
            var simConnect = SimConnect.Instance;
            var dataMgr = DataManager.Instance;

            fields.Clear();
            CopyFields();
            foreach (DataDefInfo info in fields.Values)
            {
                dataMgr.AddToDefinition(DefinitionId, info);
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
            return null;
        }
    }
}
