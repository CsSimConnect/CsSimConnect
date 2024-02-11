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

namespace CsSimConnect.DataDefs.Standard
{
    public struct InitPosition
    {
        public const int AirSpeedCruise = -1;
        public const int AirSpeedKeep = -2;

        public InitPosition(double lat, double lon, double alt, double pitch, double bank, double heading, bool onGround, int airSpeed) : this()
        {
            Position = new(lat, lon, alt);
            Orientation = new(pitch, bank, heading);
            OnGround = onGround;
            AirSpeed = airSpeed;
        }

        [EmbeddedField]
        public LatLonAlt Position { get; set; }
        [EmbeddedField]
        public PBH Orientation { get; set; }
        public bool OnGround { get; set; }
        public int AirSpeed { get; set; }
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
        public ModelMarker Marker { get; set; }
        public bool State { get; set; }
    }

    public struct WayPoint
    {
        public const uint FlagSpeedRequested = 0x00000004;
        public const uint FlagThrottleRequested = 0x00000008;
        public const uint FlagComputeVerticalSpeed = 0x00000010;
        public const uint FlagAltitudeIsAGL = 0x00000020;
        public const uint FlagOnGround = 0x00100000;
        public const uint FlagReverse = 0x00200000;
        public const uint FlagWrapToFirst = 0x00400000;

        [EmbeddedField]
        public LatLonAlt Position { get; set; }
        public uint Flags { get; set; }
        public double KtsSpeed { get; set; }
        public double PercentThrottle { get; set; }
    }

    public struct LatLonAlt
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }

        public LatLonAlt(double lat, double lon, double alt)
        {
            Latitude = lat;
            Longitude = lon;
            Altitude = alt;
        }
    }

    public struct XYZ
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public XYZ(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public struct PBH
    {
        public double Pitch { get; set; }
        public double Bank { get; set; }
        public double Heading { get; set; }

        public PBH(double pitch, double bank, double heading)
        {
            Pitch = pitch;
            Bank = bank;
            Heading = heading;
        }
    }

    public enum ObserverRegime
    {
        Tellurian = 0,
        Terrestrial = 1,
        Ghost = 2,
    }

    public struct Observer
    {
        [EmbeddedField]
        public LatLonAlt Position { get; set; }
        [EmbeddedField]
        public PBH Rotation { get; set; }
        [EmbeddedField]
        public ObserverRegime Regime { get; set; }
        public bool RotateOnTarget { get; set; }
        public bool FocusFixed { get; set; }
        public float FocalLength { get; set; }
        public float FieldOfViewH { get; set; }
        public float FieldOfViewV { get; set; }
        public float LinearStep { get; set; }
        public float AngularStep { get; set; }
    }

    public enum VideoFormat
    {
        H265 = 0,
    }

    public struct VideoStreamInfo
    {
        public string SourceAddress { get; set; }
        public string DestinationAddress { get; set; }
        public uint Port { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint FrameRate { get; set; }
        public uint BitRate { get; set; }
        public VideoFormat Format { get; set; }
    }
}
