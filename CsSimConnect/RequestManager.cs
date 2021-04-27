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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public class RequestManager
    {
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern Int64 CsRequestSystemState(IntPtr handle, UInt32 requestId, [MarshalAs(UnmanagedType.LPStr)] string state);

        private static readonly Logger log = Logger.GetLogger(typeof(RequestManager));

        private static readonly Lazy<RequestManager> lazyInstance = new(() => new RequestManager(SimConnect.Instance));

        public static RequestManager Instance { get { return lazyInstance.Value; } }

        private SimConnect simConnect;

        private RequestManager(SimConnect simConnect)
        {
            this.simConnect = simConnect;
        }

        private UInt32 nextRequest = 0;

        public UInt32 NextRequest()
        {
            return Interlocked.Increment(ref nextRequest);
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

        private MessageDispatcher dispatcher = new("RequestID");

        public void DispatchResult(UInt32 requestId, SimConnectMessage msg)
        {
            if (!dispatcher.DispatchToObserver(requestId, msg))
            {
                log.Error("Received a message for an unknown request {0}.", requestId);
            }
        }

        public MessageResult<SystemState> RequestSystemState(string systemState)
        {
            uint requestId = NextRequest();
            log.Debug("Request ID {0}: Requesting '{1}'", requestId, systemState);

            Int64 sendId = CsRequestSystemState(simConnect.handle, requestId, systemState);
            MessageResult<SystemState> result;
            if (sendId > 0)
            {
                result = new MessageResult<SystemState>((uint)sendId);
                dispatcher.AddObserver(requestId, result);
            }
            else
            {
                result = (MessageResult<SystemState>)SimConnectObserver.ErrorResult(0, new SimConnectException(1, 0));
            }
            return result;
        }

        public void RequestSystemStateBool(string systemState, Action<bool> callback)
        {
            RequestSystemState(systemState).Subscribe((SystemState systemState) => callback(systemState.AsBoolean()));
        }
        public void RequestSystemStateString(string systemState, Action<string> callback)
        {
            RequestSystemState(systemState).Subscribe((SystemState systemState) => callback(systemState.StringValue));
        }

        public void RequestDialogMode(Action<bool> callback)
        {
            RequestSystemStateBool("DialogMode", callback);
        }
        public void RequestFullScreenMode(Action<bool> callback)
        {
            RequestSystemStateBool("FullScreenMode", callback);
        }
        public void RequestSimState(Action<bool> callback)
        {
            RequestSystemStateBool("Sim", callback);
        }
        public void RequestAircraftLoaded(Action<string> callback)
        {
            RequestSystemStateString("AircraftLoaded", callback);
        }
        public void RequestFlightLoaded(Action<string> callback)
        {
            RequestSystemStateString("FlightLoaded", callback);
        }
        public void RequestFlightPlanLoaded(Action<string> callback)
        {
            RequestSystemStateString("FlightPlan", callback); // Inconsistent, SimConnect uses "FlightPlanLoaded" for the event.
        }

    }
}
