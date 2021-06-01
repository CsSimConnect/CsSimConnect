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

using CsSimConnect.Exc;
using CsSimConnect.Reflection;
using System;
using System.Reflection;

namespace CsSimConnect.DataDefs
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public abstract class MetaDataDefinition : DefinitionBase
    {

        private static readonly Logger log = Logger.GetLogger(typeof(MetaDataDefinition));

        public Type GetTargetType()
        {
            if (prop != null)
            {
                return prop.PropertyType;
            }
            if (field != null)
            {
                return field.FieldType;
            }
            return null;
        }

        protected abstract uint Get(ObjectData data);

        private void Set(object obj, ObjectData data)
        {
            if ((prop != null))
            {
                if (prop.PropertyType.IsAssignableFrom(typeof(uint)))
                {
                    prop.SetValue(obj, Get(data));
                }
                else if (prop.PropertyType.IsEnum)
                {
                    prop.SetValue(obj, Enum.ToObject(prop.PropertyType, Get(data)));
                }
                else
                {
                    log.Error?.Log("Cannot assign a uint to a {0}.", prop.PropertyType.FullName);
                    throw new NoConversionAvailableException(this, DataType.Int32, prop.PropertyType);
                }
            }
            else if ((field != null))
            {
                if (field.FieldType.IsAssignableFrom(typeof(uint)))
                {
                    field.SetValue(obj, Get(data));
                }
                else if (field.FieldType.IsEnum)
                {
                    field.SetValue(obj, Enum.ToObject(field.FieldType, Get(data)));
                }
                else
                {
                    log.Error?.Log("Cannot assign a uint to a {0}.", field.FieldType.FullName);
                    throw new NoConversionAvailableException(this, DataType.Int32, field.FieldType);
                }
            }
            else
            {
                log.Error?.Log("No field or property to assign to.");
                throw new DataDefinitionException(this, "No field or property to assign to.");
            }
        }

        public void Setup(PropertyInfo prop)
        {
            this.prop = prop;
            this.SetValue = (obj, data) => Set(obj, data);
        }

        public void Setup(FieldInfo field)
        {
            this.field = field;
            this.SetValue = (obj, data) => Set(obj, data);
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DataRequestId : MetaDataDefinition
    {
        public DataRequestId()
        {
            Name = "RequestId";
        }

        protected override uint Get(ObjectData data) => data.RequestId;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DataObjectId : MetaDataDefinition
    {
        public DataObjectId()
        {
            Name = "ObjectId";
        }

        protected override uint Get(ObjectData data) => data.ObjectId;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DataDefineId : MetaDataDefinition
    {
        public DataDefineId()
        {
            Name = "DefineId";
        }

        protected override uint Get(ObjectData data) => data.DefineId;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DataDefinitionFlags : MetaDataDefinition
    {
        public DataDefinitionFlags()
        {
            Name = "DefinitionFlags";
        }

        protected override uint Get(ObjectData data) => data.Flags;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DataEntryNumber : MetaDataDefinition
    {
        public DataEntryNumber()
        {
            Name = "EntryNumber";
        }

        protected override uint Get(ObjectData data) => data.EntryNumber;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
    public class DataCount : MetaDataDefinition
    {
        public DataCount()
        {
            Name = "Count";
        }

        protected override uint Get(ObjectData data) => data.OutOf;
    }
}
