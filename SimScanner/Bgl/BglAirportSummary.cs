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

namespace SimScanner.Bgl
{
    public struct BglMSFSAirportSummaryHeader
    {
        public const uint Size = 0x2C;

        public ushort Id;
        public uint TotalSize;
        public ushort ApproachAvailability;
        public int Longitude;
        public int Latitude;
        public int Altitude;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public uint Unknown0;
        public float MagneticVariance;
        public float LongestRunwayLength;
        public float LongestRunwayHeading;
        public uint FuelAvailability;
    }

    public class BglAirportSummary
    {
        private static readonly Logger log = Logger.GetLogger(typeof(BglAirportSummary));

        internal BglSubSection subSection;

        private BglMSFSAirportSummaryHeader header;
        public BglMSFSAirportSummaryHeader Header => header;

        public string ICAO => BglAirport.DecodeName(Header.EncodedICAO);
        public string RegionCode => BglAirport.DecodeName(Header.EncodedRegionIdent, false);
        public double Latitude => 90.0 - Header.Latitude * (180.0 / (2 * 0x10000000));
        public double Longitude => -180.0 + (Header.Longitude * (360.0 / (3 * 0x10000000)));
        public double Altitude => Header.Altitude / 1000.0;

        internal BglAirportSummary(BglSubSection subSection)
        {
            this.subSection = subSection;

            using var reader = subSection.section.file.MappedFile.Section(subSection.DataOffset, subSection.DataSize);

            reader
                .Read(out header.Id)
                .Read(out header.TotalSize)
                .Read(out header.ApproachAvailability)
                .Read(out header.Longitude)
                .Read(out header.Latitude)
                .Read(out header.Altitude)
                .Read(out header.EncodedICAO)
                .Read(out header.EncodedRegionIdent)
                .Read(out header.Unknown0)
                .Read(out header.MagneticVariance)
                .Read(out header.LongestRunwayLength)
                .Read(out header.LongestRunwayHeading)
                .Read(out header.FuelAvailability);
        }
    }
}
