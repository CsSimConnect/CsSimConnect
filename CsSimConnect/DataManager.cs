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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace CsSimConnect
{
    public sealed class DataManager : MessageManager
    {

        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsAddToDataDefinition(IntPtr handle, UInt32 defId, [MarshalAs(UnmanagedType.LPStr)] string datumName, [MarshalAs(UnmanagedType.LPStr)] string UnitsName, uint datumType, float epsilon, UInt32 datumId);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsSetDataOnSimObject(IntPtr handle, UInt32 defId, UInt32 objId, UInt32 flags, UInt32 count, Int32 unitSize, byte[] data);

        private static readonly Logger log = Logger.GetLogger(typeof(DataManager));

        private static readonly Lazy<DataManager> lazyInstance = new(() => new DataManager(SimConnect.Instance));

        public static DataManager Instance { get { return lazyInstance.Value; } }

        private DataManager(SimConnect simConnect) : base("DefinitionID", 1, simConnect)
        {
            simConnect.OnDisconnect += ResetObjectDefinitions;
        }

        private readonly Dictionary<Type, UInt32> registeredTypes = new();
        private readonly Dictionary<UInt32, ObjectDefinition> definedTypes = new();

        internal ObjectDefinition GetObjectDefinition(uint defineId)
        {
            lock (this)
            {
                return definedTypes.GetValueOrDefault(defineId);
            }
        }

        internal ObjectDefinition GetObjectDefinition(Type type)
        {
            lock (this)
            {
                if (registeredTypes.TryGetValue(type, out UInt32 defId) && definedTypes.TryGetValue(defId, out ObjectDefinition def))
                {
                    return def;
                }
                def = new(type);
                definedTypes.Add(def.DefinitionId, def);
                registeredTypes.Add(type, def.DefinitionId);
                return def;
            }
        }

        private void ResetObjectDefinitions()
        {
            log.Info("Clearing ObjectDefinition registration");
            lock (this)
            {
                registeredTypes.Clear();
                definedTypes.Clear();
            }
        }

        public IMessageResult<T> RequestData<T>()
            where T : class
        {
            Type t = typeof(T);
            log.Debug("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();
            MessageResult<ObjectData> simData =  RequestManager.Instance.RequestObjectData<ObjectData>(def);

            MessageResult<T> result = new();
            simData.Subscribe((ObjectData msg) => {
                log.Trace("RequestData<{0}> callback called", t.FullName);
                try
                {
                    result.OnNext(def.GetData<T>(msg));
                }
                catch (Exception e)
                {
                    result.OnError(e);
                }
            });
            return result;
        }

        public IMessageStream<T> RequestData<T>(ObjectDataPeriod period, bool onlyWhenChanged = false)
            where T : class
        {
            Type t = typeof(T);
            log.Debug("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, onlyWhenChanged: onlyWhenChanged);

            MessageStream<T> result = new(1);
            simData.Subscribe((ObjectData msg) => {
                if (log.IsTraceEnabled())
                {
                    log.Trace("RequestData<{0}> callback called", t.FullName);
                }
                try
                {
                    result.OnNext(def.GetData<T>(msg));
                }
                catch (Exception e)
                {
                    result.OnError(e);
                }
            }, onError: result.OnError, onCompleted: result.OnCompleted);
            return result;
        }

        public void RequestData<T>(T data, ObjectDataPeriod period = ObjectDataPeriod.Once, bool onlyWhenChanged = false)
            where T : class, IUpdatableData
        {
            Type t = typeof(T);
            log.Debug("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();

            MessageStream<ObjectData> result = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, onlyWhenChanged: onlyWhenChanged);
            result.Subscribe((ObjectData msg) => {
                if (log.IsTraceEnabled())
                {
                    log.Trace("RequestData<{0}> callback called", t.FullName);
                }
                try
                {
                    def.CopyData(msg, data);
                    data.OnNext();
                }
                catch (Exception e)
                {
                    data.OnError(e);
                }
            }, onError: result.OnError, onCompleted: result.OnCompleted);
        }

        public void RequestData<T>(T data, Action onNext = null, Action<Exception> onError = null, Action onComplete = null, ObjectDataPeriod period = ObjectDataPeriod.Once, bool onlyWhenChanged = false)
            where T : class
        {
            Type t = typeof(T);
            log.Debug("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, onlyWhenChanged: onlyWhenChanged);

            simData.Subscribe((ObjectData msg) => {
                if (log.IsTraceEnabled())
                {
                    log.Trace("RequestData<{0}> callback called", t.FullName);
                }
                try
                {
                    def.CopyData(msg, data);
                    onNext?.Invoke();
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                }
            }, onError, onComplete);
        }

        public void SetData<T>(T data)
        {
            Type t = typeof(T);
            log.Debug("SetData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();

            byte[] defBlock = def.SetData(data);
            RegisterCleanup(
                CsSetDataOnSimObject(simConnect.handle, def.DefinitionId, 0, 0, 0, defBlock.Length, defBlock),
                "SetData",
                error => log.Error("Failed to set data on SimObject: {0}", error.Message));
        }

        internal void AddToDefinition(UInt32 defId, ObjectDefinition.DataDefInfo info)
        {
            log.Debug("AddToDataDefinition(..., {0}, '{1}', '{2}', {3}, {4}, {5})", defId, info.Definition.Name, info.Definition.Units, info.Definition.Type.ToString(), info.Definition.Epsilon, info.Definition.Tag);

            RegisterCleanup(
                CsAddToDataDefinition(simConnect.handle, defId, info.Definition.Name, info.Definition.Units, (uint)info.Definition.Type, info.Definition.Epsilon, info.Definition.Tag),
                "RequestObjectData",
                error => log.Error("AddToDataDefinition() failed for DefinitionID {0} var '{1}': {2}", defId, info.Definition.Name, error.Message));
        }

    }
}
