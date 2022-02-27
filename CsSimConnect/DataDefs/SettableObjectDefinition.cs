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
using Rakis.Logging;
using System;
using System.Reflection;

namespace CsSimConnect.DataDefs
{
    public class SettableObjectDefinition : AnnotatedObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(SettableObjectDefinition));

        public SettableObjectDefinition(Type type) : base(type)
        {
        }

        protected override void CopyFields()
        {
            TotalSize = 0;
            fields.Clear();

            foreach (FieldInfo field in Type.GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DataDefinition)) is DataDefinition def)
                {
                    AnnotatedDataDefinition definition = new AnnotatedDataDefinition(def);
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    if (def.Usage != Usage.GetOnly)
                    {
                        TotalSize += def.Size;
                        fields.Add(new(field, definition, (uint)fields.Count));
                    }
                    definition.Setup(field);
                }
                else if (Attribute.GetCustomAttribute(field, typeof(MetaDataDefinition)) is MetaDataDefinition metaDef)
                {
                    log.Warn?.Log("Ignoring metadata tagged field '{0}' for SettableObjectDefinition", field.Name);
                }
            }
            foreach (PropertyInfo prop in Type.GetProperties())
            {
                if (Attribute.GetCustomAttribute(prop, typeof(DataDefinition)) is DataDefinition def)
                {
                    AnnotatedDataDefinition definition = new AnnotatedDataDefinition(def);
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    if (def.Usage != Usage.GetOnly)
                    {
                        TotalSize += def.Size;
                        fields.Add(new(prop, definition, (uint)fields.Count));
                    }
                    definition.Setup(prop);
                }
                else if (Attribute.GetCustomAttribute(prop, typeof(MetaDataDefinition)) is MetaDataDefinition metaDef)
                {
                    log.Warn?.Log("Ignoring metadata tagged property '{0}' for SettableObjectDefinition", prop.Name);
                }
            }

        }

        internal void CopyData<T>(T data, DataBlock block)
            where T : class
        {
            log.Trace?.Log("Reading an instance of {0}.", typeof(T).FullName);

            foreach (AnnotatedMember info in fields)
            {
                log.Trace?.Log("Copying value of '{0}' from member '{1}'", info.Definition.Name, info.Definition.MemberName);
                info.Definition?.SetValue(block, data);
            }
        }

        internal DataBlock SetData<T>(T data)
            where T : class
        {
            DataBlock block = new(TotalSize);
            CopyData(data, block);

            return block;
        }

    }
}
