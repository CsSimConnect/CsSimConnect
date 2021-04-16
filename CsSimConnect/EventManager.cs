﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace CsSimConnect
{

    public enum SystemEvent
    {
        // Simulator state
        SIM,               // which means both of:
        SIM_START,
        SIM_STOP,

        PAUSE,           // which means both of:
        PAUSED,
        UNPAUSED,

        CRASHED,
        CRASH_RESET,

        // Sytem events for timed recurrences
        EACH_1SEC,
        EACH_4SEC,
        FREQ_6HZ,

        // Generic recurring events
        FRAME,
        PAUSE_FRAME,
        POSITION_CHANGED,

        // Flights, plans, and aircraft
        FLIGHT_LOADED,
        FLIGHT_SAVED,

        FLIGHTPLAN_LOADED,
        FLIGHTPLAN_DEACTIVATED,

        AIRCRAFT_LOADED,

        // Application UI changes
        SOUND,
        VIEW,

        TEXT_EVENT_CREATED,
        TEXT_EVENT_DESTROYED,

        // Recording and Playback
        PLAYBACK_STATE_CHANGED,
        RECORDER_STATE_CHANGED,

        // Weather events
        WEATHER_MODE_CHANGED,

        // AI events
        OBJECT_ADDED,
        OBJECT_REMOVED,

        // Mission events
        RACE_END,
        RACE_LAP,

        MISSION_COMPLETED,
        CUSTOM_MISSION_ACTION_EXECUTED,
        FLIGHT_SEGMENT_READY_FOR_GRADING,

        // Weapons related events
        WEAPON_FIRED,
        WEAPON_DETONATED,
        COUNTERMEASURE_FIRED,
        OBJECT_DAMAGED_BY_WEAPON,

        // Multiplayer events
        MULTIPLAYER_CLIENT_STARTED,
        MULTIPLAYER_SERVER_STARTED,
        MULTIPLAYER_SESSION_ENDED,
    }

    public class EventManager
    {

        public static readonly Dictionary<SystemEvent, string> SystemEventNames = new()
        {
            // Simulator state
            [SystemEvent.SIM] = "Sim",               // which means both of:
            [SystemEvent.SIM_START] = "SimStart",
            [SystemEvent.SIM_STOP] = "SimStop",

            [SystemEvent.PAUSE] = "Pause",           // which means both of:
            [SystemEvent.PAUSED] = "Paused",
            [SystemEvent.UNPAUSED] = "Unpaused",

            [SystemEvent.CRASHED] = "Crashed",
            [SystemEvent.CRASH_RESET] = "CrashReset",

            // Sytem events for timed recurrences
            [SystemEvent.EACH_1SEC] = "1sec",
            [SystemEvent.EACH_4SEC] = "4sec",
            [SystemEvent.FREQ_6HZ] = "6Hz",

            // Generic recurring events
            [SystemEvent.FRAME] = "Frame",
            [SystemEvent.PAUSE_FRAME] = "PauseFrame",
            [SystemEvent.POSITION_CHANGED] = "PositionChanged",

            // Flights, plans, and aircraft
            [SystemEvent.FLIGHT_LOADED] = "FlightLoaded",
            [SystemEvent.FLIGHT_SAVED] = "FlightSaved",

            [SystemEvent.FLIGHTPLAN_LOADED] = "FlightPlanLoaded",
            [SystemEvent.FLIGHTPLAN_DEACTIVATED] = "FlightPlanDeactivated",

            [SystemEvent.AIRCRAFT_LOADED] = "AircraftLoaded",

            // Application UI changes
            [SystemEvent.SOUND] = "Sound",
            [SystemEvent.VIEW] = "View",

            [SystemEvent.TEXT_EVENT_CREATED] = "TextEventCreated",
            [SystemEvent.TEXT_EVENT_DESTROYED] = "TextEventDestroyed",

            // Recording and Playback
            [SystemEvent.PLAYBACK_STATE_CHANGED] = "PlaybackStateChanged",
            [SystemEvent.RECORDER_STATE_CHANGED] = "RecorderStateChanged",

            // Weather events
            [SystemEvent.WEATHER_MODE_CHANGED] = "WeatherModeChanged",

            // AI events
            [SystemEvent.OBJECT_ADDED] = "ObjectAdded",
            [SystemEvent.OBJECT_REMOVED] = "ObjectRemoved",

            // Mission events
            [SystemEvent.RACE_END] = "RaceEnd",
            [SystemEvent.RACE_LAP] = "RaceLap",

            [SystemEvent.MISSION_COMPLETED] = "MissionCompleted",
            [SystemEvent.CUSTOM_MISSION_ACTION_EXECUTED] = "CustomMissionActionExecuted",
            [SystemEvent.FLIGHT_SEGMENT_READY_FOR_GRADING] = "FlightSegmentReadyForGrading",

            // Weapons related events
            [SystemEvent.WEAPON_FIRED] = "WeaponFired",
            [SystemEvent.WEAPON_DETONATED] = "WeaponDetonated",
            [SystemEvent.COUNTERMEASURE_FIRED] = "CountermeasureFired",
            [SystemEvent.OBJECT_DAMAGED_BY_WEAPON] = "ObjectDamagedByWeapon",

            // Multiplayer events
            [SystemEvent.MULTIPLAYER_CLIENT_STARTED] = "MultiplayerClientStarted",
            [SystemEvent.MULTIPLAYER_SERVER_STARTED] = "MultiplayerServerStarted",
            [SystemEvent.MULTIPLAYER_SESSION_ENDED] = "MultiplayerSessionEnded",
        };

        [DllImport("CsSimConnectInterOp.dll", EntryPoint = "#4")]
        public static extern bool csSubscribeToSystemEvent(IntPtr handle, UInt64 id, [MarshalAs(UnmanagedType.LPStr)] string eventName);

        private SimConnect simConnect;
        public EventManager(SimConnect simConnect)
        {
            this.simConnect = simConnect;
        }

        public delegate void SystemEventHandler(SystemEvent systemEvent);

        private static readonly UInt64 USREVT_FIRST = 64;
        private UInt64 lastEvent = USREVT_FIRST;

        public UInt64 NextEvent()
        {
            return Interlocked.Increment(ref lastEvent);
        }

        private readonly SystemEventHandler[] systemEventHandlers = new SystemEventHandler[USREVT_FIRST];

        private void SystemEventDispatcher(SystemEvent id)
        {
            if (((UInt64)id) >= USREVT_FIRST)
            {
                // Complain
            } else
            {
                lock (systemEventHandlers)
                {
                    systemEventHandlers[(UInt64)id](id);
                }
            }
        }

        public void addSystemEventHandler(SystemEvent systemEvent, SystemEventHandler handler)
        {
            if (!csSubscribeToSystemEvent(simConnect.Handle, (UInt64)systemEvent, SystemEventNames[systemEvent]))
            {
                // Complain
            }
        }
    }
}
