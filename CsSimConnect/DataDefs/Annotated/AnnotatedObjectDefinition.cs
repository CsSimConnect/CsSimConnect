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
using System.Collections.Generic;
using System.Reflection;

namespace CsSimConnect.DataDefs.Annotated
{

    /**
     * <summary>An <c>ObjectDefinition</c> describes a SimConnect Definition Block.</summary>
     */
    public abstract class AnnotatedObjectDefinition : ObjectDefinition
    {

        private static readonly ILogger log = Logger.GetLogger(typeof(AnnotatedObjectDefinition));

        public Type Type { get; init; }

        public AnnotatedObjectDefinition(Type type) : base()
        {
            Type = type;

            if (log.IsDebugEnabled)
            {
                LogDefinition();
            }
        }

        protected readonly List<AnnotatedMember> fields = new();

        /**
         * <summary>Scan the associated class and gather the field definitions through their <see cref="DataDefinition"/> and <see cref="MetaDataDefinition"/> attributes.</summary>
         */
        protected abstract void CopyFields();

        public override void DefineFields()
        {
            base.DefineFields();

            var dataMgr = DataManager.Instance;

            CopyFields();
            foreach (AnnotatedMember info in fields)
            {
                if (info.Definition is AnnotatedDataDefinition)
                {
                    dataMgr.AddToDefinition(DefinitionId, info);
                }
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
