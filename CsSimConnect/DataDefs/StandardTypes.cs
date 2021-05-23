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

namespace CsSimConnect.DataDefs
{

    public struct InitPosition
    {
        public const int AirSpeedCruise = -1;
        public const int AirSpeedKeep = -2;

        [EmbeddedField]
        public LatLonAlt Position;
        [EmbeddedField]
        public PBH Rotation;
        public bool OnGround;
        public int AirSpeed;
    }

    public enum ModelMarker
    {
        Cg,
        ModelCenter,
        Wheel,
        Skid,
        Ski,
        Float,
        Scrape,
        Engine,
        Prop,
        Eyepoint,
        LongScale,
        LatScale,
        VertScale,
        AeroCenter,
        WingApex,
        RefChord,
        Datum,
        WingTip,
        FuelTank,
        Forces,
    }

    public struct MarkerState
    {
        public ModelMarker Marker;
        public bool State;
    }

    public struct WayPoint
    {
        public const uint FlagSpeedRequested       = 0x00000004;
        public const uint FlagThrottleRequested    = 0x00000008;
        public const uint FlagComputeVerticalSpeed = 0x00000010;
        public const uint FlagAltitudeIsAGL        = 0x00000020;
        public const uint FlagOnGround             = 0x00100000;
        public const uint FlagReverse              = 0x00200000;
        public const uint FlagWrapToFirst          = 0x00400000;

        [EmbeddedField]
        public LatLonAlt Position;
        public uint flags;
        public double KtsSpeed;
        public double PercentThrottle;
    }

    public struct LatLonAlt
    {
        public double Latitude;
        public double Longitude;
        public double Altitude;
    }

    public struct XYZ
    {
        public double X;
        public double Y;
        public double Z;
    }

    public struct PBH
    {
        public double Pitch;
        public double Bank;
        public double Heading;
    }

    public enum ObserverRegime
    {
        Tellurian =0,
        Terrestrial =1,
        Ghost =2,
    }

    public struct Observer
    {
        [EmbeddedField]
        public LatLonAlt Position;
        [EmbeddedField]
        public PBH Rotation;
        [EmbeddedField]
        public ObserverRegime Regime;
        public bool RotateOnTarget;
        public bool FocusFixed;
        public float FocalLength;
        public float FieldOfViewH;
        public float FieldOfViewV;
        public float LinearStep;
        public float AngularStep;
    }

    public enum VideoFormat
    {
        H265 =0,
    }

    public struct VideoStreamInfo
    {
        public string SourceAddress;
        public string DestinationAddress;
        public uint Port;
        public uint Width;
        public uint Height;
        public uint FrameRate;
        public uint BitRate;
        public VideoFormat Format;
    }
}
