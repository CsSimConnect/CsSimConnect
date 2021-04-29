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
        private static extern Int64 CsRequestSystemState(IntPtr handle, UInt32 requestId, [MarshalAs(UnmanagedType.LPStr)] string state);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsRequestDataOnSimObject(IntPtr handle, UInt32 requestId, UInt32 defId, UInt32 objectId, UInt32 period, UInt32 dataRequestFlags, UInt32 origin, UInt32 interval, UInt32 limit);

        private static readonly Logger log = Logger.GetLogger(typeof(RequestManager));

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

        public SimConnectMessageResult<SimState> RequestSystemState(SystemState systemState)
        {
            uint requestId = NextId();
            log.Debug("Request ID {0}: Requesting '{1}'", requestId, systemState.ToString());

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

        private static readonly uint simObjectUser = 0;
        private static readonly uint whenChanged = 0x00000001;
        private static readonly uint taggedFormat = 0x00000002;
        private static readonly uint blockingDispatch = 0x00000004;

        public SimConnectMessageResult<T> RequestObjectData<T>(ObjectDefinition objectDefinition, ObjectDataPeriod period,
                                                     uint origin =0, uint interval =0, uint limit =0,
                                                     bool onlyWhenChanged=false, bool useBlockingDispatch =false)
            where T : SimConnectMessage
        {
            uint requestId = NextId();
            uint flags = 0;
            if (onlyWhenChanged) flags |= whenChanged;
            if (useBlockingDispatch) flags |= blockingDispatch;
            return RegisterResultObserver<T>(requestId, CsRequestDataOnSimObject(simConnect.handle, requestId, objectDefinition.DefinitionId, simObjectUser, (uint)period, flags, origin, interval, limit), "RequestObjectData");
        }
    }
}
