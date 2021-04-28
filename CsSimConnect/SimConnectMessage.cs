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
using System.Text;

namespace CsSimConnect
{
    public enum RecvId
    {
        Null,
        Exception,
        Open,
        Quit,
        Event,
        ObjectAddRemove,
        EventFilename,
        EventFrame,
        SimObjectData,
        SimObjectDataByType,
        WeatherObservation,
        CloudState,
        AssignedObjectId,
        ReservedKey,
        CustomAction,
        SystemState,
        ClientData,
        EventWeatherMode,
        AirportList,
        VorList,
        NdbList,
        WaypointList,
        EventMultiplayerServerStarted,
        EventMultiplayerClientStarted,
        EventMultiplayerSessionEnded,
        EventRaceEnd,
        EventRaceLap,
        ObserverData,

        GroundInfo,
        SynchronousBlock,
        ExternalSimCreate,
        ExternalSimDestroy,
        ExternalSimSimulate,
        ExternalSimLocationChanged,
        ExternalSimEvent,
        EventWeapon,
        EventCounterMeasure,
        EventObjectDamagedByWeapon,
        Version,
        SceneryComplexity,
        ShadowFlags,
        TacanList,
        Camera6DOF,
        CameraFOV,
        CameraSensorMode,
        CameraWindowPosition,
        CameraWindowSize,
        MissionObjectCount,
        Goal,
        MissionObjective,
        FlightSegment,
        ParameterRange,
        FlightSegmentReadyForGrading,
        GoalPair,
        EventFlightAnalysisDiagrams,
        LandingTriggerInfo,
        LandingInfo,
        SessionDuration,
        AttachPointData,
        PlaybackStateChanged,
        RecorderStateChanged,
        RecordingInfo,
        RecordingBookmarkInfo,
        TrafficSettings,
        JoystickDeviceInfo,
        MobileSceneryInRadius,
        MobileSceneryData,
        Event64,
        EventText,
        EventTextDestroyed,
        RecordingInfoW,
        RecordingBookmarkInfoW,
        SystemStateW,
        EventFilenameW,
    };

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ReceiveAppInfo
    {
        [FieldOffset(0)]
        public fixed byte ApplicationName[256];
        [FieldOffset(256)]
        public readonly UInt32 ApplicationVersionMajor;
        [FieldOffset(260)]
        public readonly UInt32 ApplicationVersionMinor;
        [FieldOffset(264)]
        public readonly UInt32 ApplicationBuildMajor;
        [FieldOffset(268)]
        public readonly UInt32 ApplicationBuildMinor;
        [FieldOffset(272)]
        public readonly UInt32 SimConnectVersionMajor;
        [FieldOffset(276)]
        public readonly UInt32 SimConnectVersionMinor;
        [FieldOffset(280)]
        public readonly UInt32 SimConnectBuildMajor;
        [FieldOffset(284)]
        public readonly UInt32 SimConnectBuildMinor;
        [FieldOffset(288)]
        private readonly UInt32 reserved1;
        [FieldOffset(292)]
        private readonly UInt32 reserved2;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ReceiveException
    {
        [FieldOffset(0)]
        public readonly UInt32 ExceptionId;
        [FieldOffset(4)]
        public readonly UInt32 SendId;
        [FieldOffset(8)]
        public readonly UInt32 Index;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ReceiveEvent
    {
        [FieldOffset(0)]
        public readonly UInt32 GroupId;
        [FieldOffset(4)]
        public readonly UInt32 Id;
        [FieldOffset(8)]
        public readonly UInt32 Data;

        // EVENT_64
        [FieldOffset(12)]
        public readonly UInt64 Data64;
        // EVENT_FILENAME
        [FieldOffset(12)]
        public fixed byte Filename[260];
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ReceiveSystemState
    {
        [FieldOffset(0)]
        public readonly UInt32 RequestId;
        [FieldOffset(4)]
        public readonly Int32 IntValue;
        [FieldOffset(8)]
        public readonly float FloatValue;
        [FieldOffset(12)]
        public fixed byte StringValue[260];
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ReceiveStruct
    {
        [FieldOffset(0)]        // Common prefix
        public readonly UInt32 Size;
        [FieldOffset(4)]
        public readonly UInt32 Version;
        [FieldOffset(8)]
        public readonly UInt32 Id;

        [FieldOffset(12)]
        public readonly ReceiveException Exception;                 // SIMCONNECT_RECV_EXCEPTION
        [FieldOffset(12)]
        public readonly ReceiveAppInfo ConnectionInfo;              // SIMCONNECT_RECV_OPEN
        [FieldOffset(12)]
        public readonly ReceiveEvent Event;                         // SIMCONNECT_RECV_EVENT
        [FieldOffset(12)]
        public readonly ReceiveSystemState SystemState;             // SIMCONNECT_RECV_SYSTEM_STATE

    }

    public class SimConnectMessage
    {

        private static readonly Logger log = Logger.GetLogger(typeof(SimConnectMessage));

        public RecvId Id { get; }
        public uint Version { get; }

        internal SimConnectMessage(ref ReceiveStruct msg)
        {
            Id = (RecvId)msg.Id;
            Version = msg.Version;
        }

        internal SimConnectMessage(RecvId id, uint version)
        {
            Id = id;
            Version = version;
        }

        internal static SimConnectMessage FromMessage(ref ReceiveStruct msg)
        {
            switch ((RecvId)msg.Id)
            {
                case RecvId.Null: break;
                case RecvId.Exception: break;
                case RecvId.Open: return new AppInfo(ref msg);
                case RecvId.Quit: break;
                case RecvId.Event: return new SimEvent(ref msg);
                case RecvId.ObjectAddRemove: break;
                case RecvId.EventFilename: break;
                case RecvId.EventFrame: break;
                case RecvId.SimObjectData: break;
                case RecvId.SimObjectDataByType: break;
                case RecvId.WeatherObservation: break;
                case RecvId.CloudState: break;
                case RecvId.AssignedObjectId: break;
                case RecvId.ReservedKey: break;
                case RecvId.CustomAction: break;
                case RecvId.SystemState: return new SimState(ref msg);
                case RecvId.ClientData: break;
                case RecvId.EventWeatherMode: break;
                case RecvId.AirportList: break;
                case RecvId.VorList: break;
                case RecvId.NdbList: break;
                case RecvId.WaypointList: break;
                case RecvId.EventMultiplayerServerStarted: break;
                case RecvId.EventMultiplayerClientStarted: break;
                case RecvId.EventMultiplayerSessionEnded: break;
                case RecvId.EventRaceEnd: break;
                case RecvId.EventRaceLap: break;
                case RecvId.ObserverData: break;

                case RecvId.GroundInfo: break;
                case RecvId.SynchronousBlock: break;
                case RecvId.ExternalSimCreate: break;
                case RecvId.ExternalSimDestroy: break;
                case RecvId.ExternalSimSimulate: break;
                case RecvId.ExternalSimLocationChanged: break;
                case RecvId.ExternalSimEvent: break;
                case RecvId.EventWeapon: break;
                case RecvId.EventCounterMeasure: break;
                case RecvId.EventObjectDamagedByWeapon: break;
                case RecvId.Version: break;
                case RecvId.SceneryComplexity: break;
                case RecvId.ShadowFlags: break;
                case RecvId.TacanList: break;
                case RecvId.Camera6DOF: break;
                case RecvId.CameraFOV: break;
                case RecvId.CameraSensorMode: break;
                case RecvId.CameraWindowPosition: break;
                case RecvId.CameraWindowSize: break;
                case RecvId.MissionObjectCount: break;
                case RecvId.Goal: break;
                case RecvId.MissionObjective: break;
                case RecvId.FlightSegment: break;
                case RecvId.ParameterRange: break;
                case RecvId.FlightSegmentReadyForGrading: break;
                case RecvId.GoalPair: break;
                case RecvId.EventFlightAnalysisDiagrams: break;
                case RecvId.LandingTriggerInfo: break;
                case RecvId.LandingInfo: break;
                case RecvId.SessionDuration: break;
                case RecvId.AttachPointData: break;
                case RecvId.PlaybackStateChanged: break;
                case RecvId.RecorderStateChanged: break;
                case RecvId.RecordingInfo: break;
                case RecvId.RecordingBookmarkInfo: break;
                case RecvId.TrafficSettings: break;
                case RecvId.JoystickDeviceInfo: break;
                case RecvId.MobileSceneryInRadius: break;
                case RecvId.MobileSceneryData: break;
                case RecvId.Event64: return new SimEvent(ref msg);
                case RecvId.EventText: break;
                case RecvId.EventTextDestroyed: break;
                case RecvId.RecordingInfoW: break;
                case RecvId.RecordingBookmarkInfoW: break;
                case RecvId.SystemStateW: break;
                case RecvId.EventFilenameW: break;
            }
            log.Error("Unknown message type {0}", msg.Id);
            return null;
        }
    }

    public class AppInfo : SimConnectMessage
    {
        public string Name { get; }
        public uint ApplicationVersionMajor { get; }
        public uint ApplicationVersionMinor { get; }
        public uint ApplicationBuildMajor { get; }
        public uint ApplicationBuildMinor { get; }
        public uint SimConnectVersionMajor { get; }
        public uint SimConnectVersionMinor { get; }
        public uint SimConnectBuildMajor { get; }
        public uint SimConnectBuildMinor { get; }

        internal AppInfo(ref ReceiveStruct msg) : base(ref msg)
        {
            unsafe
            {
                fixed (ReceiveAppInfo* r = &msg.ConnectionInfo)
                {
                    Name = Encoding.Latin1.GetString(r->ApplicationName, 256).Trim();
                }
            }
            ApplicationVersionMajor = msg.ConnectionInfo.ApplicationVersionMajor;
            ApplicationVersionMinor = msg.ConnectionInfo.ApplicationVersionMinor;
            ApplicationBuildMajor = msg.ConnectionInfo.ApplicationBuildMajor;
            ApplicationBuildMinor = msg.ConnectionInfo.ApplicationBuildMinor;
            SimConnectVersionMajor = msg.ConnectionInfo.SimConnectVersionMajor;
            SimConnectVersionMinor = msg.ConnectionInfo.SimConnectVersionMinor;
            SimConnectBuildMajor = msg.ConnectionInfo.SimConnectBuildMajor;
            SimConnectBuildMinor = msg.ConnectionInfo.SimConnectBuildMinor;
        }

        internal AppInfo(string name) : base(RecvId.Open, 0)
        {
            Name = name;
            ApplicationVersionMajor = 0;
            ApplicationVersionMinor = 0;
            ApplicationBuildMajor = 0;
            ApplicationBuildMinor = 0;
            SimConnectVersionMajor = 0;
            SimConnectVersionMinor = 0;
            SimConnectBuildMajor = 0;
            SimConnectBuildMinor = 0;
        }

        public string SimVersion()
        {
            return String.Format("{0}.{1})", ApplicationVersionMajor, ApplicationVersionMinor);
        }

        public string SimNameAndVersion()
        {
            return String.Format("{0} {1}.{2}", Name, ApplicationVersionMajor, ApplicationVersionMinor);
        }

        public string SimNameAndFullVersion()
        {
            return String.Format("{0} {1}.{2} (build {3}.{4})", Name, ApplicationVersionMajor, ApplicationVersionMinor, ApplicationBuildMajor, ApplicationBuildMinor);
        }

        public string SimConnectVersion()
        {
            return String.Format("{0}.{1}", SimConnectVersionMajor, SimConnectVersionMinor);
        }

        public string SimConnectFullVersion()
        {
            return String.Format("{0}.{1} (build {2}.{3})", SimConnectVersionMajor, SimConnectVersionMinor, SimConnectBuildMajor, SimConnectBuildMinor);
        }
    }

    public class SimEvent : SimConnectMessage
    {
        public UInt32? GroupId { get; init; }
        public UInt32 EventId { get; init; }
        public UInt32 Data { get; init; }
        public UInt64 LongData { get; init; }

        internal SimEvent(ref ReceiveStruct msg) : base(ref msg)
        {
            GroupId = (msg.Event.GroupId == 0xffffffff) ? null : msg.Event.GroupId;
            EventId = msg.Event.Id;
            Data = msg.Event.Data;
            LongData = (((RecvId)msg.Id) == RecvId.Event64) ? msg.Event.Data64 : 0;
        }
    }

    public class SimState : SimConnectMessage
    {
        public int IntValue { get; init; }
        public float FloatValue { get; init; }
        public string StringValue { get; init; }

        public bool AsBoolean()
        {
            return IntValue != 0;
        }

        internal SimState(ref ReceiveStruct msg) : base(ref msg)
        {
            IntValue = msg.SystemState.IntValue;
            FloatValue = msg.SystemState.FloatValue;
            unsafe
            {
                fixed (ReceiveSystemState* r = &msg.SystemState)
                {
                    StringValue = Encoding.Latin1.GetString(r->StringValue, 260).Trim();
                }
            }

        }
    }
}
