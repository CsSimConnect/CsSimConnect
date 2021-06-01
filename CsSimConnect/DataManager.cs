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
using CsSimConnect.Exc;
using CsSimConnect.Reactive;
using CsSimConnect.Reflection;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

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
            simConnect.OnDisconnect += _ => ResetObjectDefinitions();
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
            log.Info?.Log("Clearing ObjectDefinition registration");
            lock (this)
            {
                registeredTypes.Clear();
                definedTypes.Clear();
            }
        }

        public IMessageResult<T> RequestData<T>(uint objectId =RequestManager.SimObjectUser)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();
            MessageResult<ObjectData> simData =  RequestManager.Instance.RequestObjectData<ObjectData>(def, objectId: objectId);

            MessageResult<T> result = new();
            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log("RequestData<{0}> callback called", t.FullName);
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

        public IMessageStream<T> RequestData<T>(ObjectDataPeriod period, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);

            MessageStream<T> result = new(1);
            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log("RequestData<{0}> callback called", t.FullName);
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

        public void RequestData<T>(T data, ObjectDataPeriod period = ObjectDataPeriod.Once, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
            where T : class, IUpdatableData
        {
            Type t = typeof(T);
            log.Debug?.Log("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();

            MessageStream<ObjectData> result = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);
            result.Subscribe((ObjectData msg) => {
                log.Trace?.Log("RequestData<{0}> callback called", t.FullName);
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

        public void RequestData<T>(T data, Action onNext = null, Action<Exception> onError = null, Action onComplete = null, ObjectDataPeriod period = ObjectDataPeriod.Once, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log("RequestData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);

            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log("RequestData<{0}> callback called", t.FullName);
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

        public const uint UserRadius = 0;
        public const uint MaxRadius = 200000;

        public MessageStream<T> RequestDataOnObjectType<T>(ObjectType objectType, uint radiusInMeters =MaxRadius)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log("RequestDataOnObjectType<{0}>({1}, {2})", t.FullName, objectType.ToString(), radiusInMeters);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestDataOnSimObjectType<ObjectData>(def, objectType: objectType, radiusInMeters: radiusInMeters);

            MessageStream<T> result = new(1);
            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log("RequestDataOnObjectType<{0}> callback called", t.FullName);
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
        public void SetData<T>(T data)
        {
            Type t = typeof(T);
            log.Debug?.Log("SetData<{0}>()", t.FullName);

            ObjectDefinition def = GetObjectDefinition(t);
            def.DefineObject();

            byte[] defBlock = def.SetData(data);
            RegisterCleanup(
                CsSetDataOnSimObject(simConnect.handle, def.DefinitionId, 0, 0, 0, defBlock.Length, defBlock),
                "SetData",
                error => log.Error?.Log("Failed to set data on SimObject: {0}", error.Message));
        }

        internal void AddToDefinition(UInt32 defId, ObjectDefinition.DataDefInfo info)
        {
            if (info.Definition is DataDefinition def)
            {
                log.Debug?.Log("AddToDataDefinition(..., {0}, '{1}', '{2}', {3}, {4}, {5})", defId, def.Name, def.Units, def.Type.ToString(), def.Epsilon, def.Tag);

                RegisterCleanup(
                    CsAddToDataDefinition(simConnect.handle, defId, def.Name, def.Units, (uint)def.Type, def.Epsilon, def.Tag),
                    "RequestObjectData",
                    error => log.Error?.Log("AddToDataDefinition() failed for DefinitionID {0} var '{1}': {2}", defId, def.Name, error.Message));
            }
            else if (info.Definition is MetaDataDefinition meta)
            {
                log.Warn?.Log("Cannot add MetaDataDefinition '{0}' to definition block {1}.", meta.Name, defId);
            }
            else
            {
                throw new DataDefinitionException(info.Definition, "Don't know how to add this to a definition block.");
            }
        }

    }
}
