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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CsSimConnect
{

    public sealed class SimConnect
    {

        private static readonly Logger log = Logger.GetLogger(typeof(SimConnect));

        private static readonly Lazy<SimConnect> lazyInstance = new Lazy<SimConnect>(() => new SimConnect());

        public static SimConnect Instance {  get { return lazyInstance.Value; } }

        public delegate void ConnectionStateHandler(bool willAutoConnect, bool isConnected);
        private delegate void DispatchProc(ref ReceiveStruct structData, Int32 wordData, IntPtr context);

        [DllImport("CsSimConnectInterOp.dll")]
        private static extern bool CsConnect([MarshalAs(UnmanagedType.LPStr)] string appName, ref IntPtr handle);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern bool CsDisconnect(IntPtr handle);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern bool CsCallDispatch(IntPtr handle, DispatchProc dispatchProc);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern bool CsGetNextDispatch(IntPtr handle, DispatchProc dispatchProc);

        internal IntPtr handle = IntPtr.Zero;
        public bool UseAutoConnect { get; set; }
        private Task<bool> messagePoller;

        private readonly Dictionary<uint, Action<Exception>> onError = new();
        public event ConnectionStateHandler OnConnectionStateChange;

        private SimConnect()
        {
            UseAutoConnect = false;
            Info = new("CsSimConnect");
        }

        public void InitDispatcher()
        {
            CsCallDispatch(handle, HandleMessage);
            messagePoller = new(() =>
            {
                while (IsConnected())
                {
                    while (CsGetNextDispatch(handle, HandleMessage))
                    {
                        log.Trace("Trying for another message");
                    }
                    Task.Delay(100);
                }
                return true;
            });
            messagePoller.Start();
        }

        public bool IsConnected()
        {
            return handle != IntPtr.Zero;
        }

        internal void InvokeConnectionStateChanged()
        {
            OnConnectionStateChange(UseAutoConnect, IsConnected());
        }

        public void Connect()
        {
            if (CsConnect("test", ref handle))
            {
                InitDispatcher();
            }
            InvokeConnectionStateChanged();
        }

        public void Disconnect()
        {
            IntPtr oldHandle = handle;
            handle = IntPtr.Zero;
            CsDisconnect(oldHandle);
            InvokeConnectionStateChanged();
        }

        internal void MessageCompleted(uint sendId)
        {
            onError.Remove(sendId);
        }

        private void HandleMessage(ref ReceiveStruct structData, Int32 wordData, IntPtr context)
        {
            if (structData.Id > (int)RecvId.EventFilenameW)
            {
                log.Error("Received message with Message ID {0}", structData.Id);
                return;
            }
            log.Debug("Received message with ID {0}", structData.Id);
            try
            {
                switch ((RecvId)structData.Id)
                {
                    case RecvId.Exception:           // 1
                        SimConnectException exc = new(structData.Exception.ExceptionId, structData.Exception.SendId, structData.Exception.Index);
                        log.Debug("Exception returned: {0} (SendID={1}, Index={2})", exc.Message, exc.SendID, exc.Index);
                        Action<Exception> cleanup;
                        if (onError.Remove(exc.SendID, out cleanup))
                        {
                            cleanup(exc);
                        }
                        else
                        {
                            log.Warn("Ignoring exception for unknown SendID {0}: {1} (index={2})", exc.SendID, exc.Message, exc.Index);
                        }
                        break;

                    case RecvId.Open:                // 2
                        Info = new(ref structData);
                        InvokeConnectionStateChanged();
                        break;

                    case RecvId.Quit:                    // 3
                        log.Info("We are disconnected from '{0}'.", Info.Name);
                        Disconnect();
                        break;

                    case RecvId.Event:                   // 4
                        log.Debug("Received event {0}.", structData.Event.Id);
                        EventManager.Instance.DispatchResult(structData.Event.Id, SimConnectMessage.FromMessage(ref structData));
                        break;

                    case RecvId.Event64:                // 67
                        log.Debug("Received 64-bit event {0}.", structData.Event.Id);
                        EventManager.Instance.DispatchResult(structData.Event.Id, SimConnectMessage.FromMessage(ref structData));
                        break;

                    case RecvId.SystemState:            // 15
                        log.Debug("Received systemState for request {0}.", structData.SystemState.RequestId);
                        RequestManager.Instance.DispatchResult(structData.SystemState.RequestId, SimConnectMessage.FromMessage(ref structData));
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

        public AppInfo Info { get; private set; }

    }
}
