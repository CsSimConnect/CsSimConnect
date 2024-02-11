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

using CsSimConnect.DataDefs;
using System;
using System.Reflection;

namespace CsSimConnect.Exc
{
    public class NoConversionAvailableException : DataDefinitionException
    {
        public DataType SourceType { get; init; }
        public Type TargetType { get; init; }

        public NoConversionAvailableException(DefinitionBase def, DataType source, Type target)
            : base(def, $"No conversion available from {source} to {target.FullName}")
        {
            SourceType = source;
            TargetType = target;
        }

        public NoConversionAvailableException(DefinitionBase def, DataType source)
            : base(def, $"DataType {source} not implemented")
        {
            SourceType = source;
        }

        public NoConversionAvailableException(DefinitionBase def, MemberInfo target)
            : base(def, String.Format($"Member type {target.Name} not implemented"))
        {
            if (target is PropertyInfo prop)
            {
                TargetType = prop.PropertyType;
            }
            else if (target is FieldInfo field)
            {
                TargetType = field.FieldType;
            }
        }
    }
}
