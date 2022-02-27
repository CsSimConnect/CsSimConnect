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
using System;
using System.Collections.Generic;

namespace CsSimConnect.DataDefs
{
    public class DynamicObjectDefinition : ObjectDefinition
    {

        private static readonly Logger log = Logger.GetLogger(typeof(DynamicObjectDefinition));

        private List<DynamicDataDefinition> fieldDefinitions = new();

        public DynamicObjectDefinition() : base()
        {
        }

        public void AddField(string name, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE, Action<bool> valueSetter = null)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
            }
        }

        public void AddField(string name, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE, Action<int> valueSetter = null)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
            }
        }

        public void AddField(string name, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE, Action<uint> valueSetter = null)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
            }
        }

        public void AddField(string name, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE, Action<long> valueSetter = null)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
            }
        }

        public void AddField(string name, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE, Action<ulong> valueSetter = null)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
            }
        }

        public void AddField(string name, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE, Action<string> valueSetter = null)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
            }
        }

        public override void DefineFields()
        {
            base.DefineFields();

            var dataMgr = DataManager.Instance;

            foreach (DynamicDataDefinition def in fieldDefinitions)
            {

                dataMgr.AddToDefinition(DefinitionId, def);
            }
        }

        internal void SetData(ObjectData msg)
        {
            foreach (DynamicDataDefinition def in fieldDefinitions)
            {
                def.Set(msg);
            }
        }
    }
}
