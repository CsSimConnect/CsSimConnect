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

using Rakis.Logging;
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

    public abstract class DefinitionBase
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(DefinitionBase));

        public string Name { get; set; }
        public Usage Usage { get; set; }
        public bool Defined { get; set; } = false;

        protected DefinitionBase(Usage usage = Usage.Always)
        {
            Usage = usage;
        }

        public bool CanBeUsed(bool forSet = false)
        {
            return (!forSet && (Usage != Usage.SetOnly)) || (forSet && (Usage != Usage.GetOnly));
        }
    }

    public abstract class MemberDefinition : DefinitionBase
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(MemberDefinition));

        internal string MemberName { get { return prop?.Name ?? field?.Name; } }
        protected PropertyInfo prop;
        protected FieldInfo field;

        internal delegate void ValueGetter(object obj, ObjectData data);
        internal ValueGetter GetValue;
        internal delegate void ValueSetter(DataBlock data, object obj);
        internal ValueSetter SetValue;

        protected abstract void Setup(PropertyInfo info);

        protected abstract void Setup(FieldInfo info);

        public void Setup(MemberInfo member)
        {
            switch (member)
            {
                case PropertyInfo info:
                    Setup(info);
                    break;
                case FieldInfo info:
                    Setup(info);
                    break;
                default:
                    log.Error?.Log($"Don't know how to deal with a {member.GetType()}.");
                    break;
            }
        }

        protected MemberDefinition(string name, Usage usage = Usage.Always) : base(usage)
        {
            Name = name;
        }
    }
}
