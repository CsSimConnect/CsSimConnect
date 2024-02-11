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

using CsSimConnect.AI;
using CsSimConnect.DataDefs.Annotated;
using CsSimConnect.DataDefs.Dynamic;
using CsSimConnect.DataDefs.Standard;
using CsSimConnect.Reactive;
using Rakis.Logging;
using System;
using System.Runtime.InteropServices;

namespace CsSimConnect
{

    public enum SystemState
    {
        AircraftLoaded,
        DialogMode,
        FlightLoaded,
        FlightPlan,
        FullScreenMode,
        Sim,
    }
    public enum ObjectDataPeriod
    {
        Never,
        Once,
        PerVisualFrame,
        PerSimFrame,
        PerSecond,
    }
    public enum ClientDataPeriod
    {
        Never,
        Once,
        PerVisualFrame,
        WhenSet,
        PerSecond,
    }

    public class RequestManager : MessageManager
    {
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsRequestSystemState(IntPtr handle, UInt32 requestId, [MarshalAs(UnmanagedType.LPStr)] string state);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsRequestDataOnSimObject(IntPtr handle, UInt32 requestId, UInt32 defId, UInt32 objectId, UInt32 period, UInt32 dataRequestFlags, UInt32 origin, UInt32 interval, UInt32 limit);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsRequestDataOnSimObjectType(IntPtr handle, UInt32 requestId, UInt32 defId, UInt32 radius, UInt32 objectType);

        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsAICreateNonATCAircraft(IntPtr handle, [MarshalAs(UnmanagedType.LPStr)] string title, [MarshalAs(UnmanagedType.LPStr)] string tailNumber, ref LatLonAlt pos, ref PBH pbh, UInt32 onGround, UInt32 airSpeed, UInt32 requestId);

        private static readonly ILogger log = Logger.GetLogger(typeof(RequestManager));

        private static readonly Lazy<RequestManager> lazyInstance = new(() => new RequestManager(SimConnect.Instance));

        public static RequestManager Instance { get { return lazyInstance.Value; } }

        private RequestManager(SimConnect simConnect) : base("RequestID", 0, simConnect)
        {
        }

        internal delegate void RequestResultHandler(ref ReceiveStruct msg);
        private class RequestResultHandlerRegistration
        {
            public readonly RequestResultHandler Handler;
            public readonly bool UseOnceOnly;

            public RequestResultHandlerRegistration(RequestResultHandler handler, bool useOnceOnly =true)
            {
                Handler = handler;
                UseOnceOnly = useOnceOnly;
            }
        }

        public MessageResult<SimState> RequestSystemState(SystemState systemState)
        {
            uint requestId = NextId();
            log.Debug?.Log("Request ID {0}: Requesting '{1}'", requestId, systemState.ToString());

            return RegisterResultObserver<SimState>(requestId, CsRequestSystemState(simConnect.handle, requestId, systemState.ToString()), "RequestSystemState");
        }

        public void RequestSystemStateBool(SystemState systemState, Action<bool> callback)
        {
            RequestSystemState(systemState).Subscribe((SimState systemState) => callback(systemState.AsBoolean()));
        }
        public void RequestSystemStateString(SystemState systemState, Action<string> callback)
        {
            RequestSystemState(systemState).Subscribe((SimState systemState) => callback(systemState.StringValue));
        }

        public void RequestDialogMode(Action<bool> callback)
        {
            RequestSystemStateBool(SystemState.DialogMode, callback);
        }
        public void RequestFullScreenMode(Action<bool> callback)
        {
            RequestSystemStateBool(SystemState.FullScreenMode, callback);
        }
        public void RequestSimState(Action<bool> callback)
        {
            RequestSystemStateBool(SystemState.Sim, callback);
        }
        public void RequestAircraftLoaded(Action<string> callback)
        {
            RequestSystemStateString(SystemState.AircraftLoaded, callback);
        }
        public void RequestFlightLoaded(Action<string> callback)
        {
            RequestSystemStateString(SystemState.FlightLoaded, callback);
        }
        public void RequestFlightPlanLoaded(Action<string> callback)
        {
            RequestSystemStateString(SystemState.FlightPlan, callback);
        }

        public const uint SimObjectUser = 0;
        private const uint whenChanged = 0x00000001;
        private const uint taggedFormat = 0x00000002;
        private const uint blockingDispatch = 0x00000004;

