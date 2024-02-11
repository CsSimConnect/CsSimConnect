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
using CsSimConnect.Events;
using CsSimConnect.Exc;
using CsSimConnect.Reactive;
using Rakis.Logging;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;

namespace CsSimConnect
{

    public enum SystemEvent
    {
        // Simulator state
        Sim,               // which means both of:
        SimStart,
        SimStop,

        Pause,           // which means both of:
        Paused,
        Unpaused,

        Crashed,
        CrashReset,

        // Sytem events for timed recurrences
        Each1Sec,
        Each4Sec,
        Freq6Hz,

        // Generic recurring events
        Frame,
        PauseFrame,
        PositionChanged,

        // Flights, plans, and aircraft
        FlightLoaded,
        FlightSaved,

        FlightPlanLoaded,
        FlightPlanDeactivated,

        AircraftLoaded,

        // Application UI changes
        Sound,
        View,

        TextEventCreated,
        TextEventDestroyed,

        // Recording and Playback
        PlaybackStateChanged,
        RecorderStateChanged,

        // Weather events
        WeatherModeChanged,

        // AI events
        ObjectAdded,
        ObjectRemoved,

        // Mission events
        RaceEnd,
        RaceLap,

        MissionCompleted,
        CustomMissionActionExecuted,
        FlightSegmentReadyForGrading,

        // Weapons related events
        WeaponFired,
        WeaponDetonated,
        CountermeasureFired,
        ObjectDamagedByWeapon,

        // Multiplayer events
        MultiplayerClientStarted,
        MultiplayerServerStarted,
        MultiplayerSessionEnded,
    }

    public sealed class EventManager : MessageManager
    {
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsSubscribeToSystemEvent(IntPtr handle, UInt32 requestId, [MarshalAs(UnmanagedType.LPStr)] string eventName);

        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsAddClientEventToNotificationGroup(IntPtr handle, UInt32 groupId, UInt32 eventId, UInt32 maskable);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsMapClientEventToSimEvent(IntPtr handle, UInt32 eventId, [MarshalAs(UnmanagedType.LPStr)] string eventName);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsMapInputEventToClientEvent(IntPtr handle, UInt32 groupId, [MarshalAs(UnmanagedType.LPStr)] string inputDefinition, UInt32 downEventId, UInt32 downValue, UInt32 upEventId, UInt32 upValue, UInt32 maskable);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsRemoveClientEvent(IntPtr handle, UInt32 groupId, UInt32 eventId);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsTransmitClientEvent(IntPtr handle, UInt32 objectId, UInt32 eventId, UInt32 data, UInt32 groupId, UInt32 flags);

        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsClearNotificationGroup(IntPtr handle, UInt32 groupId);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsRequestNotificationGroup(IntPtr handle, UInt32 groupId);
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsSetNotificationGroupPriority(IntPtr handle, UInt32 groupId, UInt32 priority);

        private static readonly ILogger log = Logger.GetLogger(typeof(EventManager));

        private static readonly Lazy<EventManager> lazyInstance = new(() => new EventManager(SimConnect.Instance));

        public static EventManager Instance { get { return lazyInstance.Value; } }

        public static string ToString(SystemEvent systemEvent) {
            string result = systemEvent.ToString();
            if (result.StartsWith("Each") || result.StartsWith("Freq"))
            {
                return result.Substring(4);
            }
            return result;
        }

        private static readonly UInt32 USREVT_FIRST = 64;

        private uint nextGroupId = 1;

        private EventManager(SimConnect simConnect) : base("EventID", USREVT_FIRST, simConnect)
        {
        }

        public uint NextGroupId()
        {
            return Interlocked.Increment(ref nextGroupId);
        }

        public MessageStream<T> SubscribeToSystemEvent<T>(SystemEvent systemEvent)
            where T : SimConnectMessage
        {
            uint eventId = NextId();
            log.Debug?.Log("Event ID {0}: Subscribing to '{1}'", eventId, systemEvent.ToString());

            return RegisterStreamObserver<T>(eventId, CsSubscribeToSystemEvent(simConnect.handle, eventId, systemEvent.ToString()), "SubscribeToSystemEvent");
        }

        public void SubscribeToSystemEventBool(SystemEvent systemEvent, Action<bool> callback)
        {
            SubscribeToSystemEvent<SimEvent>(systemEvent).Subscribe((SimEvent evt) => callback(evt.Data != 0));
        }

        public MessageStream<SimulatedObject> SubscribeToObjectAddedEvent()
        {
            uint eventId = NextId();
            log.Debug?.Log("Event ID {0}: Subscribing to ObjectAdded events", eventId);

            MessageStream<SimulatedObject> result = new(1);

            RegisterStreamObserver<ObjectAddedRemoved>(eventId, CsSubscribeToSystemEvent(simConnect.handle, eventId, SystemEvent.ObjectAdded.ToString()), "SubscribeToObjectAddedEvent")
                .Subscribe(objMsg => result.OnNext(objMsg.Type == ObjectType.Aircraft ? new SimulatedAircraft(objectId: objMsg.ObjectId) : new SimulatedObject(objMsg.Type, objectId: objMsg.ObjectId)),
                           e => result.OnError(e));
            return result;
        }

