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

namespace CsSimConnect.DataDefs
{
    public enum Usage
    {
        Ignore,
        GetOnly,
        SetOnly,
        Always
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public abstract class DefinitionBase : Attribute
    {

        internal string MemberName { get { return prop?.Name ?? field?.Name; } }
        protected PropertyInfo prop;
        protected FieldInfo field;

        internal delegate void ValueGetter(object obj, ObjectData data);
        internal ValueGetter GetValue;
        internal delegate void ValueSetter(DataBlock data, object obj);
        internal ValueSetter SetValue;

        public string Name { get; set; }
        public Usage Usage { get; set; }

        public bool CanBeUsed(bool forSet = false)
        {
            return (!forSet && (Usage != Usage.SetOnly)) || (forSet && (Usage != Usage.GetOnly));
        }

        protected DefinitionBase()
        {
            Usage = Usage.Always;
        }
    }
}
