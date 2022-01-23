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

using System.Runtime.InteropServices;

using static SimScanner.Bgl.BglRecord;

namespace SimScanner.Bgl
{
    public enum RecordId : ushort
    {
        Null = 0x00,
        RunWay = 0x04, // SubRecord FSX/P3D
        JetwayFS9 = 0x05,
        PrimaryOffsetThreshold = 0x05, // SubSubRecord of Runway
        SecondaryOffsetThreshold = 0x06, // SubSubRecord of Runway
        PrimaryBlastPad = 0x07, // SubSubRecord of Runway
        SecondaryBlastPad = 0x08, // SubSubRecord of Runway
        PrimaryOverrun = 0x09, // SubSubRecord of Runway
        SecondaryOverrun = 0x0A, // SubSubRecord of Runway
        LibraryObject = 0x000B,
        PrimaryVasiLeft = 0x0B, // SubSubRecord of Runway
        PrimaryVasiRight = 0x0C, // SubSubRecord of Runway
        SecondaryVasiLeft = 0x0D, // SubSubRecord of Runway
        SecondaryVasiRight = 0x0E, // SubSubRecord of Runway
        PrimaryApproachLights = 0x0F, // SubSubRecord of Runway
        SecondaryApproachLights = 0x10, // SubSubRecord of Runway
        RunwayStart = 0x11, // SubRecord
        Com = 0x12, // Record
        AirportName = 0x19, // SubRecord
        TaxiwayPointFSX = 0x1A, // SubRecord FSX/P3D
        TaxiwayPath = 0x1C, // SubRecord FSX/P3D
        TaxiwayName = 0x1D, // SubRecord
        TaxiWayPoint = 0x22, // SubRecord MSFS
        Approach = 0x24, // SubRecord
        Helipad = 0x26, // SubRecord
        ApronDetail = 0x30, // SubRecord FSX/P3D
        ApronEdgeLight = 0x31, // SubRecord
        DeleteAirport = 0x33, // SubRecord
        ApronSurface = 0x37, // SubRecord FSX/P3D
        BlastFence = 0x38, // SubRecord
        BoundaryFence = 0x39, // SubRecord
        Jetway = 0x3A, // SubRecord FSX/P3D
        Unknown3B = 0x3B, // SubRecord FSX/P3D
        AirportFSX = 0x3C, // Record    FSX/P3D
        TaxiwayParkingFSX = 0x3D, // SubRecord
        AirportMSFS = 0x56, // Record    MSFS
        AirportTowerScenery = 0x66, // SubRecord
        AirportP3D = 0xAB, // Record    P3Dv5
        TaxiwayPointP3D = 0xAC, // SubRecord
        TaxiwayParkingP3D = 0xAD, // SubRecord
        RunWayMSFS = 0xCE, // SubRecord MSFS
        PaintedLineMSFS = 0xCF, // SubRecord MSFS
        ApronMSFS = 0xD3, // SubRecord MSFS
        TaxiwayPathMSFS = 0xD4, // SubRecord MSFS
        PaintedHatchedAreaMSFS = 0xD8, // SubRecord MSFS
        TaxiwaySignMSFS = 0xD9, // SubRecord MSFS
        TaxiwayParkingMfgrName = 0xDD, // SubRecord MSFS
        JetwayMSFS = 0xDE, // SubRecord MSFS
        TaxiwayParkingMSFS = 0xE7, // SubRecord MSFS
        ProjectedMeshMSFS = 0xE8, // SubRecord MSFS
        GroundMergingTransfer = 0xE9, // SubRecord MSFS
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BglObjectHeader
    {
        public const uint HeaderSize = 6;

        public readonly ushort Id;
        public readonly uint Size;

        public uint DataSize => Size - HeaderSize;
    }

    // 0x0004
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Runway
    {
        public readonly RunwaySurfaceType SurfaceType;
        public readonly RunwayNumber PrimaryNumber;
        public readonly RunwayDesignator PrimaryDesignator;
        public readonly RunwayNumber SecondaryNumber;
        public readonly RunwayDesignator SecondaryDesignator;
        public readonly uint EncodedPrimaryIlsICAO;
        public readonly uint EncodedSecondaryIlsICAO;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;       // In meters, x 1000 (signed!)
        public readonly float Length;               // In meters
        public readonly float Width;                // In meters
        public readonly float Heading;
        public readonly float PatternAltitude;      // In meters
        public readonly ushort MarkingFlags;        // See enum RunwayMarkingFlags
        public readonly byte LightsFlags;           // See enum RunwayLightsFlags
        public readonly byte PatternFlags;          // See enum RunwayPatternFlags

        public string PrimaryILSICAO => DecodeName(EncodedPrimaryIlsICAO, false);
        public string SecondaryILSICAO => DecodeName(EncodedSecondaryIlsICAO, false);

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);

