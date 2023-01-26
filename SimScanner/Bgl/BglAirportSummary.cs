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
using System.Runtime.InteropServices;

using static SimScanner.Bgl.BglRecord;

namespace SimScanner.Bgl
{
    public class BglAirportSummary
    {
        private static readonly ILogger log = Logger.GetLogger(typeof(BglAirportSummary));

        internal BglSubSection subSection;

        public virtual string ICAO => null;
        public virtual string RegionCode => null;
        public virtual double Latitude => 0.0;
        public virtual double Longitude => 0.0;
        public virtual double Elevation => 0.0;
        public virtual double ElevationInFeet => 0.0;

        internal BglAirportSummary(BglSubSection subSection)
        {
            this.subSection = subSection;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BglFSXAirportSummaryHeader
    {
        public const uint Size = 0x2C;

        public ushort Id;
        public uint TotalSize;
        public ushort ApproachAvailability;
        public int Longitude;
        public int Latitude;
        public int Elevation;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public float MagneticVariance;
        public float LongestRunwayLength;
        public float LongestRunwayHeading;
        public uint FuelAvailability;
    }

    public class BglFSXAirportSummary : BglAirportSummary
    {

        private BglFSXAirportSummaryHeader header;
        public BglFSXAirportSummaryHeader Header => header;

        public override string ICAO => DecodeName(Header.EncodedICAO);
        public override string RegionCode => DecodeName(Header.EncodedRegionIdent, false);
        public override double Latitude => DecodeLatitude(Header.Latitude);
        public override double Longitude => DecodeLongitude(Header.Longitude);
        public override double Elevation => DecodeElevation(Header.Elevation);
        public override double ElevationInFeet => DecodeElevationToFeet(Header.Elevation);

        internal BglFSXAirportSummary(BglSubSection subSection, long pos) : base(subSection)
        {
            using var reader = subSection.section.file.MappedFile.Section(subSection.DataOffset, subSection.DataSize);

            reader.Seek(pos).Read(out header, BglFSXAirportSummaryHeader.Size);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BglP3DAirportSummaryHeader
    {
        public const uint Size = 0x3C; // !

        public ushort Id;
        public uint TotalSize;
        public ushort ApproachAvailability;
        public int Longitude;
        public int Latitude;
        public int Elevation;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public float MagneticVariance;
        public float LongestRunwayLength;
        public float LongestRunwayHeading;
        public uint FuelAvailability;
        public ulong Unknown1, Unknown2;
    }

    public class BglP3DAirportSummary : BglAirportSummary
    {

        private BglP3DAirportSummaryHeader header;
        public BglP3DAirportSummaryHeader Header => header;

        public override string ICAO => DecodeName(Header.EncodedICAO);
        public override string RegionCode => DecodeName(Header.EncodedRegionIdent, false);
        public override double Latitude => DecodeLatitude(Header.Latitude);
        public override double Longitude => DecodeLongitude(Header.Longitude);
        public override double Elevation => DecodeElevation(Header.Elevation);
        public override double ElevationInFeet => DecodeElevationToFeet(Header.Elevation);

        internal BglP3DAirportSummary(BglSubSection subSection, long pos) : base(subSection)
        {
            using var reader = subSection.section.file.MappedFile.Section(subSection.DataOffset, subSection.DataSize);

            reader.Seek(pos).Read(out header, BglP3DAirportSummaryHeader.Size);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BglMSFSAirportSummaryHeader
    {
        public const uint Size = 0x2C;

        public ushort Id;
        public uint TotalSize;
        public ushort ApproachAvailability;
        public int Longitude;
        public int Latitude;
        public int Elevation;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public float MagneticVariance;
        public float LongestRunwayLength;
        public float LongestRunwayHeading;
        public uint FuelAvailability;
    }

    public class BglMSFSAirportSummary : BglAirportSummary
    {

        private BglMSFSAirportSummaryHeader header;
        public BglMSFSAirportSummaryHeader Header => header;

        public override string ICAO => DecodeName(Header.EncodedICAO);
        public override string RegionCode => DecodeName(Header.EncodedRegionIdent, false);
        public override double Latitude => DecodeLatitude(Header.Latitude);
        public override double Longitude => DecodeLongitude(Header.Longitude);
        public override double Elevation => DecodeElevation(Header.Elevation);
        public override double ElevationInFeet => DecodeElevationToFeet(Header.Elevation);

        internal BglMSFSAirportSummary(BglSubSection subSection, long pos) : base(subSection)
        {
            using var reader = subSection.section.file.MappedFile.Section(subSection.DataOffset, subSection.DataSize);

            reader.Seek(pos).Read(out header, BglMSFSAirportSummaryHeader.Size);
        }
    }
}
