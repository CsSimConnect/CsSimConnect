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
using System.Runtime.InteropServices;
using System.Text;
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

    public sealed class EventManager
    {
        [DllImport("CsSimConnectInterOp.dll")]
        private static extern long CsSubscribeToSystemEvent(IntPtr handle, UInt32 requestId, [MarshalAs(UnmanagedType.LPStr)] string eventName);

        private static readonly Logger log = Logger.GetLogger(typeof(EventManager));

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


        private readonly SimConnect simConnect;
        private EventManager(SimConnect simConnect)
        {
            this.simConnect = simConnect;
        }

        private static readonly UInt32 USREVT_FIRST = 64;
        private UInt32 lastEvent = USREVT_FIRST;

        public UInt32 NextEvent()
        {
            return Interlocked.Increment(ref lastEvent);
        }

        private MessageDispatcher dispatcher = new("EventID");

        public void DispatchResult(UInt32 eventId, SimConnectMessage msg)
        {
            if (!dispatcher.DispatchToObserver(eventId, msg))
            {
                log.Error("Received a message for an unknown event {0}.", eventId);
            }
        }

        public MessageStream<T> SubscribeToSystemState<T>(string systemState)
            where T : SimConnectMessage
        {
            UInt32 eventId = NextEvent();
            log.Debug("Event ID {0}: Subscribing to '{1}'", eventId, systemState);

            long sendId = CsSubscribeToSystemEvent(simConnect.handle, eventId, systemState);
            MessageStream<T> result;
            if (sendId > 0)
            {
                result = new MessageStream<T>((uint)sendId, 1);
                dispatcher.AddObserver(eventId, result);
            }
            else
            {
                result = (MessageStream<T>)SimConnectObserver.ErrorResult(0, new SimConnectException(1, 0));
            }
            return result;
        }

        public void SubscribeToSystemStateBool(string systemState, Action<bool> callback, bool requestOnce = false)
        {
            SubscribeToSystemState<SimEvent>(systemState).Subscribe((SimEvent evt) => callback(evt.Data != 0));
            if (requestOnce)
            {
                RequestManager.Instance.RequestSystemStateBool(systemState, callback);
            }
        }
    }
}