        public bool MarkedEdges                     => (MarkingFlags & 0b00000000_00000001) != 0;
        public bool MarkedThreshold                 => (MarkingFlags & 0b00000000_00000010) != 0;
        public bool MarkedFixedDistance             => (MarkingFlags & 0b00000000_00000100) != 0;
        public bool MarkedTouchDown                 => (MarkingFlags & 0b00000000_00001000) != 0;
        public bool MarkedDashes                    => (MarkingFlags & 0b00000000_00010000) != 0;
        public bool MarkedIdent                     => (MarkingFlags & 0b00000000_00100000) != 0;
        public bool MarkedPrecision                 => (MarkingFlags & 0b00000000_01000000) != 0;
        public bool MarkedEdgePavement              => (MarkingFlags & 0b00000000_10000000) != 0;
        public bool MarkedSingleEnd                 => (MarkingFlags & 0b00000001_00000000) != 0;
        public bool MarkededPrimaryClosed           => (MarkingFlags & 0b00000010_00000000) != 0;
        public bool MarkedSecondaryClosed           => (MarkingFlags & 0b00000100_00000000) != 0;
        public bool MarkedPrimarySTOL               => (MarkingFlags & 0b00001000_00000000) != 0;
        public bool MarkedSecondarySTOL             => (MarkingFlags & 0b00010000_00000000) != 0;
        public bool MarkedAlternateThreshold        => (MarkingFlags & 0b00100000_00000000) != 0;
        public bool MarkedAlternateFixedDistance    => (MarkingFlags & 0b01000000_00000000) != 0;
        public bool MarkedAlternateTouchDown        => (MarkingFlags & 0b10000000_00000000) != 0;

        public Intensity EdgeLights                 => (Intensity)(LightsFlags & 0b00000011);
        public Intensity CenterLights               => (Intensity)((LightsFlags & 0b00001100) >> 2);
        public bool HasCenterRedLights              => (LightsFlags & 0b00010000) != 0;
        public bool HasAlternatePrecisionLights     => (LightsFlags & 0b00100000) != 0;
        public bool HasLeadingZeroIdent             => (LightsFlags & 0b01000000) != 0;
        public bool HasNoThresholdEndArrows         => (LightsFlags & 0b10000000) != 0;

        public bool HasPrimaryTakeoffPattern        => (PatternFlags & 0b00000001) == 0;
        public bool HasPrimaryLandingPattern        => (PatternFlags & 0b00000010) == 0;
        public bool HasPrimaryPattern               => (PatternFlags & 0b00000100) == 0;
        public bool HasSecondaryTakeoffPattern      => (PatternFlags & 0b00001000) == 0;
        public bool HasSecondaryLandingPattern      => (PatternFlags & 0b00010000) == 0;
        public bool HasSecondaryPattern             => (PatternFlags & 0b00100000) == 0;
    }

    public enum RunwaySurfaceType : ushort
    {
        Concrete            = 0x0000,
        Grass               = 0x0001,
        Water               = 0x0002,
        Asphalt             = 0x0004,
        Clay                = 0x0007,
        Snow                = 0x0008,
        Ice                 = 0x0009,
        Dirt                = 0x000C,
        Coral               = 0x000D,
        Gravel              = 0x000E,
        OilTreated          = 0x000F,
        SteelMats           = 0x0010,
        Bitumimous          = 0x0011,
        Brick               = 0x0012,
        Macadam             = 0x0013,
        Plank               = 0x0014,
        Sand                = 0x0015,
        Shale               = 0x0016,
        Tarmac              = 0x0017,
        Unknown             = 0x00FE,
    }

    public enum RunwayNumber : byte
    {
        Heading01 = 1, Heading02 = 2, Heading03 = 3, Heading04 = 4,
        Heading05 = 5, Heading06 = 6, Heading07 = 7, Heading08 = 8,
        Heading09 = 9, Heading10 = 10, Heading11 = 11, Heading12 = 12,
        Heading13 = 13, Heading14 = 14, Heading15 = 15, Heading16 = 16,
        Heading17 = 17, Heading18 = 18, Heading19 = 19, Heading20 = 20,
        Heading21 = 21, Heading22 = 22, Heading23 = 23, Heading24 = 24,
        Heading25 = 25, Heading26 = 26, Heading27 = 27, Heading28 = 28,
        Heading29 = 29, Heading30 = 30, Heading31 = 31, Heading32 = 32,
        Heading33 = 33, Heading34 = 34, Heading35 = 25, Heading36 = 36,

        N = 37,
        NE = 38,
        E = 39,
        SE = 40,
        S = 41,
        SW = 42,
        W = 43,
        NW = 44,
    }

    public enum RunwayDesignator : byte
    {
        None = 0,
        Left = 1,
        Right = 2,
        Center = 3,
        Water = 4,
        A = 5,
        B = 6,
    }

    public enum RunwayMarkingFlags : ushort
    {
        Edges = 0b0000000000000001,
        Threshold = 0b0000000000000010,
        FixedDistance = 0b0000000000000100,
        Touchdown = 0b0000000000001000,
        Dashes = 0b0000000000010000,
        Ident = 0b0000000000100000,
        Precision = 0b0000000001000000,
        EdgePavement = 0b0000000010000000,
        SingleEnd = 0b0000000100000000,
        PrimaryClosed = 0b0000001000000000,
        SecondaryClosed = 0b0000010000000000,
        PrimaryStol = 0b0000100000000000,
        SecondaryStol = 0b0001000000000000,
        AlternateThreshold = 0b0010000000000000,
        AlternateFixedDistance = 0b0100000000000000,
        AlternateTouchdown = 0b1000000000000000,
    }

