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

using Rakis.Logging;
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
    public struct ReceiveAssignedObjectId
    {
        [FieldOffset(0)]
        public UInt32 RequestId;
        [FieldOffset(4)]
        public UInt32 ObjectId;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ReceiveAppInfo
    {
        internal const int ApplicationNameSize = 256;
        [FieldOffset(0)]
        public fixed byte ApplicationName[ApplicationNameSize];
        [FieldOffset(ApplicationNameSize)]
        public readonly UInt32 ApplicationVersionMajor;
        [FieldOffset(ApplicationNameSize+4)]
        public readonly UInt32 ApplicationVersionMinor;
        [FieldOffset(ApplicationNameSize+8)]
        public readonly UInt32 ApplicationBuildMajor;
        [FieldOffset(ApplicationNameSize+12)]
        public readonly UInt32 ApplicationBuildMinor;
        [FieldOffset(ApplicationNameSize+16)]
        public readonly UInt32 SimConnectVersionMajor;
        [FieldOffset(ApplicationNameSize+20)]
        public readonly UInt32 SimConnectVersionMinor;
        [FieldOffset(ApplicationNameSize+24)]
        public readonly UInt32 SimConnectBuildMajor;
        [FieldOffset(ApplicationNameSize+28)]
        public readonly UInt32 SimConnectBuildMinor;
        [FieldOffset(ApplicationNameSize+32)]
        private readonly UInt32 reserved1;
        [FieldOffset(ApplicationNameSize+36)]
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
        [FieldOffset(272)]
        public readonly UInt32 FileNameFlags;

        // EVENT_OBJECT_ADDREMOVE
        [FieldOffset(12)]
        public UInt32 ObjectType;
        [FieldOffset(16)]
        public UInt32 ObjectFlags;
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ReceiveSystemState
    {
        [FieldOffset(0)]
        public readonly UInt32 Id;
        [FieldOffset(4)]
        public readonly Int32 IntValue;
        [FieldOffset(8)]
        public readonly float FloatValue;
        [FieldOffset(12)]
        public fixed byte StringValue[260];
    }

    [StructLayout(LayoutKind.Explicit)]
    unsafe public struct ReceiveSimObjectData
    {
        [FieldOffset(0)]
        public readonly uint Id;
        [FieldOffset(4)]
        public readonly uint ObjectId;
        [FieldOffset(8)]
        public readonly uint DefineId;
        [FieldOffset(12)]
        public readonly uint Flags;
        [FieldOffset(16)]
        public readonly uint EntryNumber;
        [FieldOffset(20)]
        public readonly uint OutOf;
        [FieldOffset(24)]
        public readonly uint DefineCount;

        internal readonly static uint SimConnect_Recv_SimObject_Data_Prefix_len = 28;

        [FieldOffset(28)]
        public readonly byte Data;
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

        internal const uint SimConnect_Recv_Prefix_Len = 12;

        [FieldOffset(12)]
        public readonly ReceiveException Exception;                     // SIMCONNECT_RECV_EXCEPTION
        [FieldOffset(12)]
        public readonly ReceiveAppInfo ConnectionInfo;                  // SIMCONNECT_RECV_OPEN
        [FieldOffset(12)]
        public readonly ReceiveEvent Event;                             // SIMCONNECT_RECV_EVENT, SIMCONNECT_RECV_EVENT_64, SIMCONNECT_RECV_EVENT_FILENAME, SIMCONNECT_RECV_EVENT_OBJECT_ADDREMOVE
        [FieldOffset(12)]
        public readonly ReceiveAssignedObjectId AssignedObjectId;       // SIMCONNECT_RECV_ASSIGNED_OBJECT_ID
        [FieldOffset(12)]
        public readonly ReceiveSimObjectData ObjectData;                // SIMCONNECT_RECV_SIMOBJECT_DATA
        [FieldOffset(12)]
        public readonly ReceiveSystemState SystemState;                 // SIMCONNECT_RECV_SYSTEM_STATE

    }

    public class SimConnectMessage
    {

        protected static readonly ILogger log = Logger.GetLogger(typeof(SimConnectMessage));

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

        internal static SimConnectMessage FromMessage(ref ReceiveStruct msg, uint structLen)
        {
            switch ((RecvId)msg.Id)
            {
                case RecvId.Null: break;
                case RecvId.Exception: break;
                case RecvId.Open: return new AppInfo(ref msg);
                case RecvId.Quit: break;
                case RecvId.Event: return new SimEvent(ref msg);
                case RecvId.ObjectAddRemove: return new ObjectAddedRemoved(ref msg);
                case RecvId.EventFilename: break;
                case RecvId.EventFrame: break;
                case RecvId.SimObjectData: return new ObjectData(ref msg, structLen);
                case RecvId.SimObjectDataByType: return new ObjectData(ref msg, structLen); // Structure is same as previous!
                case RecvId.WeatherObservation: break;
                case RecvId.CloudState: break;
                case RecvId.AssignedObjectId: return new AssignedObjectId(ref msg);
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
            log.Error?.Log("Unknown message type {0}", msg.Id);
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
                fixed (byte* str = &msg.ConnectionInfo.ApplicationName[0])
                {
                    DataBlock data = new(ReceiveAppInfo.ApplicationNameSize, str);
                    Name = data.FixedString(ReceiveAppInfo.ApplicationNameSize);
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
        public uint? GroupId { get; init; }
        public uint EventId { get; init; }
        public uint Data { get; init; }
        public ulong LongData { get; init; }

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

    public class ObjectData : SimConnectMessage
    {
        public uint RequestId { get; }
        public uint ObjectId { get; }
        public uint DefineId { get; }
        public uint Flags { get; }
        public uint EntryNumber { get; }
        public uint OutOf { get; }
        public DataBlock Data { get; }

        private static readonly uint prefixSize = ReceiveStruct.SimConnect_Recv_Prefix_Len + ReceiveSimObjectData.SimConnect_Recv_SimObject_Data_Prefix_len;

        internal ObjectData(ref ReceiveStruct msg, uint structLen) : base(ref msg)
        {
            RequestId = msg.ObjectData.Id;
            ObjectId = msg.ObjectData.ObjectId;
            DefineId = msg.ObjectData.DefineId;
            Flags = msg.ObjectData.Flags;
            EntryNumber = msg.ObjectData.EntryNumber;
            OutOf = msg.ObjectData.OutOf;

            unsafe {
                fixed (byte* msgData = &msg.ObjectData.Data)
                {
                    Data = new(structLen - prefixSize, msgData);
                }
            }
        }

    }

    public class AssignedObjectId : SimConnectMessage
    {
        public uint RequestId { get; init; }
        public uint ObjectId { get; init; }

        internal AssignedObjectId(ref ReceiveStruct msg) : base(ref msg)
        {
            RequestId = msg.AssignedObjectId.RequestId;
            ObjectId = msg.AssignedObjectId.ObjectId;
        }
    }

    public class ObjectAddedRemoved : SimConnectMessage
    {
        public uint EventId { get; init; }
        public uint ObjectId { get; init; }
        public ObjectType Type { get; init; }
        public uint ObjectFlags { get; init; }

        internal ObjectAddedRemoved(ref ReceiveStruct msg) : base(ref msg)
        {
            EventId = msg.Event.Id;
            ObjectId = msg.Event.Data;
            Type = (ObjectType)msg.Event.ObjectType;
            ObjectFlags = msg.Event.ObjectFlags;
        }
    }
}
