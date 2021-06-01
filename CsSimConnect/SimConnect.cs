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
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CsSimConnect
{

    public enum FlightSimType
    {
        Unknown,
        Prepar3Dv4,
        Prepar3Dv5,
        MSFS2020
    }

    public sealed class SimConnect
    {

        private static readonly Logger log = Logger.GetLogger(typeof(SimConnect));

        public static FlightSimType InterOpType { get; set; }

        private static readonly Lazy<SimConnect> lazyInstance = new (() => new SimConnect());

        public static SimConnect Instance {  get { return lazyInstance.Value; } }

        public delegate void ConnectionStateHandler(bool willAutoConnect, bool isConnected);
        private delegate void DispatchProc(ref ReceiveStruct structData, UInt32 wordData, IntPtr context);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);

        private static void LoadInterOpLibrary(string path)
        {
            try
            {
                var hExe = LoadLibrary(path);
                if (hExe == IntPtr.Zero)
                {
                    log.Fatal?.Log("Unable to load '{0}'", path);
                }
            }
            catch (Exception e) {
                log.Error?.Log("Exception caught in LoadInterOpLibrary('{0}'): {1}", path, e.Message);
            }
        }

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

        public event Action OnConnect;
        public event Action<bool> OnDisconnect;
        public event ConnectionStateHandler OnConnectionStateChange;

        private static Dictionary<string, FlightSimType> simulatorTypes = null;
        public AppInfo Info { get; private set; }
        public FlightSimType ConnectedSim { get; private set; }
        public event Action<AppInfo> OnOpen;
        public event Action OnClose;

        private SimConnect()
        {
            log.Info?.Log("Loading InterOp DLL for '{0}'.", InterOpType.ToString());
            if (InterOpType == FlightSimType.Unknown)
            {
                log.Fatal?.Log("Target InterOp type not set!");
            }
            else if (InterOpType == FlightSimType.Prepar3Dv4)
            {
                LoadInterOpLibrary("P3Dv4\\CsSimConnectInterOp.dll");
            }
            else if (InterOpType == FlightSimType.Prepar3Dv5)
            {
                LoadInterOpLibrary("P3Dv5\\CsSimConnectInterOp.dll");
            }
            else if (InterOpType == FlightSimType.MSFS2020)
            {
                LoadInterOpLibrary("MSFS\\CsSimConnectInterOp.dll");
            }
            else
            {
                log.Fatal?.Log("Unknown FlightSimType '{0}'", InterOpType.ToString());
            }

            UseAutoConnect = false;
            Info = new("CsSimConnect");
            OnOpen += SetConnectedSim;
            OnConnect += InvokeConnectionStateChanged;
            OnDisconnect += ResetErrorCallbacks;
            OnDisconnect += _ => InvokeConnectionStateChanged();
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
                        log.Trace?.Log("Trying for another message");
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
                OnConnect?.Invoke();
            }
            else
            {
                log.Error?.Log("Failed to connect.");
            }
        }

        private void SetConnectedSim(AppInfo info)
        {
            Info = info;
            if (simulatorTypes == null)
            {
                var text = File.ReadAllText("SimulatorTypes.json");
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = {
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                        }
                };
                simulatorTypes = JsonSerializer.Deserialize<Dictionary<string, FlightSimType>>(text, options);
            }
            if (!simulatorTypes.ContainsKey(info.Name))
            {
                log.Error?.Log("Connected to unknown simulator type '{0}'", info.Name);
            }
            ConnectedSim = simulatorTypes.GetValueOrDefault(info.Name, FlightSimType.Unknown);
        }

        public void Disconnect(bool connectionLost =false)
        {
            IntPtr oldHandle = handle;
            handle = IntPtr.Zero;
            if (CsDisconnect(oldHandle))
            {
                OnDisconnect?.Invoke(connectionLost);
            }
            else
            {
                log.Error?.Log("Failed to disconnect from simulator");
            }
        }

        private void ResetErrorCallbacks(bool connectionLost)
        {
            log.Info?.Log("Clearing Error callbacks");
            lock (this)
            {
                onError.Clear();
            }
        }

        internal void MessageCompleted(uint sendId)
        {
            if (onError.Remove(sendId))
            {
                log.Trace?.Log("Removed SendID {0}.");
            }
            else
            {
                log.Trace?.Log("SendID {0} already removed.");
            }
        }

        internal void AddCleanup(uint sendId, Action<SimConnectException> cleanup)
        {
            log.Trace?.Log("Adding cleanup for SendId {0}", sendId);
            onError.Add(sendId, cleanup);
        }

        private void HandleMessage(ref ReceiveStruct structData, UInt32 structLen, IntPtr context)
        {
            if (structData.Id > (int)RecvId.EventFilenameW)
            {
                log.Error?.Log("Received message with Message ID {0}", structData.Id);
                return;
            }
            log.Debug?.Log("Received message with ID {0}", structData.Id);
            try
            {
                Action followup = null;

                switch ((RecvId)structData.Id)
                {
                    case RecvId.Exception:           // 1
                        SimConnectException exc = new(structData.Exception.ExceptionId, structData.Exception.SendId, structData.Exception.Index);
                        followup = () => {
                            log.Trace?.Log("Exception returned: {0} (SendID={1}, Index={2})", exc.Message, exc.SendID, exc.Index);
                            if (onError.Remove(exc.SendID.Value, out Action<SimConnectException> cleanup))
                            {
                                cleanup(exc);
                            }
                            else
                            {
                                log.Warn?.Log("Ignoring exception for unknown SendID {0}: {1} (index={2})", exc.SendID, exc.Message, exc.Index);
                            }
                        };
                        break;

                    case RecvId.Open:                // 2
                        OnOpen?.Invoke(new(ref structData));
                        break;

                    case RecvId.Quit:                    // 3
                        log.Info?.Log("We are disconnected from '{0}'.", Info.Name);
                        OnClose?.Invoke();
                        Disconnect();
                        break;

                    case RecvId.Event:                   // 4
                        log.Debug?.Log("Received event {0}.", structData.Event.Id);
                        EventManager.Instance.DispatchResult(structData.Event.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.ObjectAddRemove:       // 5
                        log.Trace?.Log("Received event {0} for add/remove of simulation object of type {1}", structData.Event.Id, ((ObjectType)structData.Event.ObjectType).ToString());
                        EventManager.Instance.DispatchResult(structData.Event.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.SimObjectData:          // 8
                        log.Trace?.Log("Received SimObjectData for request {0}", structData.ObjectData.Id);
                        RequestManager.Instance.DispatchResult(structData.ObjectData.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.SimObjectDataByType:          // 9
                        log.Trace?.Log("Received SimObjectDataByType for request {0}", structData.ObjectData.Id);
                        RequestManager.Instance.DispatchResult(structData.ObjectData.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.AssignedObjectId:       // 12
                        log.Trace?.Log("Received ObjectID {0} for AI creation request {1}", structData.AssignedObjectId.ObjectId, structData.AssignedObjectId.RequestId);
                        RequestManager.Instance.DispatchResult(structData.AssignedObjectId.RequestId, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.Event64:                // 67
                        log.Debug?.Log("Received 64-bit event {0}.", structData.Event.Id);
                        EventManager.Instance.DispatchResult(structData.Event.Id, SimConnectMessage.FromMessage(ref structData, structLen));
                        break;

                    case RecvId.SystemState:            // 15
                        log.Debug?.Log("Received systemState for request {0}.", structData.SystemState.Id);
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
                log.Error?.Log("Exception caught while processing message: {0}\n{1}", e.Message, e.StackTrace);
            }
        }

    }
}
