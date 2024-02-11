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

using CsSimConnect.Reactive;
using Rakis.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace CsSimConnect.DataDefs.Dynamic
{
    /**
     * <summary>Defines a dynamically built "Definition" block.</summary>
     */
    public class SimObjectData : ObjectDefinition
    {

        private static readonly ILogger log = Logger.GetLogger(typeof(SimObjectData));

        public bool Completed { get; private set; } = false;

        internal List<DynamicDataDefinition> fieldDefinitions = new();

        internal readonly Dictionary<string, int> nameLookup = new();
        internal readonly List<string> names = new();
        internal readonly List<object> values = new();

        internal EventWaitHandle sync = new(false, EventResetMode.ManualReset);

        public SimObjectData() : base()
        {
        }

        public void AddField(string name, Action<bool> valueSetter, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
                ReDefineObject();
            }
        }

        public void AddField(string name, Action<int> valueSetter, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
                ReDefineObject();
            }
        }

        public void AddField(string name, Action<uint> valueSetter, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
                ReDefineObject();
            }
        }

        public void AddField(string name, Action<long> valueSetter, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
                ReDefineObject();
            }
        }

        public void AddField(string name, Action<ulong> valueSetter, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
                ReDefineObject();
            }
        }

        public void AddField(string name, Action<string> valueSetter, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag, valueSetter));
                ReDefineObject();
            }
        }

        public void AddField(string name, string units = "NULL", DataType type = DataType.Float64, float epsilon = 0.0f, uint size = UNSET_DATASIZE)
        {
            lock (fieldDefinitions)
            {
                uint tag = (uint)fieldDefinitions.Count;
                fieldDefinitions.Add(new DynamicDataDefinition(name, units, type, epsilon, size, tag));
                ReDefineObject();
            }
        }

        public void removeField(string name)
        {
            lock (fieldDefinitions)
            {
                int i = 0;
                while (i < fieldDefinitions.Count)
                {
                    if (fieldDefinitions [i].Name == name)
                    {

                    }
                }
            }
        }

        public override void DefineFields()
        {
            base.DefineFields();

            var dataMgr = DataManager.Instance;

            lock(fieldDefinitions)
            {
                foreach (DynamicDataDefinition def in fieldDefinitions)
                {
                    if (!def.Defined)
                    {
                        dataMgr.AddToDefinition(DefinitionId, def);
                        def.Defined = true;
                    }
                }
                names.Clear();
                names.AddRange(Enumerable.Repeat<string>(null, fieldDefinitions.Count));
                nameLookup.Clear();

                values.Clear();
                values.AddRange(Enumerable.Repeat<object>(null, fieldDefinitions.Count));

                foreach (DynamicDataDefinition def in fieldDefinitions)
                {
                    names[(int)def.Tag] = def.Name;
                    nameLookup.Add(def.Name, (int)def.Tag);
                }
            }
        }

        internal void SetData(ObjectData msg)
        {
            foreach (DynamicDataDefinition def in fieldDefinitions)
            {
                values [(int)def.Tag] = def.Set(msg);
            }
        }

        private void OnNext()
        {
            sync.Set();
        }

        private void OnComplete()
        {
            Completed = true;
            sync.Set();
        }

        public DynamicDataDefinition this[int i] => fieldDefinitions[i];

        private readonly DataTable dataTable = new(nameof(SimObjectData));

        public DataTable GetSchemaTable()
        {
            if (dataTable.Columns.Count == 0)
            {
                foreach (DynamicDataDefinition def in fieldDefinitions)
                {
                    var column = new DataColumn();
                    column.ColumnName = def.Name;
                    column.DataType = def.TargetType;
                    column.ReadOnly = true;
                    column.Unique = false;
                }
            }
            return dataTable;
        }

        public IMessageResult<SimDataRecord> Request(uint objectId = RequestManager.SimObjectUser)
        {
            log.Debug?.Log($"Request()");

            DefineObject();
            Completed = false;
            MessageResult<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(DefinitionId, objectId: objectId);

            MessageResult<SimDataRecord> result = new();
            SimDataRecord record = new(this);
            simData.Subscribe((ObjectData msg) =>
            {
                log.Trace?.Log($"Request callback called");
                try
                {
                    SetData(msg);
                    result.OnNext(record);
                    OnNext();
                }
                catch (Exception e)
                {
                    result.OnError(e);
                }
            }, onCompleted: () => OnComplete());
            return result;
        }

        /**
         * <summary>Request data from the user's vehicle or an AI one.</summary>
         * <param name="objectDef">The object describing the data to be requested.</param>
         * <param name="onNext">The callback to be called after all data from a single response has been processed.</param>
         * <param name="onError">The callback to be called when an error is received.</param>
         * <param name="onComplete">The callback to be called when the stream is closed.</param>
         * <param name="period">How often the data must be returned.</param>
         * <param name="objectId">The id of the object for which the data is requested, defaulted to <see cref="RequestManager.SimObjectUser"/>.</param>
         * <param name="onlyWhenChanged">If <code>true</code>, only return the data when it has changed.</param>
         */
        public void Request(Action<SimDataRecord> onNext, Action<Exception> onError = null, Action onComplete = null, ObjectDataPeriod period = ObjectDataPeriod.Once, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
        {
            log.Debug?.Log($"void Request(DynamicObjectDefinition)");

            DefineObject();
            Completed = false;
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(this, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);

            SimDataRecord record = new(this);
            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log($"void RequestData(DynamicObjectDefinition) callback called");
                try
                {
                    SetData(msg);
                    onNext?.Invoke(record);
                    OnNext();
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                }
            }, onError, onCompleted: () => { OnComplete(); onComplete(); });
        }

        /**
         * <summary>Request data from the user's vehicle or an AI one.</summary>
         * <typeparam name="T">The type of an annotated class that is requested.</typeparam>
         * <param name="period">How often the data must be returned.</param>
         * <param name="objectId">The id of the object for which the data is requested, defaulted to <see cref="RequestManager.SimObjectUser"/>.</param>
         * <param name="onlyWhenChanged">If <code>true</code>, only return the data when it has changed.</param>
         * <returns>An <see cref="IMessageStream{T}"/></returns>
         */
        public IMessageStream<SimDataRecord> RequestData(ObjectDataPeriod period, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
        {
            log.Debug?.Log($"IMessageStream<SimDataRecord> Request(DynamicObjectDefinition)");

            DefineObject();
            Completed = false;
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(this, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);

            MessageStream<SimDataRecord> result = new(1);
            SimDataRecord record = new(this);
            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log($"IMessageStream<SimDataRecord> RequestData(DynamicObjectDefinition) callback called");
                try
                {
                    SetData(msg);
                    result.OnNext(record);
                }
                catch (Exception e)
                {
                    result.OnError(e);
                }
            }, onError: result.OnError, onCompleted: () => { OnComplete(); result.OnCompleted(); });
            return result;
        }

    }
}
