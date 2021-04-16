﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CsSimConnect
{
    public enum RecvId
    {
        SIMCONNECT_RECV_ID_NULL,
        SIMCONNECT_RECV_ID_EXCEPTION,
        SIMCONNECT_RECV_ID_OPEN,
        SIMCONNECT_RECV_ID_QUIT,
        SIMCONNECT_RECV_ID_EVENT,
        SIMCONNECT_RECV_ID_EVENT_OBJECT_ADDREMOVE,
        SIMCONNECT_RECV_ID_EVENT_FILENAME,
        SIMCONNECT_RECV_ID_EVENT_FRAME,
        SIMCONNECT_RECV_ID_SIMOBJECT_DATA,
        SIMCONNECT_RECV_ID_SIMOBJECT_DATA_BYTYPE,
        SIMCONNECT_RECV_ID_WEATHER_OBSERVATION,
        SIMCONNECT_RECV_ID_CLOUD_STATE,
        SIMCONNECT_RECV_ID_ASSIGNED_OBJECT_ID,
        SIMCONNECT_RECV_ID_RESERVED_KEY,
        SIMCONNECT_RECV_ID_CUSTOM_ACTION,
        SIMCONNECT_RECV_ID_SYSTEM_STATE,
        SIMCONNECT_RECV_ID_CLIENT_DATA,
        SIMCONNECT_RECV_ID_EVENT_WEATHER_MODE,
        SIMCONNECT_RECV_ID_AIRPORT_LIST,
        SIMCONNECT_RECV_ID_VOR_LIST,
        SIMCONNECT_RECV_ID_NDB_LIST,
        SIMCONNECT_RECV_ID_WAYPOINT_LIST,
        SIMCONNECT_RECV_ID_EVENT_MULTIPLAYER_SERVER_STARTED,
        SIMCONNECT_RECV_ID_EVENT_MULTIPLAYER_CLIENT_STARTED,
        SIMCONNECT_RECV_ID_EVENT_MULTIPLAYER_SESSION_ENDED,
        SIMCONNECT_RECV_ID_EVENT_RACE_END,
        SIMCONNECT_RECV_ID_EVENT_RACE_LAP,
        SIMCONNECT_RECV_ID_OBSERVER_DATA,

        SIMCONNECT_RECV_ID_GROUND_INFO,
        SIMCONNECT_RECV_ID_SYNCHRONOUS_BLOCK,
        SIMCONNECT_RECV_ID_EXTERNAL_SIM_CREATE,
        SIMCONNECT_RECV_ID_EXTERNAL_SIM_DESTROY,
        SIMCONNECT_RECV_ID_EXTERNAL_SIM_SIMULATE,
        SIMCONNECT_RECV_ID_EXTERNAL_SIM_LOCATION_CHANGED,
        SIMCONNECT_RECV_ID_EXTERNAL_SIM_EVENT,
        SIMCONNECT_RECV_ID_EVENT_WEAPON,
        SIMCONNECT_RECV_ID_EVENT_COUNTERMEASURE,
        SIMCONNECT_RECV_ID_EVENT_OBJECT_DAMAGED_BY_WEAPON,
        SIMCONNECT_RECV_ID_VERSION,
        SIMCONNECT_RECV_ID_SCENERY_COMPLEXITY,
        SIMCONNECT_RECV_ID_SHADOW_FLAGS,
        SIMCONNECT_RECV_ID_TACAN_LIST,
        SIMCONNECT_RECV_ID_CAMERA_6DOF,
        SIMCONNECT_RECV_ID_CAMERA_FOV,
        SIMCONNECT_RECV_ID_CAMERA_SENSOR_MODE,
        SIMCONNECT_RECV_ID_CAMERA_WINDOW_POSITION,
        SIMCONNECT_RECV_ID_CAMERA_WINDOW_SIZE,
        SIMCONNECT_RECV_ID_MISSION_OBJECT_COUNT,
        SIMCONNECT_RECV_ID_GOAL,
        SIMCONNECT_RECV_ID_MISSION_OBJECTIVE,
        SIMCONNECT_RECV_ID_FLIGHT_SEGMENT,
        SIMCONNECT_RECV_ID_PARAMETER_RANGE,
        SIMCONNECT_RECV_ID_FLIGHT_SEGMENT_READY_FOR_GRADING,
        SIMCONNECT_RECV_ID_GOAL_PAIR,
        SIMCONNECT_RECV_ID_EVENT_FLIGHT_ANALYSIS_DIAGRAMS,
        SIMCONNECT_RECV_ID_LANDING_TRIGGER_INFO,
        SIMCONNECT_RECV_ID_LANDING_INFO,
        SIMCONNECT_RECV_ID_SESSION_DURATION,
        SIMCONNECT_RECV_ID_ATTACHPOINT_DATA,
        SIMCONNECT_RECV_ID_PLAYBACK_STATE_CHANGED,
        SIMCONNECT_RECV_ID_RECORDER_STATE_CHANGED,
        SIMCONNECT_RECV_ID_RECORDING_INFO,
        SIMCONNECT_RECV_ID_RECORDING_BOOKMARK_INFO,
        SIMCONNECT_RECV_ID_TRAFFIC_SETTINGS,
        SIMCONNECT_RECV_ID_JOYSTICK_DEVICE_INFO,
        SIMCONNECT_RECV_ID_MOBILE_SCENERY_IN_RADIUS,
        SIMCONNECT_RECV_ID_MOBILE_SCENERY_DATA,
        SIMCONNECT_RECV_ID_EVENT_64,
        SIMCONNECT_RECV_ID_EVENT_TEXT,
        SIMCONNECT_RECV_ID_EVENT_TEXT_DESTROYED,
        SIMCONNECT_RECV_ID_RECORDING_INFO_W,
        SIMCONNECT_RECV_ID_RECORDING_BOOKMARK_INFO_W,
        SIMCONNECT_RECV_ID_SYSTEM_STATE_W,
        SIMCONNECT_RECV_ID_EVENT_FILENAME_W,
    };
    [StructLayout(LayoutKind.Explicit)]
    public struct ReceiveStruct
    {
        [FieldOffset(0)]
        public UInt64 Size;
        [FieldOffset(4)]
        public UInt64 Version;
        [FieldOffset(8)]
        public UInt64 Id;
    }

    public class MessageDispatcher
    {

        public delegate void DispatchProc(ref ReceiveStruct structData, Int64 wordData, IntPtr context);

        [DllImport("CsSimConnectInterOp.dll", EntryPoint = "#1")]
        public static extern bool csCallDispatch(IntPtr handle, DispatchProc dispatchProc);

        private SimConnect simConnect;
        private Task dispatchTask;

        private bool isConnected = false;
        public bool IsConnected { get;  set; }

        public MessageDispatcher(SimConnect simConnect)
        {
            this.simConnect = simConnect;
            dispatchTask = Task.Run(dispatchLoop);
        }

        private void dispatchLoop()
        {
            while (isConnected)
            {
                csCallDispatch(simConnect.Handle, (ref ReceiveStruct structData, Int64 wordData, IntPtr context) =>
                {
                    if (structData.Id > (int)RecvId.SIMCONNECT_RECV_ID_EVENT_FILENAME_W)
                    {
                        //Error
                        return;
                    }
                    switch ((RecvId) structData.Id)
                    {
                        case RecvId.SIMCONNECT_RECV_ID_OPEN:
                            isConnected = true;
                            break;
                        case RecvId.SIMCONNECT_RECV_ID_QUIT:
                            isConnected = false;
                            break;
                        default:
                            break;
                    }
                });

            }
        }
    }
}