    public enum Intensity
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    }

    public enum RunwayLightsFlags : byte
    {
        EdgeNone = 0b00000000,
        EdgeLow = 0b00000001,
        EdgeMedium = 0b00000010,
        EdgeHigh = 0b00000011,
        EdgeMask = 0b00000011,
        CenterNone = 0b00000000,
        CenterLow = 0b00000100,
        CenterMedium = 0b00001000,
        CenterHigh = 0b00001100,
        CenterMask = 0b00001100,
        CenterRed = 0b00010000,
        AlternatePrecision = 0b00100000,
        LeadingZeroIdent = 0b01000000,
        NoThresholdEndArrows = 0b10000000,
    }

    public enum RunwayPatternFlags
    {
        PrimaryTakeoffYes = 0b00000000,
        PrimaryTakeoffNo = 0b00000001,
        PrimaryTakeoffMask = 0b00000001,
        PrimaryLandingYes = 0b00000000,
        PrimaryLandingNo = 0b00000010,
        PrimaryLandingMask = 0b00000010,
        PrimaryPatternLeft = 0b00000000,
        PrimaryPatternRight = 0b00000100,
        PrimaryPatternMask = 0b00000100,

        SecondaryTakeoffYes = 0b00000000,
        SecondaryTakeoffNo = 0b00001000,
        SecondaryTakeoffMask = 0b00001000,
        SecondaryLandingYes = 0b00000000,
        SecondaryLandingNo = 0b00010000,
        SecondaryLandingMask = 0b00010000,
        SecondaryPatternLeft = 0b00000000,
        SecondaryPatternRight = 0b00100000,
        SecondaryPatternMask = 0b00100000,
    }

    // 0x0005, 0x0006
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RunwayOffsetThreshold
    {
        public readonly RunwaySurfaceType SurfaceType;
        public readonly float Length;               // In meters
        public readonly float Width;                // In meters
    }

    // 0x0007, 0x0008
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RunwayBlastPad
    {
        public readonly RunwaySurfaceType SurfaceType;
        public readonly float Length;               // In meters
        public readonly float Width;                // In meters
    }

    // 0x0009, 0x000A
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RunwayOverrun
    {
        public readonly RunwaySurfaceType SurfaceType;         // See enum RunwaySurfaceType
        public readonly float Length;               // In meters
        public readonly float Width;                // In meters
    }

    // 0x000B, 0x000C, 0x000D, 0x000E
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RunwayVasi
    {
        public readonly VasiType VasiType;            // See enum VasiType
        public readonly float BiasX;
        public readonly float BiasY;
        public readonly float Spacing;
        public readonly float Pitch;
    }

    public enum VasiType : ushort
    {
        VASI21 = 0x0001,
        VASI31 = 0x0002,
        VASI22 = 0x0003,
        VASI32 = 0x0004,
        VASI23 = 0x0005,
        VASI33 = 0x0006,
        PAPI2 = 0x0007,
        PAPI4 = 0x0008,
        TRICOLOR = 0x0009,
        PVASI = 0x000A,
        TVASI = 0x000B,
        BALL = 0x000C,
        APAP = 0x000D,
    }

    // 0x000B
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LibraryObject
    {
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;              // In meters, x 1000 (signed!)
        public readonly SceneryFlags Flags;
        public readonly short Pitch;
        public readonly short Bank;
        public readonly short Heading;
        public readonly ImageComplexity Complexity;
        public readonly short Unknown;
        public readonly ulong FSXGuid;
        public readonly ulong GuidName;
        public readonly float Scale;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);

        // Here follow the signs (array of TaxiwaySignData)
    }

    public enum ImageComplexity : ushort
    {
        VerySparse              = 0x0000,
        Sparse                  = 0x0001,
        Normal                  = 0x0002,
        Dense                   = 0x0003,
        VeryDense               = 0x0004
    }

    // 0x000E
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwaySign
    {
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;              // In meters, x 1000 (signed!)
        public readonly SceneryFlags Flags;
        public readonly short Pitch;
        public readonly short Bank;
        public readonly short Heading;
        public readonly ImageComplexity Complexity;
        public readonly short Unknown;
        public readonly uint NumSigns;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);

        // Here follow the signs (array of TaxiwaySignData)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwaySignData
    {
        public readonly float LongitudeOffset;
        public readonly float LatitudeOffset;
        public readonly short Heading;
        public readonly byte Size;                  // 1-5
        public readonly Justification Justification;

        // Zero-terminated label follows here.
    }

    public enum SceneryFlags : ushort
    {
        AboveAGL                = 0b00000000_00000001,
        NoAutoGenSuppression    = 0b00000000_00000010,
        NoCrash                 = 0b00000000_00000100,
        NoFog                   = 0b00000000_00001000,
        NoShadow                = 0b00000000_00010000,
        NoZWrite                = 0b00000000_00100000,
        NoZTest                 = 0b00000000_01000000,
    }

    public enum Justification : byte
    {
        Right = 0x01,
        Left = 0x02
    }

    // 0x000F, 0x0010
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RunwayApproachLights
    {
        public readonly byte System;                // See enum ApproachLightSystem
        public readonly byte NumStrobes;

        public ApproachLightSystem LightSystem  => (ApproachLightSystem)(System & 0b00011111);
        public bool HasEndLights                => (System & 0b00100000) != 0;
        public bool HasREIL                     => (System & 0b01000000) != 0;
        public bool HasTouchDownLights          => (System & 0b10000000) != 0;

    }

    public enum ApproachLightSystem : byte
    {
        None = 0x00,
        ODALS = 0x01,
        MALSF = 0x02,
        MALSR = 0x03,
        SSALF = 0x04,
        SSALR = 0x05,
        ALSF1 = 0x06,
        ALSF2 = 0x07,
        RAIL = 0x08,
        CALVERT = 0x09,
        CALVERT2 = 0x0A,
        MALS = 0x0B,
        SALS = 0x0C,
        SSALS = 0x0E,
    }

    // 0x0011
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RunwayStart
    {
        public readonly byte Number;
        public readonly byte TypeFlags;             // See enum RunwayStartType
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;              // In meters, x 1000 (signed!)
        public readonly float Heading;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);

        public RunwayDesignator Designator      => (RunwayDesignator)(TypeFlags & 0b00001111);
        public RunwayStartType Type             => (RunwayStartType)(TypeFlags & 0b11110000);
    }

    public enum RunwayStartType : byte
    {
        Runway = 0x10,
        Water = 0x20,
        Helipad = 0x30,
    }

    // 0x0012
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Com
    {
        public readonly ComType Type;                // See enum ComType
        public readonly uint EncodedFrequency;             // x 1000000

        public double Frequency => EncodedFrequency / 1000000.0;

        // Signal name starts here at 
    }

    public enum ComType : ushort
    {
        ATIS = 0x0001,
        MULTICOM = 0x0002,
        UNICOM = 0x0003,
        CTAF = 0x0004,
        Ground = 0x0005,
        Tower = 0x0006,
        Clearance = 0x0007,
        Approach = 0x0008,
        Departure = 0x0009,
        Center = 0x000A,
        FSS = 0x000B,
        AWOS = 0x000C,
        ASOS = 0x000D,
        ClearancePreTaxi = 0x000E,
        RemoteClearanceDelivery = 0x000F,
    }

    // 0x0013
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ILSVOR
    {
        public readonly VORType Type;
        public readonly byte Flags;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;              // In meters, x 1000 (signed!)
        public readonly uint EncodedFrequency;
        public readonly float Range;                // In meters
        public readonly float ManeticVariance;
        public readonly uint EncodedICAO;
        public readonly uint EncodedRegionIdent;

        public bool IsDMEOnly => (Flags & 0b00000001) == 0;
        public bool IsILS => (Flags & 0b00000001) != 0;
        public bool HasBackCourse => (Flags & 0b00000100) != 0;
        public bool HasGlideSlope => (Flags & 0b00001000) != 0;
        public bool HasDME => (Flags & 0b00010000) != 0;
        public bool HasNAV => (Flags & 0b00100000) != 0;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);
        public double Frequency => EncodedFrequency / 1000000.0;

        public string ICAO => DecodeName(EncodedICAO, false);
        public string Region => DecodeName(EncodedRegionIdent & 0b00000011_11111111, false);
        public string AirportICAO => DecodeName(EncodedRegionIdent >> 11, false);
    }

    public enum VORType : byte
    {
        VORTerminal = 0x01,
        VORLow = 0x02,
        VORHigh = 0x03,
        ILS = 0x04,
        VORTOT = 0x05,
    }

    // 0x0014
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Localizer
    {
        public readonly RunwayNumber RunwayNumber;
        public readonly RunwayDesignator RunwayDesignator;
        public readonly float Heading;
        public readonly float BeamWidth;                // In degrees.
    }

    // 0x0015
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GlideSlope
    {
        public readonly ushort Unknown;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;              // In meters, x 1000 (signed!)
        public readonly float Range;                // In meters
        public readonly float Pitch;                // In degrees

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);
    }

    // 0x0016
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DME
    {
        public readonly ushort Unknown;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;              // In meters, x 1000 (signed!)
        public readonly float Range;                // In meters

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);

        // Optional Name following
    }

    // 0x0017
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NDB
    {
        public readonly ushort Type;
        public readonly uint EncodedFrequency;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int EncodedElevation;              // In meters, x 1000 (signed!)
        public readonly float Range;                // In meters
        public readonly float MagneticVariance;
        public readonly uint EncodedICAO;
        public readonly uint EncodedRegionIdent;

        public double Frequency => EncodedFrequency / 1000000.0;
        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
        public double Elevation => DecodeElevation(EncodedElevation);
        public double ElevationInFeet => DecodeElevationToFeet(EncodedElevation);

        public string ICAO => DecodeName(EncodedICAO, false);
        public string Region => DecodeName(EncodedRegionIdent & 0b00000011_11111111, false);
        public string AirportICAO => DecodeName(EncodedRegionIdent >> 11, false);

        // Optional Name following
    }

    public enum NDBType : ushort
    {
        CompassPoint        = 0x0001,
        MH                  = 0x0002,
        H                   = 0x0003,
        HH                  = 0x0004,
    }

    // 0x0019
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Name
    {
        // Actual name starts here at pos 0x0006, ASCII bytes, zero-terminated.
    }

    // 0x001A
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwayPointFSX
    {
        public readonly ushort NumPoints;

        // From here (array of TaxiwayPointElement)
    }
    //HERE
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwayPointElement
    {
        public readonly TaxiwayPointType Type;
        public readonly TaxiwayPointFlag Flag;
        public readonly ushort Padding;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
    }

    public enum TaxiwayPointType : byte
    {
        Normal                  = 0x01,
        HoldShort               = 0x02,
        ILSHoldShort            = 0x04,
        HoldShortNoDraw         = 0x05,
        ILSHoldShortNoDraw      = 0x06,
    }

    public enum TaxiwayPointFlag : byte
    {
        Forward = 0x00,
        Reverse = 0x01,
    }

    // 0x001C
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwayPath
    {
        public readonly ushort NumPaths;

        // here start the actual Paths (array of TaxiwayPathRecord)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwayPathRecord
    {
        public readonly ushort StartIndex;
        public readonly ushort EndIndexAndDesignator;
        public readonly byte Flags;                 // See enum TaxiwayPathFlags
        public readonly byte Number;
        public readonly byte LiningAndLighting;     // See enum LiningAndLighting
        public readonly byte Surface;               // See enum RunwaySurfaceType
        public readonly float Width;
        public readonly float WeightLimit;
        public readonly uint Unknown;
    }

    public enum TaxiwayPathFlags : byte
    {
        TypeMask                    = 0b00011111,
        TypeTaxi                    = 0x01,
        TypeRunway                  = 0x02,
        TypeParking                 = 0x03,
        TypePath                    = 0x04,
        TypeClosed                  = 0x05,
        TypeVehicle                 = 0x06,

        DrawSurface                 = 0b00100000,
        DrawDetail                  = 0b01000000,
    }

    public enum LiningAndLighting : byte
    {
        CenterMask                  = 0b00000011,
        CenterLine                  = 0b00000001,
        CenterLighted               = 0b00000010,

        LeftEdgeMask                = 0b00011100,
        LeftEdgeSolid               = 0b00000100,
        LeftEdgeDashed              = 0b00001000,
        LeftEdgeLighted             = 0b00010000,

        RightEdgeMask               = 0b11100000,
        RightEdgeSolid              = 0b00100000,
        RightEdgeDashed             = 0b01000000,
        RightEdgeLighted            = 0b10000000,
    }

    // 0x001D
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiName
    {
        public const uint HeaderSize = 2;

        public readonly ushort NumNames;

        // Names start here, zero-terminated strings.
    }

    // 0x0022
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct WayPoint
    {
        public readonly WayPointType Type;
        public readonly byte NumRouteEntries;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly float MagneticVariance;
        public readonly uint Ident;
        public readonly uint RegionICAO;
        public readonly RouteType RouteType;
        public readonly byte RouteName0, RouteName1, RouteName2, RouteName3, RouteName4, RouteName5, RouteName6, RouteName7;

        public readonly uint NextType;
        public readonly uint NextRegionICAO;
        public readonly float MinAltitude;

        public readonly uint PrevType;
        public readonly uint PrevRegionICAO;
        public readonly float PrevAltitude;

        // If Route given, skip to next DWORD boundary.

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
    }

    public enum WayPointType : byte
    {
        Named                       = 0x01,
        Unnamed                     = 0x02,
        VOR                         = 0x03,
        NDB                         = 0x04,
        OffRoute                    = 0x05,
        IAF                         = 0x06,
        FAF                         = 0x07,
    }

    public enum NextWayPointType : uint
    {
        TypeMask                    = 0x0007,
        Unnamed                     = 0x0000,
        NDB                         = 0x0002,
        VOR                         = 0x0003,
        Named                       = 0x0005,

        IdentMask                   = 0xFFF8,
    }

    public enum RouteType : byte
    {
        Victor                      = 0x01,
        Jet                         = 0x02,
        Both                        = 0x03,
    }

    // 0x0024
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Approach
    {
        public readonly byte Suffix;
        public readonly byte ApproachFlags;                  // See enum ApproachFlags
        public readonly byte NumTransitions;
        public readonly byte NumApproachLegs;
        public readonly byte NumMissedApproachLegs;
        public readonly uint FixFlags;
        public readonly uint RegionAndICAO;
        public readonly float Altitude;
        public readonly float Heading;
        public readonly float MissedAltitude;
    }

    public enum ApproachFlags : byte
    {
        TypeMask                = 0b0000_1111,
        GPS                     = 0x01,
        VOR                     = 0x02,
        NDB                     = 0x03,
        ILS                     = 0x04,
        Localizer               = 0x05,
        SDF                     = 0x06,
        LDA                     = 0x07,
        VORDME                  = 0x08,
        NDBDME                  = 0x09,
        RNAV                    = 0x0A,
        LocalizerBackCourse     = 0x0B,

        DesignatorMask          = 0b0111_0000,
        GPSOverlay              = 0b1000_0000,
    }

    public enum FixFlags : uint
    {
        TypeMask                = 0b00000000_00000000_00000000_00011111,
        Airport                 = 0x00000001,
        VOR                     = 0x00000002,
        NDB                     = 0x00000003,
        TerminalNDB             = 0x00000004,
        Waypoint                = 0x00000005,
        TerminalWaypoint        = 0x00000006,
        Localizer               = 0x00000007,
        Runway                  = 0x00000008,

        IdentMask               = 0b11111111_11111111_11111111_11100000,
    }

    // 0x0026
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct HeliPad
    {
        public readonly byte SurfaceType;           // See enum RunwaySurfaceType
        public readonly byte TypeFlags;             // See enum HeliPadType
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int Elevation;              // In meters, x 1000 (signed!)
        public readonly float Length;               // In meters
        public readonly float Width;                // In meters
        public readonly float Heading;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
    }

    public enum HeliPadType : byte
    {
        TypeMask                = 0b00001111,
        None                    = 0b00000000,
        H                       = 0b00000001,
        Square                  = 0b00000010,
        Circle                  = 0b00000011,
        Medical                 = 0b00000100,

        TransparentMask         = 0b00010000,
        TransparentNo           = 0b00000000,
        TransparentYes          = 0b00010000,

        ClosedMask              = 0b00100000,
        ClosedNo                = 0b00000000,
        ClosedYes               = 0b00100000,
    }

    // 0x002C
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Transition
    {
        public readonly TransitionType Type;
        public readonly byte NumLegs;
        public readonly uint Fix;                       // See enum FixFlags
        public readonly uint RegionICAO;                // Bits 0-10: Region, 11-31: ICAO
        public readonly float Altitude;

        // Optionally followed by DMEArcTransition (type == DME and DMEArc exists, NOTE Decide by size?)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DMEArcTransition
    {
        public readonly uint DMEIdent;
        public readonly uint DMERegionICAO;
        public readonly uint Radial;
        public readonly float Distance;
    }

    public enum TransitionType : byte
    {
        Full = 0x01,
        DME = 0x02,
    }

    // 0x002D
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ApproachLegs
    {
        public readonly ushort NumLegs;

        // Here follow the legs (array of ApproachLegsRecord)
    }

    public struct ApproachLegsRecord
    {
        public readonly LegType Type;
        public readonly LegAltitude AltitudeDescriptor;
        public readonly ushort Flags;                   // See enum ApproachLegFlags
        public readonly uint Fix;                       // Bits 0-4: enum FixFlags, 5-31: Ident
        public readonly uint RegionICAO;                // Bits 0-10: Region, 11-31: ICAO
        public readonly uint RecommendedFix;            // Bits 0-4: Type, 5-31: Ident
        public readonly uint RecommendedRegionICAO;     // Bits 0-10: Region, 11-31: ICAO
        public readonly float Theta;
        public readonly float Rho;
        public readonly float Course;                   // See Flags for course type.
        public readonly float DistanceTime;
        public readonly float Altitude1;
        public readonly float Altitude2;
    }

    public enum LegType : byte
    {
        AF = 0X01,
        CA = 0X02,
        CD = 0X03,
        CF = 0X04,
        CI = 0X05,
        CR = 0X06,
        DF = 0X07,
        FA = 0X08,
        FC = 0X09,
        FD = 0X0A,
        FM = 0X0B,
        HA = 0X0C,
        HF = 0X0D,
        HM = 0X0E,
        IF = 0X0F,
        PI = 0X10,
        RF = 0X11,
        TF = 0X12,
        VA = 0X13,
        VD = 0X14,
        VI = 0X15,
        VM = 0X16,
        VR = 0X17,
    }

    public enum LegAltitude : byte
    {
        Above                       = 0x01,
        Plus                        = 0x02,
        Minus                       = 0x03,
        Below                       = 0x04,
    }

    public enum ApproachLegFlags : ushort
    {
        TurnLeft                    = 0x0001,
        TurnRight                   = 0x0002,

        CourseMask                  = 0x0100,
        CourseMagnetic              = 0x0000,
        CourseTrue                  = 0x0100,

        FlyOverMask                 = 0x0200,
        FlyOverFalse                = 0x0000,
        FlyOverTrue                 = 0x0200,
    }

    // 0x002E
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MissedApproachLegs
    {
        public readonly ushort NumLegs;

        // Here follow the legs (array of ApproachLegsRecord)
    }

    // 0x002F
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TransitionLegs
    {
        public readonly ushort NumLegs;

        // Here follow the legs (array of ApproachLegsRecord)
    }

    // 0x0030
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ApronDetail
    {
        public readonly byte SurfaceType;           // See enum RunwaySurfaceType;
        public readonly byte DrawFlags;             // Bit 0: Draw surface, bit 1: draw detail.
        public readonly ushort NumVertices;
        public readonly ushort NumTriangles;

        // Then, first the vertices, (array of Vertex)
        // Followed by the triangles (array of SurfaceTriangle)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SurfaceTriangle
    {
        public ushort IndexPoint0;
        public ushort IndexPoint1;
        public ushort IndexPoint2;
    }

    // 0x0031
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ApronEdgeLights
    {
        public readonly ushort Unknown0;            // Always 0x0001?
        public readonly ushort NumVertices;
        public readonly ushort NumEdges;
        public readonly uint LightColor;            // BGRA (default blue, not transparent: 0xFF0000FF)
        public readonly float Intensity;            // Default 1.0f
        public readonly float RenderMaxAltitude;    // Default 800.0f, in meters

        // Then, the vertices (array of Vertex)
        // Followed by the edges (array of PointSpacing)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PointSpacing
    {
        public readonly float Distance;             // Default 60.96f, in meters
        public readonly ushort IndexPoint0;
        public readonly ushort IndexPoint1;
    }

    // 0x0032
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AirportSummary
    {
        public readonly ushort AvailabilityFlags;   // See enum ComAvailability
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;
        public readonly int Elevation;              // in meters x 1000
        public readonly uint EncodedICAO;
        public readonly uint EncodedRegion;
        public readonly float MagneticVariance;
        public readonly float LengthLongestRunway;
        public readonly float HeadingLongestRunway;
        public readonly uint FuelAvailability;

        public double Longitude => DecodeLongitude(EncodedLongitude);
        public double Latitude => DecodeLatitude(EncodedLatitude);

        public FuelAvailability Has73Octane => (FuelAvailability)(FuelAvailability & 0x03);
        public FuelAvailability Has87Octane => (FuelAvailability)((FuelAvailability >> 2) & 0x03);
        public FuelAvailability Has100Octane => (FuelAvailability)((FuelAvailability >> 4) & 0x03);
        public FuelAvailability Has130Octane => (FuelAvailability)((FuelAvailability >> 6) & 0x03);
        public FuelAvailability Has145Octane => (FuelAvailability)((FuelAvailability >> 8) & 0x03);
        public FuelAvailability HasMOGAS => (FuelAvailability)((FuelAvailability >> 10) & 0x03);
        public FuelAvailability HasJET => (FuelAvailability)((FuelAvailability >> 12) & 0x03);
        public FuelAvailability HasJETA => (FuelAvailability)((FuelAvailability >> 14) & 0x03);
        public FuelAvailability HasJETA1 => (FuelAvailability)((FuelAvailability >> 16) & 0x03);
        public FuelAvailability HasJETAP => (FuelAvailability)((FuelAvailability >> 18) & 0x03);
        public FuelAvailability HasJETB => (FuelAvailability)((FuelAvailability >> 20) & 0x03);
        public FuelAvailability HasJET4 => (FuelAvailability)((FuelAvailability >> 22) & 0x03);
        public FuelAvailability HasJET4b => (FuelAvailability)((FuelAvailability >> 24) & 0x03);
        public bool HasAVGas => (FuelAvailability & 0x4000) != 0;
        public bool HasJetFuel => (FuelAvailability & 0x8000) != 0;
    }

    public enum AvailabilityFlags : ushort
    {
        Tower                       = 0b0000_0000_0000_0001,
        AsphaltConcrete             = 0b0000_0000_0000_0010,
        WaterOnly                   = 0b0000_0000_0000_0100,
        GPSApproach                 = 0b0000_0000_0010_0000,
        VORApproach                 = 0b0000_0000_0100_0000,
        NDBApproach                 = 0b0000_0000_1000_0000,
        ILSApproach                 = 0b0000_0001_0000_0000,
        LOCApproach                 = 0b0000_0010_0000_0000,
        SDFApproach                 = 0b0000_0100_0000_0000,
        LDAApproach                 = 0b0000_1000_0000_0000,
        VORDMEApproach              = 0b0001_0000_0000_0000,
        NDBDMEVApproach             = 0b0010_0000_0000_0000,
        RNAVApproach                = 0b0100_0000_0000_0000,
        LOCBCApproach               = 0b1000_0000_0000_0000,
    }

    public enum FuelAvailability
    {
        No                          = 0x00,
        Unknown                     = 0x01,
        PriorRequest                = 0x02,
        Yes                         = 0x03,
    }

    // 0x0033
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeleteAirport
    {
        public readonly ushort DeleteFlags;         // See enum DeleteAirportFlags
        public readonly byte NumRunways;
        public readonly byte NumStarts;
        public readonly byte NumFrequencies;
        public readonly byte Unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeleteAirportRunway
    {
        public const uint Size = 4;

        public readonly byte SurfaceType;           // See enum RunwaySurfaceType
        public readonly byte PrimaryNumber;
        public readonly byte SecondaryNumber;
        public readonly byte Designators;           // See enum RunwayDesignator, primary = (Designators & 0x0F), secondary = ((Designators >> 4) & 0x0F)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeleteAirportStart
    {
        public const uint Size = 4;

        public readonly byte Number;
        public readonly byte Designator;            // See enum RunwayDesignator
        public readonly byte Type;                  // See enum RunwayStartType
        public readonly byte Unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeleteAirportFrequency
    {
        public const uint Size = 4;

        public readonly uint Frequency;             // ((Frequency >> 28) & 0x000F) see enum ComType, (Frequency & 0x0FFFFFFF) frequency x 1000000
    }

    // 0x0037
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ApronSurface
    {
        public readonly byte SurfaceType;           // See enum RunwaySurfaceType
        public readonly ushort NumVertices;

        // Vertices start here (array of Vertex)
    }

    // 0x0038 BlastFence
    // 0x0039 BoundaryFence
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Fence
    {
        public readonly ushort NumVertices;
        public readonly ulong InstanceId;           // GUID
        public readonly ulong Profile;              // GUID

        // Here follow the vertices (array of Vertex)
    }

    // 0x003A
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Jetway
    {
        public readonly ushort ParkingNumber;
        public readonly ushort GateName;
        public readonly uint ObjectSize;

        // LibraryObject record for the Jetway starts here.
    }

    // 0x003B
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AirportArea
    {
        public readonly ushort Unknown;
        public readonly ushort NumVertices;
        public readonly ushort NumTriangles;

        // Here start the vertices (array of Vertex)
        // Then the triangles (array of Triangle)
    }

    // 0x003D
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwayParkingFSX
    {
        public const uint RecordSize = 2;

        public readonly ushort NumParkings;

        // From here (array of TaxiWayParkingRecordFSX)
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TaxiwayParkingRecordFSX
    {
        public const uint RecordSize = 36;

        public readonly uint Info;
        public readonly float Radius;
        public readonly float Heading;
        public readonly float teeOffset1;
        public readonly float teeOffset2;
        public readonly float teeOffset3;
        public readonly float teeOffset4;
        public readonly int EncodedLongitude;
        public readonly int EncodedLatitude;

        public ParkingName Name                 => (ParkingName)(Info & 0x0000003F);
        public PushbackType PushbackType        => (PushbackType)(Info & 0x000000C0);
        public ParkingType Type                 => (ParkingType)(Info & 0x00000F00);
        public uint Number                      => (Info & 0x00FFF000) >> 12;
        public uint NumAirlineDesignators       => (Info & 0xFF000000) >> 24;
        public double Longitude                 => DecodeLongitude(EncodedLongitude);
        public double Latitude                  => DecodeLatitude(EncodedLatitude);

        // From here the Airline Designators
    }

    public enum ParkingName : uint
    {
        NameNone            = 0x00000000,
        Parking             = 0x00000001,
        N_Parking,
        NE_Parking,
        E_Parking,
        SE_Parking,
        S_Parking,
        SW_Parking,
        W_Parking,
        NW_Parking,
        Gate,
        Dock,
        Gate_A,
        Gate_B,
        Gate_C,
        Gate_D,
        Gate_E,
        Gate_F,
        Gate_G,
        Gate_H,
        Gate_I,
        Gate_J,
        Gate_K,
        Gate_L,
        Gate_M,
        Gate_N,
        Gate_O,
        Gate_P,
        Gate_Q,
        Gate_R,
        Gate_S,
        Gate_T,
        Gate_U,
        Gate_V,
        Gate_W,
        Gate_X,
        Gate_Y,
        Gate_Z              = 0x00000025,
    }

    public enum PushbackType : uint
    {
        None = 0,
        Left = 0x40,
        Right = 0x80,
        Both = 0xC0,
    }

    public enum ParkingType : uint
    {
        RampGA = 0x00000100,
        RampGASmall = 0x00000200,
        RampGAMedium = 0x00000300,
        RampGALarge = 0x00000400,
        RampCargo = 0x00000500,
        RampMilCargo = 0x00000600,
        RampMilCombat = 0x00000700,
        GateSmall = 0x00000800,
        GateMedium = 0x00000900,
        GateHeavy = 0x00000A00,
        DockGA = 0x00000B00,
        Fuel = 0x00000C00,
        Vehicles = 0x00000D00,
    }
    // 0x0066
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TowerSceneryObject
    {
        public readonly uint ScenerySize;

        // Actual Scenery object starts here
    }


    // 0x00A0
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TACAN
    {
        public readonly int Longitude;
        public readonly int Latitude;
        public readonly int Elevation;              // In meters, x 1000 (signed!)
        public readonly uint Channel;
        public readonly byte Type;
        public readonly float Range;                // In meters
        public readonly float MagneticVariance;
        public readonly uint EncodedICAO;
        public readonly uint RegionIdent;
        public readonly byte Unknown;

        // Optional Name following

        public bool IsXType         => (Type & 0b00000001) == 0;
        public bool IsYType         => (Type & 0b00000001) != 0;
        public bool IsDMEOnly       => (Type & 0b00000010) != 0;
        public string ICAO          => BglAirport.DecodeName(EncodedICAO, false);
    }


    public static class BglRecord
    {

        public static string DecodeName(uint encoded, bool shift5 = true)
        {
            string result = "";

            if (shift5)
            {
                encoded >>= 5;
            }
            while (encoded != 0)
            {
                uint oneChar = encoded % 38;
                if (oneChar == 0)
                {
                    result = " " + result;
                }
                else if ((oneChar >= 2) && (oneChar <= 11))
                {
                    result = ((char)('0' + oneChar - 2)) + result;
                }
                else if ((oneChar >= 12) && (oneChar <= 37))
                {
                    result = ((char)('A' + oneChar - 12)) + result;
                }
                encoded /= 38;
            }
            return result;
        }

        public static double DecodeLongitude(int encoded) => -180.0 + (encoded * (360.0 / (3 * 0x10000000)));
        public static double DecodeLatitude(int encoded) => 90.0 - encoded * (180.0 / (2 * 0x10000000));
        public static double DecodeElevation(int encoded) => encoded / 1000.0;
        public static double DecodeElevationToFeet(int encoded) => encoded * 03.281;
    }
}