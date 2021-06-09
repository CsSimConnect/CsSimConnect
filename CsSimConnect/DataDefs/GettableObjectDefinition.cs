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
using System.Reflection;

namespace CsSimConnect.DataDefs
{
    public class GettableObjectDefinition : ObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(GettableObjectDefinition));

        public GettableObjectDefinition(Type type) : base(type)
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
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    if (def.Usage != Usage.SetOnly)
                    {
                        TotalSize += def.Size;
                        fields.Add(new(field, def, (uint)fields.Count));
                    }
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
                    if (def.Size == 0)
                    {
                        def.Size = DataSize[(uint)def.Type];
                    }
                    if (def.Usage != Usage.SetOnly)
                    {
                        TotalSize += def.Size;
                        fields.Add(new(prop, def, (uint)fields.Count));
                    }
                    def.Setup(prop);
                }
                else if (Attribute.GetCustomAttribute(prop, typeof(MetaDataDefinition)) is MetaDataDefinition metaDef)
                {
                    fields.Add(new(prop, metaDef));
                    metaDef.Setup(prop);
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
                info.Definition?.GetValue(data, msg);
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

    }
}
