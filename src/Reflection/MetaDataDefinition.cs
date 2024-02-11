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

namespace CsSimConnect.Reflection
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class MetaDataDefinition : DefinitionAttribute
    {
        public MetaDataDefinition(string name) : base(name, DataDefs.Usage.GetOnly)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataRequestId : MetaDataDefinition
    {
        public DataRequestId() : base("RequestId")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataObjectId : MetaDataDefinition
    {
        public DataObjectId() : base("ObjectId")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataDefineId : MetaDataDefinition
    {
        public DataDefineId() : base("DefineId")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataDefinitionFlags : MetaDataDefinition
    {
        public DataDefinitionFlags() : base("DefinitionFlags")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataEntryNumber : MetaDataDefinition
    {
        public DataEntryNumber() : base("EntryNumber")
        {
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DataCount : MetaDataDefinition
    {
        public DataCount() : base("Count")
        {
        }
    }
}
