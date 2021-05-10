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
using System.Threading.Tasks;

namespace CsSimConnect
{

    public sealed class SimConnect
    {

        private static readonly Logger log = Logger.GetLogger(typeof(SimConnect));

        private static readonly Lazy<SimConnect> lazyInstance = new (() => new SimConnect());

        public static SimConnect Instance {  get { return lazyInstance.Value; } }

        public delegate void ConnectionStateHandler(bool willAutoConnect, bool isConnected);
        private delegate void DispatchProc(ref ReceiveStruct structData, UInt32 wordData, IntPtr context);

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

        private readonly Dictionary<uint, Action<SimConnectException>> onError = new();
        public event ConnectionStateHandler OnConnectionStateChange;
        public event Action OnConnect;
        public event Action OnDisconnect;

        private SimConnect()
        {
            UseAutoConnect = false;
            Info = new("CsSimConnect");
            OnDisconnect += ResetErrorCallbacks;
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
            if (CsConnect("CsSimConnect", ref handle))
            {
                InitDispatcher();
                if (OnConnect != null)
                {
                    OnConnect.Invoke();
                }
            }
            else
            {
                log.Error("Failed to connect.");
            }
            InvokeConnectionStateChanged();
        }

        public void Disconnect()
        {
            IntPtr oldHandle = handle;
            handle = IntPtr.Zero;
            if (CsDisconnect(oldHandle))
            {
                OnDisconnect.Invoke();
            }
            else
            {
                log.Error("Failed to disconnect from simulator");
            }
            InvokeConnectionStateChanged();
        }

        private void ResetErrorCallbacks()
        {
            log.Info("Clearing Error callbacks");
            lock (this)
            {
                onError.Clear();
            }
        }

        internal void MessageCompleted(uint sendId)
        {
            if (onError.Remove(sendId))
            {
                log.Trace("Removed SendID {0}.");
            }
            else
            {
                log.Trace("SendID {0} already removed.");
            }
        }

        internal void AddCleanup(uint sendId, Action<SimConnectException> cleanup)
        {
            log.Trace("Adding cleanup for SendId {0}", sendId);
            onError.Add(sendId, cleanup);
        }

        private void HandleMessage(ref ReceiveStruct structData, UInt32 structLen, IntPtr context)
        {
            if (structData.Id > (int)RecvId.EventFilenameW)
            {
                log.Error("Received message with Message ID {0}", structData.Id);
                return;
            }
            log.Debug("Received message with ID {0}", structData.Id);
            try
            {
                Action followup = null;

                switch ((RecvId)structData.Id)
                {
                    case RecvId.Exception:           // 1
                        SimConnectException exc = new(structData.Exception.ExceptionId, structData.Exception.SendId, structData.Exception.Index);
                        followup = () => {
                            log.Trace("Exception returned: {0} (SendID={1}, Index={2})", exc.Message, exc.SendID, exc.Index);
                            if (onError.Remove(exc.SendID.Value, out Action<SimConnectException> cleanup))
                            {
                                cleanup(exc);
                            }
                            else
                            {
                                log.Warn("Ignoring exception for unknown SendID {0}: {1} (index={2})", exc.SendID, exc.Message, exc.Index);
                            }
                        };
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
                        EventManager.Instance.DispatchResult(structData.Event.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.SimObjectData:          // 8
                        log.Trace("Received SimObjectData for request {0}", structData.ObjectData.Id);
                        RequestManager.Instance.DispatchResult(structData.ObjectData.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.Event64:                // 67
                        log.Debug("Received 64-bit event {0}.", structData.Event.Id);
                        EventManager.Instance.DispatchResult(structData.Event.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.SystemState:            // 15
                        log.Debug("Received systemState for request {0}.", structData.SystemState.Id);
                        RequestManager.Instance.DispatchResult(structData.SystemState.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    default:
                        break;
                }
                if (followup != null)
                {
                    followup.Invoke();
                }
            }
            catch (Exception e)
            {
                log.Error("Exception caught while processing message: {0}\n{1}", e.Message, e.StackTrace);
            }
        }

        public AppInfo Info { get; private set; }

    }
}
