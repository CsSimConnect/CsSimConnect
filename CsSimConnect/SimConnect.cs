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

using CsSimConnect.Exc;
using Rakis.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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

        private static readonly Lazy<SimConnect> lazyInstance = new (() => new SimConnect());

        public static SimConnect Instance {  get { return lazyInstance.Value; } }

        public delegate void ConnectionStateHandler(bool willAutoConnect, bool isConnected);
        private delegate void DispatchProc(ref ReceiveStruct structData, UInt32 wordData, IntPtr context);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool FreeLibrary(IntPtr hModule);

        private static FlightSimType interOpType;
        public static FlightSimType InterOpType => interOpType;
        public static FlightSimType ConnectedSim { get; private set; }

        private static IntPtr interOpDll;

        private static void LoadInterOpLibrary(string path)
        {
            UnloadInterOpLibrary();
            try
            {
                interOpDll = LoadLibrary(path);
                if (interOpDll == IntPtr.Zero)
                {
                    log.Fatal?.Log("Unable to load '{0}'", path);
                }
            }
            catch (Exception e) {
                log.Error?.Log("Exception caught in LoadInterOpLibrary('{0}'): {1}", path, e.Message);
            }
        }

        private static void UnloadInterOpLibrary()
        {
            if (interOpDll != IntPtr.Zero)
            {
                try
                {
                    log.Info?.Log("Unloading InterOp DLL");
                    while (FreeLibrary(interOpDll))
                    {
                        log.Debug?.Log("Reduced link count of InterOp DLL by one");
                    }
                    interOpDll = IntPtr.Zero;
                }
                catch (Exception e)
                {
                    log.Error?.Log("Exception caught in UnloadInterOpLibrary('{0}'): {1}", e.Message);
                }
            }
        }

        public static void SetFlightSimType(FlightSimType type)
        {
            interOpType = type;

            log.Info?.Log("Loading InterOp DLL for '{0}'.", type.ToString());
            if (type == FlightSimType.Unknown)
            {
                log.Fatal?.Log("Target InterOp type not set!");
            }
            else if (type == FlightSimType.Prepar3Dv4)
            {
                LoadInterOpLibrary("P3Dv4\\CsSimConnectInterOp.dll");
            }
            else if (type == FlightSimType.Prepar3Dv5)
            {
                LoadInterOpLibrary("P3Dv5\\CsSimConnectInterOp.dll");
            }
            else if (type == FlightSimType.MSFS2020)
            {
                LoadInterOpLibrary("MSFS\\CsSimConnectInterOp.dll");
            }
            else
            {
                log.Fatal?.Log("Unknown FlightSimType '{0}'", type.ToString());
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
        public bool IsConnected => (handle != IntPtr.Zero);

        private Task autoConnecter;
        private bool useAutoConnect;
        public bool UseAutoConnect
        {
            get => useAutoConnect;
            set { useAutoConnect = value; if (useAutoConnect && !IsConnected) RunAutoConnect(); }
        }
        public TimeSpan AutoConnectRetryPeriod { get; set; }
        private EventWaitHandle disconnectEvent = new AutoResetEvent(false);

        private Task messagePoller;
        public TimeSpan MessagePollerRetryPeriod { get; set; }

        private readonly Dictionary<uint, Action<SimConnectException>> onError = new();

        public event Action OnConnect;
        public event Action<bool> OnDisconnect;
        public event ConnectionStateHandler OnConnectionStateChange;

        private static Dictionary<string, FlightSimType> simulatorTypes = null;
        public AppInfo Info { get; private set; }
        public event Action<AppInfo> OnOpen;
        public event Action OnClose;

        private SimConnect()
        {
            UseAutoConnect = false;
            AutoConnectRetryPeriod = TimeSpan.FromSeconds(5);
            MessagePollerRetryPeriod = TimeSpan.FromMilliseconds(100);

            Info = new("CsSimConnect");

            OnOpen += SetConnectedSim;
            OnClose += LogSimulatorShutdown;

            OnConnect += LogConnectedState;
            OnConnect += InvokeConnectionStateChanged;

            OnDisconnect += LogDisconnectedState;
            OnDisconnect += ResetErrorCallbacks;
            OnDisconnect += _ => InvokeConnectionStateChanged();
            OnDisconnect += _ => RunAutoConnect();
            OnDisconnect += _ => { disconnectEvent.Set(); };
        }

        private void RunAutoConnect()
        {
            if (autoConnecter != null)
            {
                log.Warn?.Log("Not starting a second autoconnector.");
                return;
            }
            messagePoller = null;
            autoConnecter = new(() =>
            {
                while (UseAutoConnect)
                {
                    while (!IsConnected)
                    {
                        log.Debug?.Log("Trying to AutoConnect");
                        if (!Connect())
                        {
                            Task.Delay(AutoConnectRetryPeriod).Wait();
                        }
                    }
                    disconnectEvent.WaitOne();
                }
            });
            autoConnecter.Start();
        }

        private void RunDispatcher()
        {
            autoConnecter = null;
            CsCallDispatch(handle, HandleMessage);
            messagePoller = new(() =>
            {
                while (IsConnected)
                {
                    while (CsGetNextDispatch(handle, HandleMessage))
                    {
                        log.Trace?.Log("Trying for another message");
                    }
                    Task.Delay(MessagePollerRetryPeriod).Wait();
                }
            });
            messagePoller.Start();
        }

        private void LogConnectedState()
        {
            log.Info?.Log("Connected to Simulator");
        }

        private void LogDisconnectedState(bool connectionLost)
        {
            string reason = connectionLost ? "Connection lost" : "Disconnect called";
            log.Info?.Log($"Disconnected from Simulator ({reason})");
        }

        private void LogSimulatorShutdown()
        {
            log.Info?.Log("Simulator shutting down");
        }

        internal void InvokeConnectionStateChanged()
        {
            OnConnectionStateChange?.Invoke(UseAutoConnect, IsConnected);
        }

        public bool Connect()
        {
            if (CsConnect("CsSimConnect", ref handle))
            {
                RunDispatcher();
                OnConnect?.Invoke();
            }
            else
            {
                log.Debug?.Log("Failed to connect.");
            }
            return IsConnected;
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
            ConnectedSim = simulatorTypes.GetValueOrDefault(info.Name, FlightSimType.Unknown);
            if (!simulatorTypes.ContainsKey(info.Name))
            {
                log.Error?.Log("Connected to unknown simulator type '{0}'", info.Name);
            }
            else
            {
                log.Info?.Log($"Connected to {info.Name} (type {ConnectedSim})");
            }
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
                                log.Trace?.Log("Calling cleanup for SendID {0}.", exc.SendID.Value);
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
