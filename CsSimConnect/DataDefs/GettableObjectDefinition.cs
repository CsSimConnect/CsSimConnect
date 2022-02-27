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
    public class GettableObjectDefinition : AnnotatedObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(GettableObjectDefinition));

        public GettableObjectDefinition(Type type) : base(type)
        {
        }

        private void AddMembers<T>(T[] members) where T : MemberInfo
        {
            foreach (T field in members)
            {
                switch (Attribute.GetCustomAttribute(field, typeof(DefinitionAttribute)))
                {
                    case DataDefinition dataDef:
                        {
                            AnnotatedDataDefinition definition = new AnnotatedDataDefinition(dataDef);
                            if (definition.Size == 0)
                            {
                                definition.Size = DataSize[(uint)dataDef.Type];
                            }
                            if (definition.Usage != Usage.SetOnly)
                            {
                                TotalSize += definition.Size;
                                fields.Add(new(field, definition, (uint)fields.Count));
                            }
                            definition.Setup(field);
                        }
                        break;
                    case DataRequestId dataDef:
                        {
                            AnnotatedMetadataDefinition definition = new DataRequestIdDefinition(dataDef);
                            fields.Add(new(field, definition));
                            definition.Setup(field);
                        }
                        break;
                    case DataObjectId dataDef:
                        {
                            AnnotatedMetadataDefinition definition = new DataObjectIdDefinition(dataDef);
                            fields.Add(new(field, definition));
                            definition.Setup(field);
                        }
                        break;
                    case DataDefineId dataDef:
                        {
                            AnnotatedMetadataDefinition definition = new DataDefineIdDefinition(dataDef);
                            fields.Add(new(field, definition));
                            definition.Setup(field);
                        }
                        break;
                    case DataDefinitionFlags dataDef:
                        {
                            AnnotatedMetadataDefinition definition = new DataDefinitionFlagsDefinition(dataDef);
                            fields.Add(new(field, definition));
                            definition.Setup(field);
                        }
                        break;
                    case DataEntryNumber dataDef:
                        {
                            AnnotatedMetadataDefinition definition = new DataEntryNumberDefinition(dataDef);
                            fields.Add(new(field, definition));
                            definition.Setup(field);
                        }
                        break;
                    case DataCount dataDef:
                        {
                            AnnotatedMetadataDefinition definition = new DataCountDefinition(dataDef);
                            fields.Add(new(field, definition));
                            definition.Setup(field);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        protected override void CopyFields()
        {
            TotalSize = 0;
            fields.Clear();

            AddMembers(Type.GetFields());
            AddMembers(Type.GetProperties());
        }

        internal void CopyData<T>(ObjectData msg, T data)
            where T : class
        {
            log.Trace?.Log("Filling an instance of {0}.", typeof(T).FullName);

            foreach (AnnotatedMember info in fields)
            {
                log.Trace?.Log($"Copying value of '{info.Definition.Name}' into member '{info.Definition.MemberName}'");
                info.Definition?.GetValue(data, msg);
            }
        }

        internal T GetData<T>(ObjectData msg)
            where T : class
        {
            var actualType = typeof(T);
            log.Trace?.Log($"Creating an instance of {actualType.FullName}.");
            T data = (T)Activator.CreateInstance(actualType);
            CopyData(msg, data);

            return data;
        }

    }
}