        public MessageResult<T> RequestObjectData<T>(uint definitionId, uint objectId =SimObjectUser, bool useBlockingDispatch = false)
            where T : SimConnectMessage
        {
            uint requestId = NextId();
            log.Debug?.Log($"RequestObjectData<{typeof(T).FullName}>(): RequestId {requestId}, DefinitionId {definitionId}");
            uint flags = 0;
            if (useBlockingDispatch) flags |= blockingDispatch;

            return RegisterResultObserver<T>(requestId, CsRequestDataOnSimObject(simConnect.handle, requestId, definitionId, objectId, (uint)ObjectDataPeriod.Once, flags, 0, 0, 0), "RequestObjectData");
        }

        public MessageStream<T> RequestObjectData<T>(AnnotatedObjectDefinition objectDefinition, ObjectDataPeriod period, uint objectId = SimObjectUser,
                                                     uint origin = 0, uint interval = 0, uint limit = 0,
                                                     bool onlyWhenChanged = false, bool useBlockingDispatch = false)
            where T : SimConnectMessage
        {
            uint requestId = NextId();
            log.Debug?.Log("RequestObjectData<{0}>(): RequestId {1}, target object type {2}, period = {3}, onlyWhenChanged = {4}, useBlockingDispatch = {5}", 
                typeof(T).FullName, requestId, objectDefinition.Type.FullName, period.ToString(), onlyWhenChanged, useBlockingDispatch);
            uint flags = 0;
            if (onlyWhenChanged) flags |= whenChanged;
            if (useBlockingDispatch) flags |= blockingDispatch;

            return RegisterStreamObserver<T>(requestId, CsRequestDataOnSimObject(simConnect.handle, requestId, objectDefinition.DefinitionId, objectId, (uint)period, flags, origin, interval, limit), "RequestObjectData");
        }

        public MessageStream<T> RequestObjectData<T>(SimObjectData objectDefinition, ObjectDataPeriod period, uint objectId = SimObjectUser,
                                                     uint origin = 0, uint interval = 0, uint limit = 0,
                                                     bool onlyWhenChanged = false, bool useBlockingDispatch = false)
            where T : SimConnectMessage
        {
            uint requestId = NextId();
            log.Debug?.Log("RequestObjectData<{0}>(): RequestId {1}, period = {2}, onlyWhenChanged = {3}, useBlockingDispatch = {4}",
                typeof(T).FullName, requestId, period.ToString(), onlyWhenChanged, useBlockingDispatch);
            uint flags = 0;
            if (onlyWhenChanged) flags |= whenChanged;
            if (useBlockingDispatch) flags |= blockingDispatch;

            return RegisterStreamObserver<T>(requestId, CsRequestDataOnSimObject(simConnect.handle, requestId, objectDefinition.DefinitionId, objectId, (uint)period, flags, origin, interval, limit), "RequestObjectData");
        }

        public MessageStream<T> RequestDataOnSimObjectType<T>(AnnotatedObjectDefinition objectDefinition, ObjectType objectType, uint radiusInMeters)
            where T : SimConnectMessage
        {
            uint requestId = NextId();
            log.Debug?.Log("RequestDataOnSimObjectType<{0}>(): RequestId {1}, radius {2}, target object type = {3}",
                typeof(T).FullName, requestId, radiusInMeters, objectType.ToString());

            return RegisterStreamObserver<T>(requestId, CsRequestDataOnSimObjectType(simConnect.handle, requestId, objectDefinition.DefinitionId, radiusInMeters, (uint)objectType), "RequestDataOnSimObjectType");
        }

        public MessageResult<AssignedObjectId> CreateNonATCAircraft(SimulatedAircraft aircraft)
        {
            uint requestId = NextId();
            log.Debug?.Log("CreateNonAircraft(): RequestId {0}", requestId);

            LatLonAlt pos = new() { Latitude = aircraft.Latitude, Longitude = aircraft.Longitude, Altitude = aircraft.Altitude };
            PBH pbh = new() { Pitch = aircraft.Pitch, Bank = aircraft.Bank, Heading = aircraft.Heading };

            return RegisterResultObserver<AssignedObjectId>(requestId, CsAICreateNonATCAircraft(simConnect.handle, aircraft.Title, aircraft.TailNumber, ref pos, ref pbh, (uint)(aircraft.OnGround ? 1 : 0), (uint)aircraft.AirSpeed, requestId), "CreateParkedAircraft");
        }
    }
}
