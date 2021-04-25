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

    public sealed class MessageDispatcher
    {

        [DllImport("CsSimConnectInterOp.dll")]
        public static extern bool CsCallDispatch(IntPtr handle, DispatchProc dispatchProc);
        [DllImport("CsSimConnectInterOp.dll")]
        public static extern bool CsGetNextDispatch(IntPtr handle, DispatchProc dispatchProc);

        private static readonly Logger log = Logger.GetLogger(typeof(MessageDispatcher));

        private static readonly Lazy<MessageDispatcher> lazyInstance = new Lazy<MessageDispatcher>(() => new MessageDispatcher(SimConnect.Instance));

        public static MessageDispatcher Instance { get { return lazyInstance.Value; } }

        public delegate void DispatchProc(ref ReceiveStruct structData, Int32 wordData, IntPtr context);

        private readonly SimConnect simConnect;

        public MessageDispatcher(SimConnect simConnect)
        {
            this.simConnect = simConnect;
        }

        private Task<bool> messagePoller;

        public void Init()
        {
            CsCallDispatch(simConnect.handle, HandleMessage);
            messagePoller = new(() =>
            {
                while (simConnect.IsConnected())
                {
                    while (CsGetNextDispatch(simConnect.handle, HandleMessage))
                    {
                        log.Trace("Trying for another message");
                    }
                    Thread.Sleep(100);
                }
                return true;
            });
            messagePoller.Start();
        }

        private ConcurrentDictionary<UInt32, SimConnectObserver> Messages = new();

        private void HandleMessage(ref ReceiveStruct structData, Int32 wordData, IntPtr context)
        {
            if (structData.Id > (int)RecvId.SIMCONNECT_RECV_ID_EVENT_FILENAME_W)
            {
                log.Error("Received message with Message ID {0}", structData.Id);
                return;
            }
            log.Debug("Received message with ID {0}", structData.Id);
            try
            {
                switch ((RecvId)structData.Id)
                {
                    case RecvId.SIMCONNECT_RECV_ID_EXCEPTION:           // 1
                        SimConnectException exc = new(structData.Exception.ExceptionId, structData.Exception.SendId, structData.Exception.Index);
                        log.Debug("Exception returned: {0} (SendID={1}, Index={2})", exc.Message, exc.SendID, exc.Index);
                        SimConnectObserver msg;
                        if (Messages.Remove(exc.SendID, out msg))
                        {
                            msg.OnError(exc);
                        }
                        else
                        {
                            log.Warn("Ignoring exception for unknown SendID {0}: {1} (index={2})", exc.SendID, exc.Message, exc.Index);
                        }
                        break;

                    case RecvId.SIMCONNECT_RECV_ID_OPEN:                // 2
                        unsafe
                        {
                            fixed (ReceiveAppInfo* r = &structData.ConnectionInfo)
                            {
                                simConnect.SimName = Encoding.Latin1.GetString(r->ApplicationName, 256).Trim();
                            }
                        }
                        log.Info("We are connected to '{0}'.", simConnect.SimName);
                        simConnect.ApplicationVersionMajor = structData.ConnectionInfo.ApplicationVersionMajor;
                        simConnect.ApplicationVersionMinor = structData.ConnectionInfo.ApplicationVersionMinor;
                        simConnect.ApplicationBuildMajor = structData.ConnectionInfo.ApplicationBuildMajor;
                        simConnect.ApplicationBuildMinor = structData.ConnectionInfo.ApplicationBuildMinor;
                        simConnect.SimConnectVersionMajor = structData.ConnectionInfo.SimConnectVersionMajor;
                        simConnect.SimConnectVersionMinor = structData.ConnectionInfo.SimConnectVersionMinor;
                        simConnect.SimConnectBuildMajor = structData.ConnectionInfo.SimConnectBuildMajor;
                        simConnect.SimConnectBuildMinor = structData.ConnectionInfo.SimConnectBuildMinor;
                        simConnect.InvokeConnectionStateChanged();
                        break;

                    case RecvId.SIMCONNECT_RECV_ID_QUIT:                    // 3
                        log.Info("We are disconnected from '{0}'.", simConnect.SimName);
                        simConnect.Disconnect();
                        break;

                    case RecvId.SIMCONNECT_RECV_ID_EVENT:                   // 4
                        log.Debug("Received event {0}.", structData.Event.Id);
                        EventManager.Instance.DispatchResult(structData.Event.Id, ref structData);
                        break;

                    case RecvId.SIMCONNECT_RECV_ID_EVENT_64:                // 67
                        log.Debug("Received 64-bit event {0}.", structData.Event.Id);
                        EventManager.Instance.DispatchResult(structData.Event.Id, ref structData);
                        break;

                    case RecvId.SIMCONNECT_RECV_ID_SYSTEM_STATE:            // 15
                        log.Debug("Received systemState for request {0}.", structData.SystemState.RequestId);
                        RequestManager.Instance.DispatchResult(structData.SystemState.RequestId, ref structData);
                        break;

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                log.Error("Exception caught while processing message: {0}", e.Message);
            }
        }
    }
}
