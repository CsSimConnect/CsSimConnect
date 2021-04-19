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
        private static extern bool CsRequestSystemState(IntPtr handle, UInt32 requestId, [MarshalAs(UnmanagedType.LPStr)] string state);

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
        private readonly ConcurrentDictionary<UInt32, RequestResultHandlerRegistration> resultHandlers = new();

        public void DispatchResult(UInt32 requestId, ref ReceiveStruct msg)
        {
            RequestResultHandlerRegistration handler;
            if (resultHandlers.TryGetValue(requestId, out handler))
            {
                handler.Handler.Invoke(ref msg);
                if (handler.UseOnceOnly)
                {
                    resultHandlers.TryRemove(requestId, out _);
                }
            }
            else
            {
                // Complain
            }
        }

        public void RequestSystemStateBool(string systemState, Action<bool> processResult)
        {
            UInt32 requestId = NextRequest();
            RequestResultHandlerRegistration registration = new((ref ReceiveStruct msg) =>
            {
                processResult(msg.SystemState.IntValue != 0);
            });
            resultHandlers.AddOrUpdate(requestId, registration, (_, _) => registration);
            if (!CsRequestSystemState(simConnect.handle, requestId, systemState))
            {
                //Complain
                resultHandlers.TryRemove(requestId, out _);
            }
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

        public void RequestSystemStateString(string systemState, Action<string> callback)
        {
            UInt32 requestId = NextRequest();
            RequestResultHandlerRegistration registration = new((ref ReceiveStruct msg) =>
            {
                string value;
                unsafe
                {
                    fixed (ReceiveSystemState* r = &msg.SystemState)
                    {
                        value = Encoding.Latin1.GetString(r->stringValue, 260).Trim();
                    }
                }
                callback(value);
            });
            resultHandlers.AddOrUpdate(requestId, registration, (_, _) => registration);
            if (!CsRequestSystemState(simConnect.handle, requestId, systemState))
            {
                //Complain
                resultHandlers.TryRemove(requestId, out _);
            }
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