        public MessageStream<SimulatedObject> SubscribeToObjectRemovedEvent()
        {
            uint eventId = NextId();
            log.Debug?.Log("Event ID {0}: Subscribing to ObjectRemoved events", eventId);

            MessageStream<SimulatedObject> result = new(1);

            RegisterStreamObserver<ObjectAddedRemoved>(eventId, CsSubscribeToSystemEvent(simConnect.handle, eventId, SystemEvent.ObjectRemoved.ToString()), "SubscribeToObjectRemovedEvent")
                .Subscribe(objMsg => result.OnNext(objMsg.Type == ObjectType.Aircraft ? new SimulatedAircraft(objectId: objMsg.ObjectId) : new SimulatedObject(objMsg.Type, objectId: objMsg.ObjectId)),
                           e => result.OnError(e));
            return result;
        }

        private static readonly ConcurrentDictionary<string, ClientEvent> clientEvents = new();

        public static ClientEvent GetEvent(string eventName)
        {
            return ((eventName == null) || (eventName == "")) ? null : clientEvents.GetOrAdd(eventName, name => new(Instance, name));
        }

        private EventGroup defaultGroup;
        private EventGroup GetDefaultGroup()
        {
            if (defaultGroup == null)
            {
                defaultGroup = new("Default EventGroup", EventGroup.PriorityHighest);
            }
            return defaultGroup;
        }

        public MessageStream<EventData> SubscribeToEvent(ClientEvent clientEvent)
        {
            MessageStream<EventData> result = new(1);
            RegisterStreamObserver<SimEvent>(clientEvent.Id, 0, "SubscribeToEvent").Subscribe(
                simEvent => result.OnNext(new EventData(clientEvent, simEvent.Data)), result.OnError, result.OnCompleted);

            if (!clientEvent.IsMapped)
            {
                RegisterCleanup(CsMapClientEventToSimEvent(simConnect.handle, clientEvent.Id, clientEvent.MappedEvent), "MapClientEventToSimEvent", result.OnError);
                clientEvent.IsMapped = true;
            }
            if (clientEvent.Group == null)
            {
                clientEvent.Group = defaultGroup;
                RegisterCleanup(CsAddClientEventToNotificationGroup(simConnect.handle, defaultGroup.Id, clientEvent.Id, 0), "AddClientEventToNotificationGroup", result.OnError);
            }
            RegisterCleanup(CsSetNotificationGroupPriority(simConnect.handle, clientEvent.Group.Id, clientEvent.Group.Priority), "SetNotificationGroupPriority", result.OnError);

            return result;
        }

        public void SendEvent(ClientEvent clientEvent, uint objectId =0, uint data =0, Action<SimConnectException> onError =null)
        {
            if (!clientEvent.IsMapped)
            {
                RegisterCleanup(CsMapClientEventToSimEvent(simConnect.handle, clientEvent.Id, clientEvent.MappedEvent), "MapClientEventToSimEvent", onError);
                clientEvent.IsMapped = true;
            }
            if (clientEvent.Group == null)
            {
                clientEvent.Group = GetDefaultGroup();
                RegisterCleanup(CsAddClientEventToNotificationGroup(simConnect.handle, defaultGroup.Id, clientEvent.Id, 0), "AddClientEventToNotificationGroup", onError);
            }
            RegisterCleanup(CsTransmitClientEvent(simConnect.handle, objectId, clientEvent.Id, data, clientEvent.Group.Id, 0), "TransmitClientEvent", onError);
        }

        public void SendEventSigned(ClientEvent clientEvent, uint objectId = 0, int data = 0, Action<SimConnectException> onError = null)
        {
            SendEvent(clientEvent, objectId, (uint)data, onError);
        }

        public void SendEvent(ClientEvent clientEvent, SimulatedObject obj, uint data = 0, Action<SimConnectException> onError = null)
        {
            if (!clientEvent.IsMapped)
            {
                RegisterCleanup(CsMapClientEventToSimEvent(simConnect.handle, clientEvent.Id, clientEvent.MappedEvent), "MapClientEventToSimEvent", onError);
                clientEvent.IsMapped = true;
            }
            if (clientEvent.Group == null)
            {
                clientEvent.Group = GetDefaultGroup();
                RegisterCleanup(CsAddClientEventToNotificationGroup(simConnect.handle, defaultGroup.Id, clientEvent.Id, 0), "AddClientEventToNotificationGroup", onError);
            }
            RegisterCleanup(CsTransmitClientEvent(simConnect.handle, obj.ObjectId, clientEvent.Id, data, clientEvent.Group.Id, 0), "TransmitClientEvent", onError);
        }
    }
}
