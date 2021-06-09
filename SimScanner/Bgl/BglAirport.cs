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

namespace SimScanner.Bgl
{
    public struct BglAirportHeader
    {
        public const uint Size = 0x38;

        public ushort Id;
        public uint TotalSize;
        public byte NumRunways;
        public byte NumComs;
        public byte NumStarts;
        public byte NumApproaches;
        public byte EncodedNumAprons;
        public byte NumHeliPads;
        public uint Longitude;
        public uint Latitude;
        public uint Altitude;
        public uint TowerLongitude;
        public uint TowerLatitude;
        public uint TowerAltitude;
        public float MagneticVariance;
        public uint EncodedICAO;
        public uint EncodedRegionIdent;
        public uint FuelAvailability;

        public int NumAprons => EncodedNumAprons & 0x7f;
        public bool Deleted => (EncodedNumAprons & 0x80) != 0;
    }

    public class BglAirport
    {
        internal BglSubSection subSection;
        private BglAirportHeader header;
        public BglAirportHeader Header => header;

        public string ICAO => DecodeName(header.EncodedICAO);
        public string RegionCode => DecodeName(header.EncodedRegionIdent, false);


        internal BglAirport(BglSubSection subSection)
        {
            this.subSection = subSection;
            // With 64-bits architectures we have alignment issues
            BglFile file = subSection.section.file;
            long pos = subSection.DataOffset;

            using var reader = subSection.section.file.File.Section(subSection.DataOffset, subSection.DataSize);

            reader
                .Read(out header.Id)
                .Read(out header.TotalSize, sizeof(uint))
                .Read(out header.NumRunways, sizeof(byte))
                .Read(out header.NumComs, sizeof(byte))
                .Read(out header.NumStarts, sizeof(byte))
                .Read(out header.NumApproaches, sizeof(byte))
                .Read(out header.EncodedNumAprons, sizeof(byte))
                .Read(out header.NumHeliPads, sizeof(byte))
                .Read(out header.Longitude, sizeof(uint))
                .Read(out header.Latitude, sizeof(uint))
                .Read(out header.Altitude, sizeof(uint))
                .Read(out header.TowerLongitude, sizeof(uint))
                .Read(out header.TowerLatitude, sizeof(uint))
                .Read(out header.TowerAltitude, sizeof(uint))
                .Read(out header.MagneticVariance, sizeof(float))
                .Read(out header.EncodedICAO, sizeof(uint))
                .Read(out header.EncodedRegionIdent, sizeof(uint))
                .Read(out header.FuelAvailability, sizeof(uint));
        }

        public string DecodeName(uint encoded, bool shift5 =true)
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
    }
}
