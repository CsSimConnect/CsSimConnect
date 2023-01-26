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
using CsSimConnect.DataDefs.Annotated;
using CsSimConnect.DataDefs.Dynamic;
using CsSimConnect.Exc;
using CsSimConnect.Reactive;
using Rakis.Logging;
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
        private static extern long CsClearDataDefinition(IntPtr handle, UInt32 defId);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsSetDataOnSimObject(IntPtr handle, UInt32 defId, UInt32 objId, UInt32 flags, UInt32 count, Int32 unitSize, byte[] data);

        private static readonly ILogger log = Logger.GetLogger(typeof(DataManager));

        private static readonly Lazy<DataManager> lazyInstance = new(() => new DataManager(SimConnect.Instance));

        public static DataManager Instance { get { return lazyInstance.Value; } }

        private DataManager(SimConnect simConnect) : base("DefinitionID", 1, simConnect)
        {
            simConnect.OnDisconnect += _ => ResetObjectDefinitions();
        }

        private readonly Dictionary<Type, GettableObjectDefinition> registeredGettableTypes = new();
        private readonly Dictionary<Type, SettableObjectDefinition> registeredSettableTypes = new();
        private readonly Dictionary<UInt32, AnnotatedObjectDefinition> definedTypes = new();

        /**
         * <summary>Check if a certain type is already known as an annotated <see cref="GettableObjectDefinition"/> and return it if so. If not,
         * add this type as a defined, gettable type.</summary>
         * <param name="type">The type to search for or register.</param>
         */
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

        /**
         * <summary>Check if a certain type is already known as an annotated <see cref="SettableObjectDefinition"/> and return it if so. If not,
         * add this type as a defined, settable type.</summary>
         * <param name="type">The type to search for or register.</param>
         */
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

        /**
         * <summary>Reset all registrations by clearing the registration maps.</summary>
         */
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
        public void RequestDynamicData(SimObjectData objectDef, Action onNext = null, Action<Exception> onError = null, Action onComplete = null, ObjectDataPeriod period = ObjectDataPeriod.Once, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
        {
            log.Debug?.Log($"RequestData(DynamicObjectDefinition)");

            objectDef.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(objectDef, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);

            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log($"RequestData(DynamicObjectDefinition) callback called");
                try
                {
                    objectDef.SetData(msg);
                    onNext?.Invoke();
                }
                catch (Exception e)
                {
                    onError?.Invoke(e);
                }
            }, onError, onComplete);
        }

        /**
         * <summary>Request data once from the user's vehicle or an AI one.</summary>
         * <typeparam name="T">The type of an annotated class that is requested.</typeparam>
         * <param name="objectId">The id of the object for which the data is requested, defaulted to <see cref="RequestManager.SimObjectUser"/>.</param>
         */
        public IMessageResult<T> RequestData<T>(uint objectId =RequestManager.SimObjectUser)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log($"RequestData<{t.FullName}>()");

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
            def.DefineObject();
            MessageResult<ObjectData> simData =  RequestManager.Instance.RequestObjectData<ObjectData>(def.DefinitionId, objectId: objectId);

            MessageResult<T> result = new();
            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log($"RequestData<{t.FullName}> callback called");
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

        /**
         * <summary>Request data from the user's vehicle or an AI one.</summary>
         * <typeparam name="T">The type of an annotated class that is requested.</typeparam>
         * <param name="period">How often the data must be returned.</param>
         * <param name="objectId">The id of the object for which the data is requested, defaulted to <see cref="RequestManager.SimObjectUser"/>.</param>
         * <param name="onlyWhenChanged">If <code>true</code>, only return the data when it has changed.</param>
         * <returns>An <see cref="IMessageStream{T}"/></returns>
         */
        public IMessageStream<T> RequestData<T>(ObjectDataPeriod period, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log($"RequestData<{t.FullName}>()");

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
            def.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);

            MessageStream<T> result = new(1);
            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log($"RequestData<{t.FullName}> callback called");
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

        /**
         * <summary>Request data from the user's vehicle or an AI one.</summary>
         * <typeparam name="T">The type of an annotated class that is requested.</typeparam>
         * <param name="data">The object into which the data must be stored and whose callbacks will be called.</param>
         * <param name="period">How often the data must be returned.</param>
         * <param name="objectId">The id of the object for which the data is requested, defaulted to <see cref="RequestManager.SimObjectUser"/>.</param>
         * <param name="onlyWhenChanged">If <code>true</code>, only return the data when it has changed.</param>
         */
        public void RequestData<T>(T data, ObjectDataPeriod period = ObjectDataPeriod.Once, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
            where T : class, IUpdatableData
        {
            Type t = typeof(T);
            log.Debug?.Log($"RequestData<{t.FullName}>()");

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
            def.DefineObject();

            MessageStream<ObjectData> result = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);
            result.Subscribe((ObjectData msg) => {
                log.Trace?.Log($"RequestData<{t.FullName}> callback called");
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


        /**
         * <summary>Request data from the user's vehicle or an AI one.</summary>
         * <typeparam name="T">The type of an annotated class that is requested.</typeparam>
         * <param name="data">The object into which the data must be stored.</param>
         * <param name="onNext">The callback to be called whenever new data has been received.</param>
         * <param name="onError">The callback to be called when an error is received.</param>
         * <param name="onComplete">The callback to be called when the stream is closed.</param>
         * <param name="period">How often the data must be returned.</param>
         * <param name="objectId">The id of the object for which the data is requested, defaulted to <see cref="RequestManager.SimObjectUser"/>.</param>
         * <param name="onlyWhenChanged">If <code>true</code>, only return the data when it has changed.</param>
         */
        public void RequestData<T>(T data, Action onNext = null, Action<Exception> onError = null, Action onComplete = null, ObjectDataPeriod period = ObjectDataPeriod.Once, uint objectId = RequestManager.SimObjectUser, bool onlyWhenChanged = false)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log($"RequestData<{t.FullName}>()");

            GettableObjectDefinition def = GetObjectDefinitionForGet(t);
            def.DefineObject();
            MessageStream<ObjectData> simData = RequestManager.Instance.RequestObjectData<ObjectData>(def, period, objectId: objectId, onlyWhenChanged: onlyWhenChanged);

            simData.Subscribe((ObjectData msg) => {
                log.Trace?.Log($"RequestData<{t.FullName}> callback called");
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

        /**
         * <summary>Request a stream of data on a type of simulation objects.</summary>
         * <typeparam name="T">The type of an annotated class that is requested.</typeparam>
         * <param name="objectType">The type of object about which data is requested.</param>
         * <param name="radiusInMeters">The maximal distance to the user's object for the returned data.</param>
         */
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
                log.Trace?.Log($"RequestDataOnObjectType<{t.FullName}> callback called");
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

        internal void AddToDefinition(uint defId, AnnotatedMember info, bool forSet = false)
        {
            if ((info.Definition is AnnotatedDataDefinition def) && def.CanBeUsed(forSet))
            {
                log.Trace?.Log($"AddToDataDefinition(..., {defId}, '{def.Name}', '{def.Units}', {def.Type}, {def.Epsilon}, {info.Tag})");

                RegisterCleanup(
                    CsAddToDataDefinition(simConnect.handle, defId, def.Name, def.Units, (uint)def.Type, def.Epsilon, info.Tag),
                    "AddToDataDefinition",
                    error => log.Error?.Log($"AddToDataDefinition() failed for DefinitionID {defId} var '{def.Name}': {error.Message}"));
            }
            else if (!forSet && (info.Definition is AnnotatedMetadataDefinition meta))
            {
                log.Warn?.Log($"Cannot add MetaDataDefinition '{meta.Name}' to definition block {defId}.");
            }
            else
            {
                throw new DataDefinitionException(info.Definition, "Don't know how to add this to a definition block.");
            }
        }

        internal void AddToDefinition(uint defId, DynamicDataDefinition def, bool forSet = false)
        {
            log.Trace?.Log($"AddToDataDefinition({defId}, '{def.Name}', '{def.Units}', {def.Type}, {def.Epsilon}, {def.Tag})");

            RegisterCleanup(
                CsAddToDataDefinition(simConnect.handle, defId, def.Name, def.Units, (uint)def.Type, def.Epsilon, def.Tag),
                "AddToDataDefinition",
                error => log.Error?.Log($"AddToDataDefinition() failed for DefinitionID {defId} var '{def.Name}': {error.Message}"));
        }

        internal void ClearDefinition(uint defId)
        {
            log.Trace?.Log($"ClearDefinition({defId})");

            RegisterCleanup(
                CsClearDataDefinition(simConnect.handle, defId),
                "ClearDefinition",
                error => log.Error?.Log($"ClearDefinition() failed for DefinitionID {defId}"));
        }

        public void ClearDefinition<T>(T data)
            where T : class
        {
            Type t = typeof(T);
            log.Debug?.Log("SetData<{0}>()", t.FullName);

            SettableObjectDefinition def = GetObjectDefinitionForSet(t);
            if (def.Defined)
            {
                ClearDefinition(def.DefinitionId);
            }
        }
    }
}
