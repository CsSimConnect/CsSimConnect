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

        private static readonly UInt32 USREVT_FIRST = 64;

        private EventManager(SimConnect simConnect) : base("EventID", USREVT_FIRST, simConnect)
        {
        }

        public MessageStream<T> SubscribeToSystemEvent<T>(SystemEvent systemEvent)
            where T : SimConnectMessage
        {
            uint eventId = NextId();
            log.Debug("Event ID {0}: Subscribing to '{1}'", eventId, systemEvent.ToString());

            return RegisterStreamObserver<T>(eventId, CsSubscribeToSystemEvent(simConnect.handle, eventId, systemEvent.ToString()), "SubscribeToSystemEvent");
        }

        public void SubscribeToSystemEventBool(SystemEvent systemEvent, Action<bool> callback)
        {
            SubscribeToSystemEvent<SimEvent>(systemEvent).Subscribe((SimEvent evt) => callback(evt.Data != 0));
        }
    }
}
