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

namespace CsSimConnect.Exc
{
    public class NoConversionAvailableException : DataDefinitionException
    {
        public DataType SourceType { get; init; }
        public Type TargetType { get; init; }

        public NoConversionAvailableException(DataDefinition def, DataType source, Type target)
            : base(def, String.Format("No conversion available from {0} to {1}", source.ToString(), target.FullName))
        {
            SourceType = source;
            TargetType = target;
        }

        public NoConversionAvailableException(DataDefinition def, DataType source)
            : base(def, String.Format("DataType {0} not implemented", source.ToString()))
        {
            SourceType = source;
        }

        public NoConversionAvailableException(DataDefinition def, MemberInfo target)
            : base(def, String.Format("Member type {0} not implemented", target.Name))
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
