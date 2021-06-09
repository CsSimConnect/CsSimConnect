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

        private readonly Dictionary<Type, GettableObjectDefinition> registeredGettableTypes = new();
        private readonly Dictionary<Type, SettableObjectDefinition> registeredSettableTypes = new();
        private readonly Dictionary<UInt32, ObjectDefinition> definedTypes = new();

        //internal ObjectDefinition GetObjectDefinition(uint defineId)
        //{
        //    lock (this)
        //    {
        //        return definedTypes.GetValueOrDefault(defineId);
        //    }
        //}

        private GettableObjectDefinition GetObjectDefinitionForGet(Type type)
        {
            lock (this)
            {
                if (registeredGettableTypes.TryGetValue(type, out GettableObjectDefinition def))
                {
                    return def;
                }
                def = new(type);
                definedTypes.Add(def.DefinitionId, def);
                registeredGettableTypes.Add(type, def);
                return def;
            }
        }

        private SettableObjectDefinition GetObjectDefinitionForSet(Type type)
        {
            lock (this)
            {
                if (registeredSettableTypes.TryGetValue(type, out SettableObjectDefinition def))
                {
                    return def;
                }
                def = new(type);
                definedTypes.Add(def.DefinitionId, def);
                registeredSettableTypes.Add(type, def);
                return def;
            }
        }

        private void ResetObjectDefinitions()
        {
            log.Info?.Log("Clearing ObjectDefinition registration");
            lock (this)
            {
                registeredGettableTypes.Clear();
                registeredSettableTypes.Clear();
                definedTypes.Clear();
            }
        }

        public IMessageResult<T> RequestData<T>(uint objectId =RequestManager.SimObjectUser)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log("RequestData<{0}>()", t.FullName);

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
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

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
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

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
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

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
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

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
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

        public void SetData<T>(uint objectId, T data)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log("SetData<{0}>()", t.FullName);

            SettableObjectDefinition def = GetObjectDefinitionForSet(t);
            def.DefineObject();

            DataBlock defBlock = def.SetData(data);
            RegisterCleanup(
                CsSetDataOnSimObject(simConnect.handle, def.DefinitionId, objectId, 0, 0, (int)defBlock.Size, defBlock.Data),
                "SetData",
                error => log.Error?.Log("Failed to set data on SimObject: {0}", error.Message));
        }

        internal void AddToDefinition(UInt32 defId, DataDefInfo info, bool forSet = false)
        {
            if ((info.Definition is DataDefinition def) && def.CanBeUsed(forSet))
            {
                log.Debug?.Log("AddToDataDefinition(..., {0}, '{1}', '{2}', {3}, {4}, {5})", defId, def.Name, def.Units, def.Type.ToString(), def.Epsilon, info.Tag);

                RegisterCleanup(
                    CsAddToDataDefinition(simConnect.handle, defId, def.Name, def.Units, (uint)def.Type, def.Epsilon, info.Tag),
                    "RequestObjectData",
                    error => log.Error?.Log("AddToDataDefinition() failed for DefinitionID {0} var '{1}': {2}", defId, def.Name, error.Message));
            }
            else if (!forSet && (info.Definition is MetaDataDefinition meta))
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
